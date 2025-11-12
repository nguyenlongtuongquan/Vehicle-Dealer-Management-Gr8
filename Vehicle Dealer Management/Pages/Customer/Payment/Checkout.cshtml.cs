using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.BLL.IService;
using Vehicle_Dealer_Management.DAL.Constants;
using Vehicle_Dealer_Management.DAL.Data;

namespace Vehicle_Dealer_Management.Pages.Customer.Payment
{
    public class CheckoutModel : PageModel
    {
        private readonly ISalesDocumentService _salesDocumentService;
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly IPaymentService _paymentService;
        private readonly IContractService _contractService;
        private readonly ApplicationDbContext _context;

        public CheckoutModel(
            ISalesDocumentService salesDocumentService,
            IPaymentGatewayService paymentGatewayService,
            IPaymentService paymentService,
            IContractService contractService,
            ApplicationDbContext context)
        {
            _salesDocumentService = salesDocumentService;
            _paymentGatewayService = paymentGatewayService;
            _paymentService = paymentService;
            _contractService = contractService;
            _context = context;
        }

        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string? ErrorMessage { get; set; }
        public bool CanPayCash { get; set; }

        public async Task<IActionResult> OnGetAsync(int orderId, string? provider)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole") ?? "CUSTOMER";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "Customer";

            var userIdInt = int.Parse(userId);
            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == userIdInt);

            if (customer == null)
            {
                return RedirectToPage("/Auth/Profile");
            }

            var order = await _salesDocumentService.GetSalesDocumentWithDetailsAsync(orderId);
            if (order == null || order.CustomerId != customer.Id || order.Type != "ORDER")
            {
                return NotFound();
            }

            var contract = await _contractService.GetContractByOrderIdAsync(orderId);
            if (contract == null || !SalesContractStatus.IsSigned(contract.Status))
            {
                TempData["Error"] = "Đơn hàng chưa có hợp đồng hợp lệ. Vui lòng hoàn tất ký hợp đồng trước khi thanh toán.";
                return RedirectToPage("/Customer/OrderDetail", new { id = orderId });
            }
            CanPayCash = true;

            var totalAmount = order.Lines?.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue) ?? 0;
            var paidAmount = await _paymentService.GetTotalPaidAmountAsync(orderId);
            var remainingAmount = totalAmount - paidAmount;

            OrderId = orderId;
            OrderNumber = $"ORD-{orderId:D6}";
            TotalAmount = totalAmount;
            RemainingAmount = remainingAmount;

            if (remainingAmount <= 0)
            {
                TempData["Error"] = "Đơn hàng này đã được thanh toán đủ.";
                return RedirectToPage("/Customer/OrderDetail", new { id = orderId });
            }

            // Nếu có provider thì redirect đến payment gateway
            if (!string.IsNullOrEmpty(provider))
            {
                return await ProcessPaymentAsync(orderId, provider, remainingAmount);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostProcessPaymentAsync(int orderId, string provider)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var userIdInt = int.Parse(userId);
            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == userIdInt);

            if (customer == null)
            {
                return RedirectToPage("/Auth/Profile");
            }

            var order = await _salesDocumentService.GetSalesDocumentWithDetailsAsync(orderId);
            if (order == null || order.CustomerId != customer.Id || order.Type != "ORDER")
            {
                return NotFound();
            }

            var contract = await _contractService.GetContractByOrderIdAsync(orderId);
            if (contract == null || !SalesContractStatus.IsSigned(contract.Status))
            {
                TempData["Error"] = "Đơn hàng chưa có hợp đồng hợp lệ. Vui lòng hoàn tất ký hợp đồng trước khi thanh toán.";
                return RedirectToPage("/Customer/OrderDetail", new { id = orderId });
            }

            var totalAmount = order.Lines?.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue) ?? 0;
            var paidAmount = await _paymentService.GetTotalPaidAmountAsync(orderId);
            var remainingAmount = totalAmount - paidAmount;

            if (remainingAmount <= 0)
            {
                TempData["Error"] = "Đơn hàng này đã được thanh toán đủ.";
                return RedirectToPage("/Customer/OrderDetail", new { id = orderId });
            }

            return await ProcessPaymentAsync(orderId, provider, remainingAmount);
        }

        private async Task<IActionResult> ProcessPaymentAsync(int orderId, string provider, decimal amount)
        {
            try
            {
                var orderNumber = $"ORD-{orderId:D6}";
                var orderInfo = $"Thanh toan don hang {orderNumber}";
                string paymentUrl;

                if (provider.Equals("momo", StringComparison.OrdinalIgnoreCase))
                {
                    paymentUrl = await _paymentGatewayService.CreateMoMoPaymentUrlAsync(
                        orderId,
                        amount,
                        $"/Customer/Payment/Callback?provider=momo&orderId={orderId}",
                        $"/Customer/Payment/IPN?provider=momo",
                        orderInfo
                    );
                }
                else if (provider.Equals("momo_atm", StringComparison.OrdinalIgnoreCase))
                {
                    paymentUrl = await _paymentGatewayService.CreateMoMoPaymentUrlAsync(
                        orderId,
                        amount,
                        $"/Customer/Payment/Callback?provider=momo_atm&orderId={orderId}",
                        $"/Customer/Payment/IPN?provider=momo_atm",
                        orderInfo + " - Napas ATM",
                        MoMoPaymentMethod.ATM
                    );
                }
                else if (provider.Equals("VNPAY", StringComparison.OrdinalIgnoreCase))
                {
                    paymentUrl = await _paymentGatewayService.CreateVNPayPaymentUrlAsync(
                        orderId,
                        amount,
                        $"/Customer/Payment/Callback?provider=vnpay&orderId={orderId}",
                        $"/Customer/Payment/IPN?provider=vnpay",
                        orderInfo,
                        HttpContext
                    );
                }
                else
                {
                TempData["Error"] = "Phương thức thanh toán không hợp lệ.";
                    return RedirectToPage(new { orderId });
                }

                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tạo thanh toán: {ex.Message}";
                return RedirectToPage(new { orderId });
            }
        }

        public async Task<IActionResult> OnPostPayCashAsync(int orderId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var userIdInt = int.Parse(userId);
            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == userIdInt);

            if (customer == null)
            {
                return RedirectToPage("/Auth/Profile");
            }

            var order = await _salesDocumentService.GetSalesDocumentWithDetailsAsync(orderId);
            if (order == null || order.CustomerId != customer.Id || order.Type != "ORDER")
            {
                return NotFound();
            }

            var contract = await _contractService.GetContractByOrderIdAsync(orderId);
            if (contract == null || !SalesContractStatus.IsSigned(contract.Status))
            {
                TempData["Error"] = "Đơn hàng chưa có hợp đồng hợp lệ. Vui lòng hoàn tất ký hợp đồng trước khi thanh toán tiền mặt.";
                return RedirectToPage("/Customer/OrderDetail", new { id = orderId });
            }

            var totalAmount = order.Lines?.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue) ?? 0;
            var paidAmount = await _paymentService.GetTotalPaidAmountAsync(orderId);
            var remainingAmount = totalAmount - paidAmount;

            if (remainingAmount <= 0)
            {
                TempData["Error"] = "Đơn hàng này đã được thanh toán đủ.";
                return RedirectToPage("/Customer/OrderDetail", new { id = orderId });
            }

            try
            {
                var metaJson = "{\"Provider\":\"CASH\",\"Note\":\"Khách hàng xác nhận thanh toán tiền mặt tại đại lý\"}";
                await _paymentService.CreatePaymentAsync(orderId, "CASH", remainingAmount, metaJson);
                TempData["Success"] = $"Đã ghi nhận thanh toán tiền mặt {remainingAmount:N0} VND. Vui lòng hoàn tất tại đại lý.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi ghi nhận thanh toán tiền mặt: {ex.Message}";
            }

            return RedirectToPage("/Customer/OrderDetail", new { id = orderId });
        }
    }
}


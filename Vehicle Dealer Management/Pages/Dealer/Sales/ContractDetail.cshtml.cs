using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.Pages.Dealer.Sales
{
    public class ContractDetailModel : PageModel
    {
        private readonly ISalesDocumentService _salesDocumentService;
        private readonly IPaymentService _paymentService;
        private readonly IDeliveryService _deliveryService;

        public ContractDetailModel(
            ISalesDocumentService salesDocumentService,
            IPaymentService paymentService,
            IDeliveryService deliveryService)
        {
            _salesDocumentService = salesDocumentService;
            _paymentService = paymentService;
            _deliveryService = deliveryService;
        }

        public ContractDetailViewModel Contract { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole") ?? "DEALER_STAFF";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "User";

            var dealerIdInt = int.Parse(dealerId);

            var contract = await _salesDocumentService.GetSalesDocumentWithDetailsAsync(id);

            if (contract == null || contract.DealerId != dealerIdInt || contract.Type != "CONTRACT")
            {
                return NotFound();
            }

            var totalAmount = contract.Lines?.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue) ?? 0;

            Contract = new ContractDetailViewModel
            {
                Id = contract.Id,
                ContractNumber = $"CTR-{contract.Id:D6}",
                Status = contract.Status,
                SignedAt = contract.SignedAt ?? contract.CreatedAt,
                CreatedAt = contract.CreatedAt,

                CustomerName = contract.Customer?.FullName ?? "N/A",
                CustomerPhone = contract.Customer?.Phone ?? "N/A",
                CustomerEmail = contract.Customer?.Email ?? "N/A",
                CustomerAddress = contract.Customer?.Address ?? "N/A",

                DealerName = contract.Dealer?.Name ?? "N/A",
                DealerAddress = contract.Dealer?.Address ?? "N/A",

                CreatedBy = contract.CreatedByUser?.FullName ?? "N/A",

                PromotionName = contract.Promotion?.Name,

                Items = contract.Lines?.Select(l => new ContractItemViewModel
                {
                    VehicleModel = l.Vehicle?.ModelName ?? "N/A",
                    VehicleVariant = l.Vehicle?.VariantName ?? "N/A",
                    ColorCode = l.ColorCode,
                    Qty = l.Qty,
                    UnitPrice = l.UnitPrice,
                    DiscountValue = l.DiscountValue,
                    LineTotal = l.UnitPrice * l.Qty - l.DiscountValue,
                    VehicleImageUrl = l.Vehicle?.ImageUrl
                }).ToList() ?? new List<ContractItemViewModel>(),

                TotalAmount = totalAmount
            };

            return Page();
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var dealerIdInt = int.Parse(dealerId);
            var contract = await _salesDocumentService.GetSalesDocumentWithDetailsAsync(id);

            if (contract == null || contract.DealerId != dealerIdInt || contract.Type != "CONTRACT")
            {
                return NotFound();
            }

            await _salesDocumentService.UpdateSalesDocumentStatusAsync(id, "COMPLETED");

            TempData["Success"] = "H?p ð?ng ð? ðý?c hoàn thành!";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostTerminateAsync(int id, string reason)
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var dealerIdInt = int.Parse(dealerId);
            var contract = await _salesDocumentService.GetSalesDocumentWithDetailsAsync(id);

            if (contract == null || contract.DealerId != dealerIdInt || contract.Type != "CONTRACT")
            {
                return NotFound();
            }

            await _salesDocumentService.UpdateSalesDocumentStatusAsync(id, "TERMINATED");

            TempData["Success"] = $"H?p ð?ng ð? b? h?y. L? do: {reason}";
            return RedirectToPage(new { id });
        }

        public class ContractDetailViewModel
        {
            public int Id { get; set; }
            public string ContractNumber { get; set; } = "";
            public string Status { get; set; } = "";
            public DateTime SignedAt { get; set; }
            public DateTime CreatedAt { get; set; }

            public string CustomerName { get; set; } = "";
            public string CustomerPhone { get; set; } = "";
            public string CustomerEmail { get; set; } = "";
            public string CustomerAddress { get; set; } = "";

            public string DealerName { get; set; } = "";
            public string DealerAddress { get; set; } = "";

            public string CreatedBy { get; set; } = "";

            public string? PromotionName { get; set; }

            public List<ContractItemViewModel> Items { get; set; } = new();

            public decimal TotalAmount { get; set; }
        }

        public class ContractItemViewModel
        {
            public string VehicleModel { get; set; } = "";
            public string VehicleVariant { get; set; } = "";
            public string ColorCode { get; set; } = "";
            public decimal Qty { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal DiscountValue { get; set; }
            public decimal LineTotal { get; set; }
            public string? VehicleImageUrl { get; set; }
        }
    }
}

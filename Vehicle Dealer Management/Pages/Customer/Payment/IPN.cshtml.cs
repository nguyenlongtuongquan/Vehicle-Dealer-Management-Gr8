using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vehicle_Dealer_Management.BLL.IService;
using System.Text;

namespace Vehicle_Dealer_Management.Pages.Customer.Payment
{
    [IgnoreAntiforgeryToken]
    public class IPNModel : PageModel
    {
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly IPaymentService _paymentService;
        private readonly ISalesDocumentService _salesDocumentService;

        public IPNModel(
            IPaymentGatewayService paymentGatewayService,
            IPaymentService paymentService,
            ISalesDocumentService salesDocumentService)
        {
            _paymentGatewayService = paymentGatewayService;
            _paymentService = paymentService;
            _salesDocumentService = salesDocumentService;
        }

        public async Task<IActionResult> OnPostAsync(string? provider)
        {
            try
            {
                // Read request body
                using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                var body = await reader.ReadToEndAsync();

                // Parse callback data
                var callbackData = new Dictionary<string, string>();
                
                if (string.Equals(provider, "MOMO", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(provider, "MOMO_ATM", StringComparison.OrdinalIgnoreCase))
                {
                    // MoMo sends JSON
                    var jsonData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(body);
                    if (jsonData != null)
                    {
                        foreach (var item in jsonData)
                        {
                            callbackData[item.Key] = item.Value?.ToString() ?? "";
                        }
                    }

                    var result = await _paymentGatewayService.ProcessMoMoCallbackAsync(callbackData);
                    if (result.IsSuccess && !string.IsNullOrEmpty(result.OrderId) && result.Amount.HasValue)
                    {
                        var methodCode = string.Equals(provider, "MOMO_ATM", StringComparison.OrdinalIgnoreCase) ? "MOMO_ATM" : "MOMO";
                        await ProcessPaymentAsync(result.OrderId, result.Amount.Value, methodCode, result.TransactionId);
                    }
                }
                else if (provider?.ToUpper() == "VNPAY")
                {
                    // VNPay sends query string format
                    var queryParams = body.Split('&');
                    foreach (var param in queryParams)
                    {
                        var parts = param.Split('=');
                        if (parts.Length == 2)
                        {
                            callbackData[parts[0]] = Uri.UnescapeDataString(parts[1]);
                        }
                    }

                    var result = await _paymentGatewayService.ProcessVNPayCallbackAsync(callbackData);
                    if (result.IsSuccess && !string.IsNullOrEmpty(result.OrderId) && result.Amount.HasValue)
                    {
                        await ProcessPaymentAsync(result.OrderId, result.Amount.Value, "VNPAY", result.TransactionId);
                    }
                }
                // Return success response
                return new OkResult();
            }
            catch (Exception ex)
            {
                // Log error but still return OK to avoid retry
                return new OkResult();
            }
        }

        private async Task ProcessPaymentAsync(string orderIdStr, decimal amount, string method, string? transactionId)
        {
            if (!int.TryParse(orderIdStr, out int orderId) || amount <= 0)
            {
                return;
            }

            try
            {
                var order = await _salesDocumentService.GetSalesDocumentWithDetailsAsync(orderId);
                if (order == null || order.Type != "ORDER")
                {
                    return;
                }

                // Check if payment already exists (avoid duplicate)
                var existingPayments = await _paymentService.GetPaymentsBySalesDocumentIdAsync(orderId);
                if (existingPayments.Any(p => p.Method == method && p.MetaJson?.Contains(transactionId ?? "") == true))
                {
                    return; // Already processed
                }

                // Create payment record
                var metaJson = $"{{\"TransactionId\":\"{transactionId}\",\"Provider\":\"{method}\",\"IPN\":true}}";
                await _paymentService.CreatePaymentAsync(orderId, method, amount, metaJson);
            }
            catch
            {
                // Silent fail for IPN
            }
        }
    }
}


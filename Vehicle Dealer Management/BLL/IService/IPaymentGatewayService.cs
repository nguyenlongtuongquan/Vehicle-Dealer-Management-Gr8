using Microsoft.AspNetCore.Http;

namespace Vehicle_Dealer_Management.BLL.IService
{
    public interface IPaymentGatewayService
    {
        /// <summary>
        /// Tạo payment link từ MoMo
        /// </summary>
        Task<string> CreateMoMoPaymentUrlAsync(int orderId, decimal amount, string returnUrl, string notifyUrl, string orderInfo, MoMoPaymentMethod method = MoMoPaymentMethod.Wallet);

        /// <summary>
        /// Tạo payment link từ VNPay
        /// </summary>
        Task<string> CreateVNPayPaymentUrlAsync(int orderId, decimal amount, string returnUrl, string ipnUrl, string orderDescription, HttpContext? httpContext = null);

        /// <summary>
        /// Xác thực và xử lý callback từ MoMo
        /// </summary>
        Task<MoMoPaymentResult> ProcessMoMoCallbackAsync(Dictionary<string, string> callbackData);

        /// <summary>
        /// Xác thực và xử lý callback từ VNPay
        /// </summary>
        Task<VNPayPaymentResult> ProcessVNPayCallbackAsync(Dictionary<string, string> vnpParams);
    }

    public enum MoMoPaymentMethod
    {
        Wallet,
        ATM
    }

    public class MoMoPaymentResult
    {
        public bool IsSuccess { get; set; }
        public string? OrderId { get; set; }
        public decimal? Amount { get; set; }
        public string? TransactionId { get; set; }
        public string? Message { get; set; }
    }

    public class VNPayPaymentResult
    {
        public bool IsSuccess { get; set; }
        public string? OrderId { get; set; }
        public decimal? Amount { get; set; }
        public string? TransactionId { get; set; }
        public string? ResponseCode { get; set; }
        public string? Message { get; set; }
    }
}


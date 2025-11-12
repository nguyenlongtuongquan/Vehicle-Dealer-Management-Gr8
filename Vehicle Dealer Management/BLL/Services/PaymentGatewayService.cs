using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.BLL.Services
{
    public class PaymentGatewayService : IPaymentGatewayService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public PaymentGatewayService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<string> CreateMoMoPaymentUrlAsync(int orderId, decimal amount, string returnUrl, string notifyUrl, string orderInfo, MoMoPaymentMethod method = MoMoPaymentMethod.Wallet)
        {
            var momoConfig = _configuration.GetSection("PaymentGateways:MoMo");
            var partnerCode = momoConfig["PartnerCode"] ?? "";
            var accessKey = momoConfig["AccessKey"] ?? "";
            var secretKey = momoConfig["SecretKey"] ?? "";
            var paymentUrl = momoConfig["PaymentUrl"] ?? "";
            var isProduction = momoConfig.GetValue<bool>("IsProduction", false);
            var testModeScaleDownEnabled = momoConfig.GetValue<bool>("TestModeScaleDown", true);
            var testModeScaleDownMethod = momoConfig["TestModeScaleDownMethod"] ?? "auto"; // "auto" hoặc "fixed"
            var testModeScaleDownAmount = momoConfig.GetValue<decimal?>("TestModeScaleDownAmount");

            var baseUrl = _configuration["PaymentSettings:BaseUrl"] ?? "https://localhost:7042";
            var fullReturnUrl = $"{baseUrl}{returnUrl}";
            var fullNotifyUrl = $"{baseUrl}{notifyUrl}";

            // Validation cho MoMo sandbox
            const decimal momoWalletMinAmount = 1000m;
            const decimal momoWalletMaxAmount = 50000000m;
            const decimal momoAtmMinAmount = 1000m;
            const decimal momoAtmMaxAmount = 5000000m;
            var sandboxMin = method == MoMoPaymentMethod.ATM ? momoAtmMinAmount : momoWalletMinAmount;
            var sandboxMax = method == MoMoPaymentMethod.ATM ? momoAtmMaxAmount : momoWalletMaxAmount;

            var originalAmount = amount;

            // Kiểm tra nếu là sandbox/test mode
            if (!isProduction)
            {
                if (amount < sandboxMin)
                {
                    throw new ArgumentException($"Số tiền thanh toán ({amount:N0} VND) nhỏ hơn số tiền tối thiểu cho phép là {sandboxMin:N0} VND", nameof(amount));
                }

                if (amount > sandboxMax)
                {
                    // Nếu bật TestModeScaleDown, tự động scale down số tiền để test
                    if (testModeScaleDownEnabled)
                    {
                        if (testModeScaleDownMethod == "auto")
                        {
                            // Tự động scale down theo tỷ lệ: lấy phần nghìn của số tiền gốc
                            // Ví dụ: 2,500,000,000 -> 2500, 1,500,000,000 -> 1500
                            amount = Math.Floor(originalAmount / 1000000m);
                            // Đảm bảo nằm trong khoảng min/max sandbox
                            amount = Math.Max(sandboxMin, Math.Min(amount, sandboxMax));
                        }
                        else if (testModeScaleDownMethod == "fixed" && testModeScaleDownAmount.HasValue)
                        {
                            // Dùng số tiền cố định nếu được config
                            amount = testModeScaleDownAmount.Value;
                        }
                        else
                        {
                            // Fallback: tự động tính theo tỷ lệ
                            amount = Math.Floor(originalAmount / 1000000m);
                            amount = Math.Max(sandboxMin, Math.Min(amount, sandboxMax));
                        }

                        // Đảm bảo số tiền sau khi scale down vẫn >= minAmount
                        if (amount < sandboxMin)
                        {
                            amount = sandboxMin;
                        }
                        
                        // Thêm thông tin vào orderInfo để biết đã scale down
                        orderInfo = $"{orderInfo} [TEST: {originalAmount:N0} -> {amount:N0} VND]";
                    }
                    else
                    {
                        throw new ArgumentException($"Số tiền thanh toán ({amount:N0} VND) lớn hơn số tiền tối đa cho phép trong sandbox là {sandboxMax:N0} VND. " +
                            $"MoMo sandbox chỉ cho phép thanh toán từ {sandboxMin:N0} VND đến {sandboxMax:N0} VND. " +
                            $"Vui lòng thanh toán nhiều lần hoặc bật 'TestModeScaleDown' trong appsettings.json để tự động giảm số tiền khi test.", 
                            nameof(amount));
                    }
                }
            }

            var requestId = Guid.NewGuid().ToString();
            
            // Tạo unique orderId bằng cách thêm timestamp để tránh trùng lặp khi thanh toán lại cùng đơn hàng
            // Format: orderId (10 số) + timestamp (10 số) = 20 số tổng cộng
            // MoMo cho phép orderId dài hơn 10 số, nhưng để đảm bảo tương thích, ta sẽ dùng format này
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // Unix timestamp (10 số)
            var orderIdStr = $"{orderId.ToString().PadLeft(10, '0')}{timestamp}"; // 20 số tổng cộng
            var amountLong = (long)(amount); // MoMo yêu cầu số tiền dạng long
            var requestType = method == MoMoPaymentMethod.Wallet ? "captureWallet" : "payWithATM";

            var rawHash = $"accessKey={accessKey}&amount={amountLong}&extraData=&ipnUrl={fullNotifyUrl}&orderId={orderIdStr}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={fullReturnUrl}&requestId={requestId}&requestType={requestType}";

            var signature = ComputeHmacSha256(rawHash, secretKey);

            var requestData = new
            {
                partnerCode = partnerCode,
                partnerName = "Test",
                storeId = "MomoTestStore",
                requestId = requestId,
                amount = amountLong,
                orderId = orderIdStr,
                orderInfo = orderInfo,
                redirectUrl = fullReturnUrl,
                ipnUrl = fullNotifyUrl,
                lang = "vi",
                autoCapture = true,
                extraData = "",
                requestType = requestType,
                signature = signature
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(paymentUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<MoMoCreatePaymentResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.ResultCode == 0 && !string.IsNullOrEmpty(result.PayUrl))
                {
                    return result.PayUrl;
                }

                throw new Exception($"MoMo payment creation failed: {result?.Message ?? "Unknown error"}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating MoMo payment: {ex.Message}", ex);
            }
        }

        public async Task<string> CreateVNPayPaymentUrlAsync(int orderId, decimal amount, string returnUrl, string ipnUrl, string orderDescription, HttpContext? httpContext = null)
        {
            var vnpayConfig = _configuration.GetSection("PaymentGateways:VNPay");
            var tmnCode = vnpayConfig["TmnCode"] ?? "";
            var hashSecret = vnpayConfig["HashSecret"] ?? "";
            var paymentUrl = vnpayConfig["PaymentUrl"] ?? "";

            var baseUrl = _configuration["PaymentSettings:BaseUrl"] ?? "https://localhost:7042";
            var fullReturnUrl = $"{baseUrl}{returnUrl}";
            var fullIpnUrl = $"{baseUrl}{ipnUrl}";

            var vnpAmount = (long)(amount * 100); // VNPay yêu cầu số tiền nhân 100
            var vnpCreateDate = DateTime.Now.ToString("yyyyMMddHHmmss"); // Format: yyyyMMddHHmmss
            // vnp_TxnRef: format orderId + timestamp ngắn để có thể extract orderId sau (tối đa 40 ký tự)
            var vnpTxnRef = $"{orderId}{DateTime.Now:yyMMddHHmmss}";

            // Lấy IP address từ HttpContext
            var ipAddress = GetIpAddress(httpContext);

            // Sử dụng SortedList để sắp xếp theo key (ordinal comparison như VNPay yêu cầu)
            var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", tmnCode },
                { "vnp_Amount", vnpAmount.ToString() },
                { "vnp_CurrCode", "VND" },
                { "vnp_TxnRef", vnpTxnRef },
                { "vnp_OrderInfo", orderDescription },
                { "vnp_OrderType", "other" },
                { "vnp_Locale", "vn" }, // Theo hướng dẫn dùng "vn"
                { "vnp_ReturnUrl", fullReturnUrl },
                { "vnp_IpAddr", ipAddress },
                { "vnp_CreateDate", vnpCreateDate }
            };

            // Thêm IPN URL nếu có
            if (!string.IsNullOrEmpty(fullIpnUrl))
            {
                vnpParams.Add("vnp_IpnUrl", fullIpnUrl);
            }

            // Tạo querystring với URL encode (như trong hướng dẫn)
            var queryString = new StringBuilder();
            foreach (var kvp in vnpParams.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                queryString.Append(WebUtility.UrlEncode(kvp.Key) + "=" + WebUtility.UrlEncode(kvp.Value) + "&");
            }

            // Loại bỏ dấu & cuối cùng để tính hash
            var signData = queryString.ToString();
            if (signData.Length > 0)
            {
                signData = signData.Remove(signData.Length - 1, 1);
            }

            // Tính hash từ signData đã encode
            var vnpSecureHash = ComputeHmacSha512(hashSecret, signData);

            // Thêm SecureHash vào querystring
            queryString.Append("vnp_SecureHash=" + vnpSecureHash);

            var paymentUrlWithParams = $"{paymentUrl}?{queryString}";
            return paymentUrlWithParams;
        }

        public Task<MoMoPaymentResult> ProcessMoMoCallbackAsync(Dictionary<string, string> callbackData)
        {
            return Task.FromResult(ProcessMoMoCallback(callbackData));
        }

        private MoMoPaymentResult ProcessMoMoCallback(Dictionary<string, string> callbackData)
        {
            var result = new MoMoPaymentResult();

            try
            {
                if (!callbackData.ContainsKey("resultCode") || !callbackData.ContainsKey("orderId"))
                {
                    result.IsSuccess = false;
                    result.Message = "Invalid callback data";
                    return result;
                }

                var resultCode = callbackData.GetValueOrDefault("resultCode", "");
                var orderIdStr = callbackData.GetValueOrDefault("orderId", "");
                var amountStr = callbackData.GetValueOrDefault("amount", "0");
                
                // Parse orderId: Format mới là 20 số (10 số đầu là orderId gốc + 10 số cuối là timestamp)
                // Lấy 10 số đầu để lấy orderId gốc
                string orderId;
                if (orderIdStr.Length >= 10)
                {
                    // Lấy 10 số đầu (orderId gốc)
                    var originalOrderIdStr = orderIdStr.Substring(0, 10);
                    orderId = originalOrderIdStr.TrimStart('0');
                    if (string.IsNullOrEmpty(orderId)) orderId = "0";
                }
                else
                {
                    // Fallback cho format cũ (nếu có)
                    orderId = orderIdStr.TrimStart('0');
                    if (string.IsNullOrEmpty(orderId)) orderId = "0";
                }
                
                var amount = decimal.TryParse(amountStr, out var amt) ? amt : 0;
                var transId = callbackData.GetValueOrDefault("transId");
                var message = callbackData.GetValueOrDefault("message");

                result.IsSuccess = resultCode == "0";
                result.OrderId = orderId;
                result.Amount = amount; // MoMo trả về số tiền đúng
                result.TransactionId = transId;
                result.Message = message ?? (result.IsSuccess ? "Payment successful" : "Payment failed");
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"Error processing MoMo callback: {ex.Message}";
            }

            return result;
        }

        public Task<VNPayPaymentResult> ProcessVNPayCallbackAsync(Dictionary<string, string> vnpParams)
        {
            return Task.FromResult(ProcessVNPayCallback(vnpParams));
        }

        private VNPayPaymentResult ProcessVNPayCallback(Dictionary<string, string> vnpParams)
        {
            var result = new VNPayPaymentResult();

            try
            {
                var vnpayConfig = _configuration.GetSection("PaymentGateways:VNPay");
                var hashSecret = vnpayConfig["HashSecret"] ?? "";

                if (!vnpParams.ContainsKey("vnp_SecureHash"))
                {
                    result.IsSuccess = false;
                    result.Message = "Missing secure hash";
                    return result;
                }

                var vnpSecureHash = vnpParams["vnp_SecureHash"];
                vnpParams.Remove("vnp_SecureHash");

                // Tạo lại hash để verify (theo đúng cách của VNPay - URL encode)
                var sortedParams = new SortedDictionary<string, string>(vnpParams, StringComparer.Ordinal);
                var queryString = new StringBuilder();
                foreach (var kvp in sortedParams.Where(kv => !string.IsNullOrEmpty(kv.Value)))
                {
                    queryString.Append(WebUtility.UrlEncode(kvp.Key) + "=" + WebUtility.UrlEncode(kvp.Value) + "&");
                }

                var signData = queryString.ToString();
                if (signData.Length > 0)
                {
                    signData = signData.Remove(signData.Length - 1, 1);
                }

                var hash = ComputeHmacSha512(hashSecret, signData);

                if (hash != vnpSecureHash)
                {
                    result.IsSuccess = false;
                    result.Message = "Invalid signature";
                    return result;
                }

                var responseCode = vnpParams.ContainsKey("vnp_ResponseCode") ? vnpParams["vnp_ResponseCode"] : "";
                var vnpTxnRef = vnpParams.ContainsKey("vnp_TxnRef") ? vnpParams["vnp_TxnRef"] : "";
                var amount = vnpParams.ContainsKey("vnp_Amount") ? decimal.Parse(vnpParams["vnp_Amount"]) / 100 : 0;
                var transId = vnpParams.ContainsKey("vnp_TransactionNo") ? vnpParams["vnp_TransactionNo"] : "";

                result.IsSuccess = responseCode == "00";
                // Extract orderId từ vnp_TxnRef (format: orderId + yyMMddHHmmss)
                // Tìm vị trí đầu tiên của timestamp (10 số) và lấy phần orderId
                string? extractedOrderId = null;
                if (!string.IsNullOrEmpty(vnpTxnRef))
                {
                    // Format: orderId + yyMMddHHmmss (10 số cuối là timestamp)
                    // Ví dụ: "30411120754" -> orderId = "3", timestamp = "0411120754"
                    // Cần tách orderId bằng cách loại bỏ 10 số cuối
                    if (vnpTxnRef.Length > 10)
                    {
                        extractedOrderId = vnpTxnRef.Substring(0, vnpTxnRef.Length - 10);
                    }
                    else
                    {
                        extractedOrderId = vnpTxnRef; // Fallback nếu không đủ dài
                    }
                }
                result.OrderId = extractedOrderId;
                result.Amount = amount;
                result.TransactionId = transId;
                result.ResponseCode = responseCode;
                result.Message = result.IsSuccess ? "Payment successful" : $"Payment failed: {responseCode}";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"Error processing VNPay callback: {ex.Message}";
            }

            return result;
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        private string ComputeHmacSha512(string secretKey, string inputData)
        {
            // Theo hướng dẫn: dùng format "x2" (lowercase hex)
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            
            using var hmac = new HMACSHA512(keyBytes);
            var hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
            
            return hash.ToString();
        }

        private string GetIpAddress(HttpContext? context)
        {
            var ipAddress = string.Empty;
            try
            {
                if (context?.Connection.RemoteIpAddress != null)
                {
                    var remoteIpAddress = context.Connection.RemoteIpAddress;

                    // Xử lý IPv6
                    if (remoteIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        try
                        {
                            var hostEntry = Dns.GetHostEntry(remoteIpAddress);
                            remoteIpAddress = hostEntry.AddressList
                                .FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                        }
                        catch
                        {
                            // Fallback nếu không resolve được
                        }
                    }

                    if (remoteIpAddress != null)
                    {
                        ipAddress = remoteIpAddress.ToString();
                    }
                }
            }
            catch
            {
                // Fallback
            }

            return string.IsNullOrEmpty(ipAddress) ? "127.0.0.1" : ipAddress;
        }

        private class MoMoCreatePaymentResponse
        {
            public int ResultCode { get; set; }
            public string? Message { get; set; }
            public string? PayUrl { get; set; }
            public string? QrCodeUrl { get; set; }
            public string? Deeplink { get; set; }
        }
    }
}


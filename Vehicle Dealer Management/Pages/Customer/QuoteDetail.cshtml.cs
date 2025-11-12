using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.BLL.IService;
using Vehicle_Dealer_Management.DAL.Constants;
using Vehicle_Dealer_Management.DAL.Data;
using System.IO;
using System.Linq;

namespace Vehicle_Dealer_Management.Pages.Customer
{
    public class QuoteDetailModel : PageModel
    {
        private readonly ISalesDocumentService _salesDocumentService;
        private readonly IContractService _contractService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public QuoteDetailModel(
            ISalesDocumentService salesDocumentService,
            IContractService contractService,
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _salesDocumentService = salesDocumentService;
            _contractService = contractService;
            _context = context;
            _environment = environment;
        }

        public QuoteDetailViewModel Quote { get; set; } = null!;
        public ContractViewModel? Contract { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            ViewData["UserRole"] = "CUSTOMER";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "Customer";

            var userIdInt = int.Parse(userId);

            // Get customer profile from user
            var customer = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == userIdInt);

            if (customer == null)
            {
                return RedirectToPage("/Auth/Profile");
            }

            // Get quote with all related data - only quotes belonging to this customer
            var quote = await _salesDocumentService.GetSalesDocumentWithDetailsAsync(id);

            if (quote == null || quote.CustomerId != customer.Id || quote.Type != "QUOTE")
            {
                return NotFound();
            }

            // Calculate totals from real data
            var totalAmount = quote.Lines?.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue) ?? 0;

            Quote = new QuoteDetailViewModel
            {
                Id = quote.Id,
                QuoteNumber = $"QUO-{quote.Id:D6}",
                Status = quote.Status,
                CreatedAt = quote.CreatedAt,
                UpdatedAt = quote.UpdatedAt,

                // Dealer Info
                DealerName = quote.Dealer?.Name ?? "N/A",
                DealerAddress = quote.Dealer?.Address ?? "N/A",
                DealerPhone = quote.Dealer?.PhoneNumber ?? "N/A",

                // Promotion
                PromotionId = quote.PromotionId,
                PromotionName = quote.Promotion?.Name,

                // Items (Lines)
                Items = quote.Lines?.Select(l => new QuoteItemViewModel
                {
                    Id = l.Id,
                    VehicleId = l.VehicleId,
                    VehicleModel = l.Vehicle?.ModelName ?? "N/A",
                    VehicleVariant = l.Vehicle?.VariantName ?? "N/A",
                    ColorCode = l.ColorCode,
                    Qty = l.Qty,
                    UnitPrice = l.UnitPrice,
                    DiscountValue = l.DiscountValue,
                    LineTotal = l.UnitPrice * l.Qty - l.DiscountValue
                }).ToList() ?? new List<QuoteItemViewModel>(),

                TotalAmount = totalAmount
            };

            var contract = await _contractService.GetContractByQuoteIdAsync(quote.Id);
            if (contract != null && contract.CustomerId == customer.Id)
            {
                Contract = new ContractViewModel
                {
                    Id = contract.Id,
                    Status = contract.Status,
                    CreatedAt = contract.CreatedAt,
                    CustomerSignedAt = contract.CustomerSignedAt,
                    CustomerSignatureUrl = contract.CustomerSignatureUrl,
                    DealerId = contract.DealerId
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAcceptAsync(int id)
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

            var quote = await _salesDocumentService.GetSalesDocumentWithDetailsAsync(id);

            if (quote == null || quote.CustomerId != customer.Id || quote.Type != "QUOTE")
            {
                return NotFound();
            }

            // Update status to ACCEPTED using service
            await _salesDocumentService.UpdateSalesDocumentStatusAsync(id, "ACCEPTED");

            TempData["Success"] = "Báo giá đã được chấp nhận!";
            return RedirectToPage("/Customer/QuoteDetail", new { id });
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
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

            var quote = await _salesDocumentService.GetSalesDocumentWithDetailsAsync(id);

            if (quote == null || quote.CustomerId != customer.Id || quote.Type != "QUOTE")
            {
                return NotFound();
            }

            // Update status to REJECTED
            quote.Status = "REJECTED";
            quote.UpdatedAt = DateTime.UtcNow;
            await _salesDocumentService.UpdateSalesDocumentStatusAsync(id, "REJECTED");

            return RedirectToPage("/Customer/MyQuotes");
        }

        public async Task<IActionResult> OnPostUploadSignatureAsync(int id, IFormFile? signatureFile)
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

            var quote = await _salesDocumentService.GetSalesDocumentWithDetailsAsync(id);

            if (quote == null || quote.CustomerId != customer.Id || quote.Type != "QUOTE")
            {
                return NotFound();
            }

            var contract = await _contractService.GetContractByQuoteIdAsync(id);
            if (contract == null || contract.CustomerId != customer.Id)
            {
                TempData["Error"] = "Không tìm thấy hợp đồng để ký.";
                return RedirectToPage("/Customer/QuoteDetail", new { id });
            }

            if (SalesContractStatus.IsSigned(contract.Status))
            {
                TempData["Info"] = "Bạn đã tải chữ ký cho hợp đồng này.";
                return RedirectToPage("/Customer/QuoteDetail", new { id });
            }

            if (signatureFile == null || signatureFile.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn ảnh chữ ký hợp lệ.";
                return RedirectToPage("/Customer/QuoteDetail", new { id });
            }

            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
            var extension = Path.GetExtension(signatureFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                TempData["Error"] = "Chỉ cho phép định dạng ảnh PNG hoặc JPG.";
                return RedirectToPage("/Customer/QuoteDetail", new { id });
            }

            var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "signatures");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            var fileName = $"contract_{id}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await signatureFile.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/signatures/{fileName}".Replace("\\", "/");
            await _contractService.SaveCustomerSignatureAsync(contract.Id, relativePath);

            TempData["Success"] = "Đã tải chữ ký thành công. Dealer sẽ kiểm tra và tiếp tục xử lý đơn hàng.";
            return RedirectToPage("/Customer/QuoteDetail", new { id });
        }

        public class QuoteDetailViewModel
        {
            public int Id { get; set; }
            public string QuoteNumber { get; set; } = "";
            public string Status { get; set; } = "";
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }

            // Dealer Info
            public string DealerName { get; set; } = "";
            public string DealerAddress { get; set; } = "";
            public string DealerPhone { get; set; } = "";

            // Promotion
            public int? PromotionId { get; set; }
            public string? PromotionName { get; set; }

            // Items
            public List<QuoteItemViewModel> Items { get; set; } = new();
            public decimal TotalAmount { get; set; }
        }

        public class QuoteItemViewModel
        {
            public int Id { get; set; }
            public int VehicleId { get; set; }
            public string VehicleModel { get; set; } = "";
            public string VehicleVariant { get; set; } = "";
            public string ColorCode { get; set; } = "";
            public decimal Qty { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal DiscountValue { get; set; }
            public decimal LineTotal { get; set; }
        }

        public class ContractViewModel
        {
            public int Id { get; set; }
            public string Status { get; set; } = "";
            public DateTime CreatedAt { get; set; }
            public DateTime? CustomerSignedAt { get; set; }
            public string? CustomerSignatureUrl { get; set; }
            public int DealerId { get; set; }
        }
    }
}


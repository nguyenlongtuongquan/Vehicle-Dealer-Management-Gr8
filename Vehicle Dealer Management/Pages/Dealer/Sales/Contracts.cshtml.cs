using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.Pages.Dealer.Sales
{
    public class ContractsModel : PageModel
    {
        private readonly ISalesDocumentService _salesDocumentService;

        public ContractsModel(ISalesDocumentService salesDocumentService)
        {
            _salesDocumentService = salesDocumentService;
        }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        public List<ContractViewModel> Contracts { get; set; } = new();
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int CompletedCount { get; set; }
        public int TerminatedCount { get; set; }

        public async Task<IActionResult> OnGetAsync(string? status)
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole") ?? "DEALER_STAFF";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "User";

            StatusFilter = status ?? "all";
            var dealerIdInt = int.Parse(dealerId);

            // Get all contracts for counting
            var allContracts = await _salesDocumentService.GetSalesDocumentsByDealerIdAsync(
                dealerIdInt, 
                type: "CONTRACT", 
                status: null);

            TotalCount = allContracts.Count();
            ActiveCount = allContracts.Count(c => c.Status == "ACTIVE");
            CompletedCount = allContracts.Count(c => c.Status == "COMPLETED");
            TerminatedCount = allContracts.Count(c => c.Status == "TERMINATED");

            // Filter by status if specified
            var contracts = StatusFilter != "all" && !string.IsNullOrEmpty(StatusFilter)
                ? allContracts.Where(c => c.Status == StatusFilter)
                : allContracts;

            Contracts = contracts.Select(c => new ContractViewModel
            {
                Id = c.Id,
                CustomerName = c.Customer?.FullName ?? "N/A",
                CustomerPhone = c.Customer?.Phone ?? "N/A",
                SignedAt = c.SignedAt ?? c.CreatedAt,
                Status = c.Status,
                TotalAmount = c.Lines?.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue) ?? 0,
                VehicleCount = (int)(c.Lines?.Sum(l => (decimal?)l.Qty) ?? 0)
            }).ToList();

            return Page();
        }

        public class ContractViewModel
        {
            public int Id { get; set; }
            public string CustomerName { get; set; } = "";
            public string CustomerPhone { get; set; } = "";
            public DateTime SignedAt { get; set; }
            public string Status { get; set; } = "";
            public decimal TotalAmount { get; set; }
            public int VehicleCount { get; set; }
        }
    }
}

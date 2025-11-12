using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vehicle_Dealer_Management.BLL.IService;
using Vehicle_Dealer_Management.DAL.Constants;

namespace Vehicle_Dealer_Management.Pages.Dealer.Sales
{
    public class ContractsModel : PageModel
    {
        private readonly IContractService _contractService;

        public ContractsModel(IContractService contractService)
        {
            _contractService = contractService;
        }

        public List<ContractListItem> Contracts { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole") ?? "DEALER_STAFF";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "User";

            var dealerIdInt = int.Parse(dealerId);
            var contracts = await _contractService.GetContractsForDealerAsync(dealerIdInt);

            Contracts = contracts.Select(c => new ContractListItem
            {
                Id = c.Id,
                QuoteId = c.QuoteId,
                OrderId = c.OrderId,
                CustomerName = c.Customer?.FullName ?? "Khách hàng",
                Status = c.Status,
                CreatedAt = c.CreatedAt,
                SignedAt = c.CustomerSignedAt,
                SignatureUrl = c.CustomerSignatureUrl
            }).ToList();

            return Page();
        }

        public class ContractListItem
        {
            public int Id { get; set; }
            public int QuoteId { get; set; }
            public int? OrderId { get; set; }
            public string CustomerName { get; set; } = "";
            public string Status { get; set; } = "";
            public DateTime CreatedAt { get; set; }
            public DateTime? SignedAt { get; set; }
            public string? SignatureUrl { get; set; }

            public bool IsSigned => SalesContractStatus.IsSigned(Status);
        }
    }
}



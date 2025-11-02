using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.Pages.EVM
{
    public class DealersModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IDealerService _dealerService;
        private readonly ISalesDocumentService _salesDocumentService;

        public DealersModel(
            ApplicationDbContext context, 
            IDealerService dealerService,
            ISalesDocumentService salesDocumentService)
        {
            _context = context;
            _dealerService = dealerService;
            _salesDocumentService = salesDocumentService;
        }

        public int TotalDealers { get; set; }
        public int ActiveDealers { get; set; }
        public int InactiveDealers { get; set; }
        public decimal AvgDealerSales { get; set; }

        public List<DealerViewModel> Dealers { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Get all dealers
            var dealers = (await _dealerService.GetAllDealersAsync()).ToList();

            TotalDealers = dealers.Count;
            ActiveDealers = dealers.Count(d => d.Status == "ACTIVE");
            InactiveDealers = dealers.Count(d => d.Status != "ACTIVE");

            // Get sales for each dealer from actual data
            foreach (var dealer in dealers)
            {
                // Get orders for this dealer
                var orders = (await _salesDocumentService.GetSalesDocumentsByDealerIdAsync(dealer.Id, "ORDER", null)).ToList();
                
                // Calculate monthly sales (current month)
                var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var monthlyOrders = orders
                    .Where(o => o.CreatedAt >= currentMonthStart && 
                                o.CreatedAt < currentMonthStart.AddMonths(1))
                    .ToList();
                
                var monthlySales = monthlyOrders
                    .Sum(o => o.Lines?.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue) ?? 0);
                
                // Total orders count
                var totalOrders = orders.Count;

                Dealers.Add(new DealerViewModel
                {
                    Id = dealer.Id,
                    Name = dealer.Name,
                    Address = dealer.Address,
                    Phone = dealer.PhoneNumber,
                    Email = dealer.Email,
                    Status = dealer.Status,
                    MonthlySales = monthlySales,
                    TotalOrders = totalOrders
                });
            }

            AvgDealerSales = Dealers.Any() ? Dealers.Average(d => d.MonthlySales) : 0;

            return Page();
        }

        public class DealerViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Address { get; set; } = "";
            public string Phone { get; set; } = "";
            public string Email { get; set; } = "";
            public string Status { get; set; } = "";
            public decimal MonthlySales { get; set; }
            public int TotalOrders { get; set; }
        }
    }
}


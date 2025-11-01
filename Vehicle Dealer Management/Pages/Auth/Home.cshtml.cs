using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;

namespace Vehicle_Dealer_Management.Pages.Auth
{
    public class HomeModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public HomeModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalVehicles { get; set; }
        public int TotalDealers { get; set; }
        public int TotalCustomers { get; set; }

        public async Task OnGetAsync()
        {
            // Get statistics for homepage
            TotalVehicles = await _context.Vehicles.CountAsync(v => v.Status == "AVAILABLE");
            TotalDealers = await _context.Dealers.CountAsync(d => d.Status == "ACTIVE");
            TotalCustomers = await _context.CustomerProfiles.CountAsync();
        }
    }
}


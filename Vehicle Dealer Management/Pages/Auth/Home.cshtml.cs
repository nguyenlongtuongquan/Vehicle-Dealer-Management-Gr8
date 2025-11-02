using Microsoft.AspNetCore.Mvc;
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

        public async Task<IActionResult> OnGetAsync()
        {
            // If user is logged in, redirect to their dashboard
            var userId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

                if (user != null)
                {
                    return user.Role.Code switch
                    {
                        "CUSTOMER" => RedirectToPage("/Customer/Dashboard"),
                        "DEALER_STAFF" => RedirectToPage("/Dealer/Dashboard"),
                        "DEALER_MANAGER" => RedirectToPage("/DealerManager/Dashboard"),
                        "EVM_STAFF" => RedirectToPage("/EVM/Dashboard"),
                        "EVM_ADMIN" => RedirectToPage("/Admin/Dashboard"),
                        _ => Page()
                    };
                }
            }

            // Get statistics for homepage (public)
            TotalVehicles = await _context.Vehicles.CountAsync(v => v.Status == "AVAILABLE");
            TotalDealers = await _context.Dealers.CountAsync(d => d.Status == "ACTIVE");
            TotalCustomers = await _context.CustomerProfiles.CountAsync();

            return Page();
        }
    }
}


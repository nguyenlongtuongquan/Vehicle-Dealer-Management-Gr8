using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using System.Text.Json;

namespace Vehicle_Dealer_Management.Pages.Customer
{
    public class VehicleDetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public VehicleDetailModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public VehicleDetailViewModel Vehicle { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Get vehicle with all related data from DB
            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == id && v.Status == "AVAILABLE");

            if (vehicle == null)
            {
                return NotFound();
            }

            // Get price policy (MSRP - customer sees retail price)
            var pricePolicy = await _context.PricePolicies
                .Where(p => p.VehicleId == vehicle.Id && p.DealerId == null &&
                            p.ValidFrom <= DateTime.UtcNow &&
                            (p.ValidTo == null || p.ValidTo >= DateTime.UtcNow))
                .OrderByDescending(p => p.ValidFrom)
                .FirstOrDefaultAsync();

            // Get all dealers for test drive booking
            var dealers = await _context.Dealers
                .Where(d => d.Status == "ACTIVE" && d.IsActive == true)
                .Select(d => new DealerSimpleViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Address = d.Address,
                    Phone = d.PhoneNumber
                })
                .ToListAsync();

            // Parse specs from JSON
            var specs = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(vehicle.SpecJson))
            {
                try
                {
                    specs = JsonSerializer.Deserialize<Dictionary<string, string>>(vehicle.SpecJson) ?? new Dictionary<string, string>();
                }
                catch
                {
                    specs = new Dictionary<string, string>();
                }
            }

            Vehicle = new VehicleDetailViewModel
            {
                Id = vehicle.Id,
                ModelName = vehicle.ModelName,
                VariantName = vehicle.VariantName,
                ImageUrl = vehicle.ImageUrl,
                Status = vehicle.Status,
                Specs = specs,
                Price = pricePolicy?.Msrp ?? 0,
                Dealers = dealers
            };

            return Page();
        }

        public class VehicleDetailViewModel
        {
            public int Id { get; set; }
            public string ModelName { get; set; } = "";
            public string VariantName { get; set; } = "";
            public string ImageUrl { get; set; } = "";
            public string Status { get; set; } = "";
            public Dictionary<string, string> Specs { get; set; } = new();
            public decimal Price { get; set; }
            public List<DealerSimpleViewModel> Dealers { get; set; } = new();
        }

        public class DealerSimpleViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Address { get; set; } = "";
            public string Phone { get; set; } = "";
        }
    }
}


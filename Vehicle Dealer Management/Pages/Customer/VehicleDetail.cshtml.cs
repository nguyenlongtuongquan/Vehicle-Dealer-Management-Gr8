using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.BLL.IService;
using Vehicle_Dealer_Management.DAL.Data;
using System.Text.Json;

namespace Vehicle_Dealer_Management.Pages.Customer
{
    public class VehicleDetailModel : PageModel
    {
        private readonly IVehicleService _vehicleService;
        private readonly IPricePolicyService _pricePolicyService;
        private readonly IDealerService _dealerService;
        private readonly ApplicationDbContext _context; // Tạm thời cần cho Dealers query

        public VehicleDetailModel(
            IVehicleService vehicleService,
            IPricePolicyService pricePolicyService,
            IDealerService dealerService,
            ApplicationDbContext context)
        {
            _vehicleService = vehicleService;
            _pricePolicyService = pricePolicyService;
            _dealerService = dealerService;
            _context = context;
        }

        public VehicleDetailViewModel Vehicle { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Get vehicle from Service
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);

            if (vehicle == null || vehicle.Status != "AVAILABLE")
            {
                return NotFound();
            }

            // Get price policy (MSRP - customer sees retail price)
            var pricePolicy = await _pricePolicyService.GetActivePricePolicyAsync(vehicle.Id, null);

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
                    var parsedSpecs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(vehicle.SpecJson);
                    if (parsedSpecs != null)
                    {
                        foreach (var kvp in parsedSpecs)
                        {
                            // Convert JsonElement to string representation
                            specs[kvp.Key] = kvp.Value.ValueKind switch
                            {
                                JsonValueKind.String => kvp.Value.GetString() ?? "",
                                JsonValueKind.Number => kvp.Value.GetRawText(),
                                JsonValueKind.True => "Có",
                                JsonValueKind.False => "Không",
                                _ => kvp.Value.GetRawText()
                            };
                        }
                    }
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
                OriginalPrice = pricePolicy?.OriginalMsrp, // Giá gốc trước khi giảm
                PriceNote = pricePolicy?.Note,
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
            public decimal Price { get; set; } // Giá cuối (sau discount)
            public decimal? OriginalPrice { get; set; } // Giá gốc (trước khi giảm)
            public string? PriceNote { get; set; }
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


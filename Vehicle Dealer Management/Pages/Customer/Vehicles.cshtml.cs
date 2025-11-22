using Microsoft.AspNetCore.Mvc.RazorPages;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.Pages.Customer
{
    public class VehiclesModel : PageModel
    {
        private readonly IVehicleService _vehicleService;
        private readonly IPricePolicyService _pricePolicyService;

        public VehiclesModel(IVehicleService vehicleService, IPricePolicyService pricePolicyService)
        {
            _vehicleService = vehicleService;
            _pricePolicyService = pricePolicyService;
        }

        public List<VehicleViewModel> Vehicles { get; set; } = new();
        public string? FilterModel { get; set; }
        public List<string> AvailableModels { get; set; } = new();

        public async Task OnGetAsync(string? filterModel)
        {
            FilterModel = filterModel;
            
            var vehicles = await _vehicleService.GetAvailableVehiclesAsync();

            // Get unique model names for filter dropdown
            AvailableModels = vehicles.Select(v => v.ModelName).Distinct().OrderBy(m => m).ToList();

            // Apply filter if specified
            if (!string.IsNullOrWhiteSpace(filterModel))
            {
                vehicles = vehicles.Where(v => v.ModelName == filterModel);
            }

            foreach (var vehicle in vehicles)
            {
                var pricePolicy = await _pricePolicyService.GetActivePricePolicyAsync(vehicle.Id, null);

                Vehicles.Add(new VehicleViewModel
                {
                    Id = vehicle.Id,
                    Name = vehicle.ModelName,
                    Variant = vehicle.VariantName,
                    ImageUrl = vehicle.ImageUrl,
                    Description = "Xe điện hiện đại, tiết kiệm năng lượng",
                    Price = pricePolicy?.Msrp ?? 0
                });
            }
        }

        public class VehicleViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Variant { get; set; } = "";
            public string ImageUrl { get; set; } = "";
            public string Description { get; set; } = "";
            public decimal Price { get; set; }
        }
    }
}


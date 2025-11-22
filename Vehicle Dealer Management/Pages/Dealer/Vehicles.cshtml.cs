using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.Pages.Dealer
{
    public class VehiclesModel : PageModel
    {
        private readonly IVehicleService _vehicleService;
        private readonly IPricePolicyService _pricePolicyService;
        private readonly IStockService _stockService;

        public VehiclesModel(
            IVehicleService vehicleService,
            IPricePolicyService pricePolicyService,
            IStockService stockService)
        {
            _vehicleService = vehicleService;
            _pricePolicyService = pricePolicyService;
            _stockService = stockService;
        }

        public List<VehicleViewModel> Vehicles { get; set; } = new();
        public string? SearchQuery { get; set; }
        public string? FilterModel { get; set; }
        public string? FilterStatus { get; set; }
        public List<string> AvailableModels { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? search, string? filterModel, string? filterStatus)
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole") ?? "DEALER_STAFF";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "User";

            SearchQuery = search;
            FilterModel = filterModel;
            FilterStatus = filterStatus;

            var dealerIdInt = int.Parse(dealerId);

            // ✅ THAY ĐỔI: Chỉ lấy xe trong kho DEALER, không phải EVM
            var dealerStocks = await _stockService.GetStocksByOwnerAsync("DEALER", dealerIdInt);

            // Group by VehicleId để tránh trùng lặp
            var vehicleGroups = dealerStocks.GroupBy(s => s.VehicleId);

            var allVehicles = new List<VehicleViewModel>();
            var allVehicleModels = new HashSet<string>();

            foreach (var group in vehicleGroups)
            {
                var vehicleId = group.Key;

                // Get vehicle details
                var vehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId);
                if (vehicle == null) continue;

                // Collect all model names for filter dropdown (before filtering)
                allVehicleModels.Add(vehicle.ModelName);

                // Apply status filter
                if (!string.IsNullOrWhiteSpace(filterStatus))
                {
                    if (filterStatus == "available" && vehicle.Status != "AVAILABLE") continue;
                    if (filterStatus == "coming_soon" && vehicle.Status != "COMING_SOON") continue;
                }
                else
                {
                    // Default: only show AVAILABLE if no filter
                    if (vehicle.Status != "AVAILABLE") continue;
                }

                // Get price policy
                var pricePolicy = await _pricePolicyService.GetActivePricePolicyAsync(vehicleId, dealerIdInt);

                // Get available colors from dealer stock
                var colorStocks = group.Select(s => new ColorStock
                {
                    Color = s.ColorCode,
                    Qty = (int)s.Qty
                }).ToList();

                allVehicles.Add(new VehicleViewModel
                {
                    Id = vehicle.Id,
                    Name = vehicle.ModelName,
                    Variant = vehicle.VariantName,
                    ImageUrl = vehicle.ImageUrl,
                    Status = vehicle.Status,
                    Msrp = pricePolicy?.Msrp ?? 0,
                    WholesalePrice = pricePolicy?.WholesalePrice ?? 0,
                    AvailableColors = colorStocks
                });
            }

            // Get unique model names for filter dropdown (from all vehicles, not filtered)
            AvailableModels = allVehicleModels.OrderBy(m => m).ToList();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                allVehicles = allVehicles.Where(v => 
                    v.Name.ToLower().Contains(searchLower) || 
                    v.Variant.ToLower().Contains(searchLower)
                ).ToList();
            }

            // Apply model filter
            if (!string.IsNullOrWhiteSpace(filterModel))
            {
                allVehicles = allVehicles.Where(v => v.Name == filterModel).ToList();
            }

            Vehicles = allVehicles;

            return Page();
        }

        public class VehicleViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Variant { get; set; } = "";
            public string ImageUrl { get; set; } = "";
            public string Status { get; set; } = "";
            public decimal Msrp { get; set; }
            public decimal WholesalePrice { get; set; }
            public List<ColorStock> AvailableColors { get; set; } = new();
        }

        public class ColorStock
        {
            public string Color { get; set; } = "";
            public int Qty { get; set; }
        }
    }
}
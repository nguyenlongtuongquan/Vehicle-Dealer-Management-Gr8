using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.Models;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.Pages.Dealer
{
    public class TestDrivesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ICustomerService _customerService;
        private readonly IVehicleService _vehicleService;
        private readonly ITestDriveService _testDriveService;

        public TestDrivesModel(
            ApplicationDbContext context,
            ICustomerService customerService,
            IVehicleService vehicleService,
            ITestDriveService testDriveService)
        {
            _context = context;
            _customerService = customerService;
            _vehicleService = vehicleService;
            _testDriveService = testDriveService;
        }

        public string Filter { get; set; } = "all";
        public int TodayCount { get; set; }
        public int RequestedCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int DoneCount { get; set; }

        public List<CustomerSimple> AllCustomers { get; set; } = new();
        public List<VehicleSimple> AllVehicles { get; set; } = new();
        public List<TestDriveViewModel> TestDrives { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? filter)
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                TempData["Error"] = "Không tìm thấy thông tin đại lý. Vui lòng đăng nhập lại.";
                return RedirectToPage("/Auth/Login");
            }

            Filter = filter ?? "all";
            var dealerIdInt = int.Parse(dealerId);
            
            // Debug: Verify dealer exists
            var dealer = await _context.Dealers.FindAsync(dealerIdInt);
            if (dealer == null)
            {
                TempData["Error"] = $"Đại lý với ID {dealerIdInt} không tồn tại.";
                return RedirectToPage("/Auth/Login");
            }

            // Get customers for create form
            var customers = await _customerService.GetAllCustomersAsync();
            AllCustomers = customers.Select(c => new CustomerSimple
            {
                Id = c.Id,
                Name = c.FullName,
                Phone = c.PhoneNumber ?? ""
            }).ToList();

            // Get vehicles for create form
            var vehicles = await _vehicleService.GetAvailableVehiclesAsync();
            AllVehicles = vehicles.Select(v => new VehicleSimple
            {
                Id = v.Id,
                Name = v.ModelName + " " + v.VariantName
            }).ToList();

            // Get test drives using service
            IEnumerable<Vehicle_Dealer_Management.DAL.Models.TestDrive> testDrives;

            switch (Filter)
            {
                case "today":
                    testDrives = await _testDriveService.GetTestDrivesByDealerAndDateAsync(dealerIdInt, DateTime.Today);
                    break;
                case "requested":
                    testDrives = await _testDriveService.GetTestDrivesByStatusAsync("REQUESTED", dealerIdInt);
                    break;
                case "upcoming":
                    var allTestDrives = await _testDriveService.GetTestDrivesByDealerIdAsync(dealerIdInt);
                    testDrives = allTestDrives.Where(t => t.ScheduleTime > DateTime.Now && t.Status == "CONFIRMED");
                    break;
                default:
                    testDrives = await _testDriveService.GetTestDrivesByDealerIdAsync(dealerIdInt);
                    break;
            }

            var testDrivesList = testDrives.ToList();
            
            // Debug: Check all test drives to see if there's a mismatch
            var allTestDrivesInDb = await _context.TestDrives.ToListAsync();
            var testDrivesWithDifferentDealer = allTestDrivesInDb
                .Where(t => t.DealerId != dealerIdInt)
                .ToList();
            
            // Debug: If there are test drives but none match, show debug info
            if (allTestDrivesInDb.Any() && !testDrivesList.Any())
            {
                // There are test drives in DB but none match this dealerId
                var actualDealerIds = allTestDrivesInDb.Select(t => t.DealerId).Distinct().ToList();
                var dealersInTestDrives = await _context.Dealers
                    .Where(d => actualDealerIds.Contains(d.Id))
                    .Select(d => $"{d.Name} (ID: {d.Id})")
                    .ToListAsync();
                
                TempData["DebugInfo"] = $"Lưu ý: Tìm thấy {allTestDrivesInDb.Count} test drive trong hệ thống, nhưng không có lịch nào thuộc về đại lý của bạn. " +
                    $"Bạn đang làm việc tại: <strong>{dealer?.Name ?? "N/A"}</strong> (ID: {dealerIdInt}). " +
                    $"Các test drive hiện có thuộc về: {string.Join(", ", dealersInTestDrives)}. " +
                    $"Vui lòng đăng nhập với tài khoản của đại lý tương ứng để xem lịch.";
            }

            // Calculate counts
            var todayTestDrives = await _testDriveService.GetTestDrivesByDealerAndDateAsync(dealerIdInt, DateTime.Today);
            TodayCount = todayTestDrives.Count();

            var requestedTestDrives = await _testDriveService.GetTestDrivesByStatusAsync("REQUESTED", dealerIdInt);
            RequestedCount = requestedTestDrives.Count();

            var confirmedTestDrives = await _testDriveService.GetTestDrivesByStatusAsync("CONFIRMED", dealerIdInt);
            ConfirmedCount = confirmedTestDrives.Count();

            var doneTestDrives = await _testDriveService.GetTestDrivesByStatusAsync("DONE", dealerIdInt);
            DoneCount = doneTestDrives.Count();

            TestDrives = testDrivesList.Select(t => new TestDriveViewModel
            {
                Id = t.Id,
                CustomerName = t.Customer?.FullName ?? "N/A",
                CustomerPhone = t.Customer?.Phone ?? "N/A",
                VehicleName = $"{t.Vehicle?.ModelName} {t.Vehicle?.VariantName}",
                ScheduleTime = t.ScheduleTime,
                Status = t.Status,
                Note = t.Note ?? ""
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmAsync(int id)
        {
            await _testDriveService.UpdateTestDriveStatusAsync(id, "CONFIRMED");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMarkDoneAsync(int id)
        {
            await _testDriveService.UpdateTestDriveStatusAsync(id, "DONE");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateAsync(int customerId, int vehicleId, DateTime date, TimeSpan time, string? note)
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var scheduleTime = date.Add(time);

            var testDrive = new Vehicle_Dealer_Management.DAL.Models.TestDrive
            {
                CustomerId = customerId,
                DealerId = int.Parse(dealerId),
                VehicleId = vehicleId,
                ScheduleTime = scheduleTime,
                Status = "CONFIRMED", // Auto confirm if created by staff
                Note = note
            };

            await _testDriveService.CreateTestDriveAsync(testDrive);

            return RedirectToPage();
        }

        public class CustomerSimple
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Phone { get; set; } = "";
        }

        public class VehicleSimple
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }

        public class TestDriveViewModel
        {
            public int Id { get; set; }
            public string CustomerName { get; set; } = "";
            public string CustomerPhone { get; set; } = "";
            public string VehicleName { get; set; } = "";
            public DateTime ScheduleTime { get; set; }
            public string Status { get; set; } = "";
            public string Note { get; set; } = "";
        }
    }
}


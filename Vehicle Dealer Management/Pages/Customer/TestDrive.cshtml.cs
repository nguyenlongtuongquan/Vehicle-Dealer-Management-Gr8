using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.BLL.IService;
using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.Pages.Customer
{
    public class TestDriveModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IDealerService _dealerService;
        private readonly IVehicleService _vehicleService;
        private readonly ITestDriveService _testDriveService;

        public TestDriveModel(
            ApplicationDbContext context,
            IDealerService dealerService,
            IVehicleService vehicleService,
            ITestDriveService testDriveService)
        {
            _context = context;
            _dealerService = dealerService;
            _vehicleService = vehicleService;
            _testDriveService = testDriveService;
        }

        public List<TestDriveViewModel> TestDrives { get; set; } = new();
        public List<DealerSimple> AllDealers { get; set; } = new();
        public List<VehicleSimple> AllVehicles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "CUSTOMER")
            {
                return RedirectToPage("/Auth/Login");
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (user == null || user.Role?.Code != "CUSTOMER")
            {
                return RedirectToPage("/Auth/Login");
            }

            // Get all dealers and vehicles for booking form (always reload)
            var activeDealers = await _dealerService.GetActiveDealersAsync();
            AllDealers = activeDealers.Select(d => new DealerSimple
            {
                Id = d.Id,
                Name = d.Name,
                Address = d.Address
            }).ToList();

            var availableVehicles = await _vehicleService.GetAvailableVehiclesAsync();
            AllVehicles = availableVehicles.Select(v => new VehicleSimple
            {
                Id = v.Id,
                Name = v.ModelName + " " + v.VariantName
            }).ToList();

            var customerProfile = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customerProfile != null)
            {
                var testDrives = await _testDriveService.GetTestDrivesByCustomerIdAsync(customerProfile.Id);

                TestDrives = testDrives.Select(t => new TestDriveViewModel
                {
                    Id = t.Id,
                    VehicleId = t.VehicleId,
                    VehicleName = $"{t.Vehicle?.ModelName} {t.Vehicle?.VariantName}",
                    DealerId = t.DealerId,
                    DealerName = t.Dealer?.Name ?? "N/A",
                    DealerAddress = t.Dealer?.Address ?? "N/A",
                    ScheduleTime = t.ScheduleTime,
                    Status = t.Status,
                    Note = t.Note
                }).ToList();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int dealerId, int vehicleId, string date, string time, string? note)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "CUSTOMER")
            {
                return RedirectToPage("/Auth/Login");
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (user == null || user.Role?.Code != "CUSTOMER")
            {
                return RedirectToPage("/Auth/Login");
            }

            // Get or create customer profile
            var customerProfile = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customerProfile == null)
            {
                // Check if profile exists with same email (but UserId is null)
                if (!string.IsNullOrEmpty(user.Email))
                {
                    var existingProfileByEmail = await _context.CustomerProfiles
                        .FirstOrDefaultAsync(c => c.Email == user.Email && c.UserId == null);
                    
                    if (existingProfileByEmail != null)
                    {
                        // Update existing profile to link with this user (only if UserId is null)
                        existingProfileByEmail.UserId = user.Id;
                        existingProfileByEmail.FullName = user.FullName ?? existingProfileByEmail.FullName;
                        existingProfileByEmail.Phone = user.Phone ?? existingProfileByEmail.Phone;
                        customerProfile = existingProfileByEmail;
                        await _context.SaveChangesAsync();
                    }
                }

                // If still no profile, check by phone
                if (customerProfile == null && !string.IsNullOrEmpty(user.Phone))
                {
                    var existingProfileByPhone = await _context.CustomerProfiles
                        .FirstOrDefaultAsync(c => c.Phone == user.Phone && c.UserId == null);
                    
                    if (existingProfileByPhone != null)
                    {
                        // Update existing profile by phone (only if UserId is null)
                        existingProfileByPhone.UserId = user.Id;
                        existingProfileByPhone.FullName = user.FullName ?? existingProfileByPhone.FullName;
                        if (!string.IsNullOrEmpty(user.Email))
                        {
                            // Only set email if it doesn't conflict
                            var emailExists = await _context.CustomerProfiles
                                .AnyAsync(c => c.Email == user.Email && c.Id != existingProfileByPhone.Id);
                            if (!emailExists)
                            {
                                existingProfileByPhone.Email = user.Email;
                            }
                        }
                        customerProfile = existingProfileByPhone;
                        await _context.SaveChangesAsync();
                    }
                }

                // If still no profile, create new one (but skip email if it already exists)
                if (customerProfile == null)
                {
                    // Check if email already exists in another profile
                    string? emailToUse = user.Email;
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        var emailExists = await _context.CustomerProfiles
                            .AnyAsync(c => c.Email == user.Email);
                        if (emailExists)
                        {
                            // Email already taken, don't set it to avoid unique constraint violation
                            emailToUse = null;
                        }
                    }

                    customerProfile = new CustomerProfile
                    {
                        UserId = user.Id,
                        FullName = user.FullName ?? "Khách hàng",
                        Phone = user.Phone ?? "",
                        Email = emailToUse, // May be null if email already exists
                        Address = "",
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.CustomerProfiles.Add(customerProfile);
                    await _context.SaveChangesAsync();
                }
            }

            // Load dealers and vehicles for form (in case of error, need to reload page)
            var activeDealers = await _dealerService.GetActiveDealersAsync();
            AllDealers = activeDealers.Select(d => new DealerSimple
            {
                Id = d.Id,
                Name = d.Name,
                Address = d.Address
            }).ToList();

            var availableVehicles = await _vehicleService.GetAvailableVehiclesAsync();
            AllVehicles = availableVehicles.Select(v => new VehicleSimple
            {
                Id = v.Id,
                Name = v.ModelName + " " + v.VariantName
            }).ToList();

            // Load test drives for display
            var testDrives = await _testDriveService.GetTestDrivesByCustomerIdAsync(customerProfile.Id);
            TestDrives = testDrives.Select(t => new TestDriveViewModel
            {
                Id = t.Id,
                VehicleId = t.VehicleId,
                VehicleName = $"{t.Vehicle?.ModelName} {t.Vehicle?.VariantName}",
                DealerId = t.DealerId,
                DealerName = t.Dealer?.Name ?? "N/A",
                DealerAddress = t.Dealer?.Address ?? "N/A",
                ScheduleTime = t.ScheduleTime,
                Status = t.Status,
                Note = t.Note
            }).ToList();

            // Validate inputs
            if (dealerId <= 0 || vehicleId <= 0 || string.IsNullOrEmpty(date) || string.IsNullOrEmpty(time))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin.";
                TempData["KeepModalOpen"] = "true";
                return Page();
            }

            // Parse date and time
            if (!DateTime.TryParse(date, out var scheduleDate))
            {
                TempData["Error"] = "Ngày không hợp lệ.";
                TempData["KeepModalOpen"] = "true";
                return Page();
            }

            if (!TimeSpan.TryParse(time, out var scheduleTime))
            {
                TempData["Error"] = "Giờ không hợp lệ.";
                TempData["KeepModalOpen"] = "true";
                return Page();
            }

            var scheduleDateTime = scheduleDate.Date.Add(scheduleTime);

            // Validate schedule time is in the future
            if (scheduleDateTime < DateTime.Now)
            {
                TempData["Error"] = "Thời gian đặt lịch phải trong tương lai.";
                TempData["KeepModalOpen"] = "true";
                return Page();
            }

            // Validate dealer exists and is active
            var dealer = await _dealerService.GetDealerByIdAsync(dealerId);
            if (dealer == null || dealer.Status != "ACTIVE")
            {
                TempData["Error"] = "Đại lý không tồn tại hoặc không hoạt động.";
                TempData["KeepModalOpen"] = "true";
                return Page();
            }

            // Validate vehicle exists and is available
            var vehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId);
            if (vehicle == null || vehicle.Status != "AVAILABLE")
            {
                TempData["Error"] = "Mẫu xe không tồn tại hoặc không có sẵn.";
                TempData["KeepModalOpen"] = "true";
                return Page();
            }

            // Create test drive using service
            var testDrive = new TestDrive
            {
                CustomerId = customerProfile.Id,
                DealerId = dealerId,
                VehicleId = vehicleId,
                ScheduleTime = scheduleDateTime,
                Status = "REQUESTED",
                Note = note
            };

            await _testDriveService.CreateTestDriveAsync(testDrive);

            TempData["Success"] = "Đặt lịch lái thử thành công! Đại lý sẽ xác nhận và liên hệ với bạn.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelAsync(int testDriveId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "CUSTOMER")
            {
                return RedirectToPage("/Auth/Login");
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (user == null || user.Role?.Code != "CUSTOMER")
            {
                return RedirectToPage("/Auth/Login");
            }

            // Get customer profile
            var customerProfile = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customerProfile == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToPage();
            }

            // Get test drive
            var testDrive = await _testDriveService.GetTestDriveByIdAsync(testDriveId);
            if (testDrive == null)
            {
                TempData["Error"] = "Không tìm thấy lịch lái thử này.";
                return RedirectToPage();
            }

            // Verify ownership
            if (testDrive.CustomerId != customerProfile.Id)
            {
                TempData["Error"] = "Bạn không có quyền hủy lịch này.";
                return RedirectToPage();
            }

            // Check if can be cancelled
            if (testDrive.Status == "DONE" || testDrive.Status == "CANCELLED")
            {
                TempData["Error"] = "Lịch này không thể hủy.";
                return RedirectToPage();
            }

            // Check if past
            if (testDrive.ScheduleTime < DateTime.Now)
            {
                TempData["Error"] = "Không thể hủy lịch đã qua.";
                return RedirectToPage();
            }

            // Cancel test drive
            await _testDriveService.UpdateTestDriveStatusAsync(testDriveId, "CANCELLED");

            TempData["Success"] = "Đã hủy lịch lái thử thành công.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int testDriveId, int dealerId, int vehicleId, string date, string time, string? note)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "CUSTOMER")
            {
                return RedirectToPage("/Auth/Login");
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (user == null || user.Role?.Code != "CUSTOMER")
            {
                return RedirectToPage("/Auth/Login");
            }

            // Get customer profile
            var customerProfile = await _context.CustomerProfiles
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customerProfile == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToPage();
            }

            // Get test drive
            var testDrive = await _testDriveService.GetTestDriveByIdAsync(testDriveId);
            if (testDrive == null)
            {
                TempData["Error"] = "Không tìm thấy lịch lái thử này.";
                return RedirectToPage();
            }

            // Verify ownership
            if (testDrive.CustomerId != customerProfile.Id)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa lịch này.";
                return RedirectToPage();
            }

            // Check if can be edited
            if (testDrive.Status == "DONE" || testDrive.Status == "CANCELLED")
            {
                TempData["Error"] = "Lịch này không thể chỉnh sửa.";
                return RedirectToPage();
            }

            // Validate inputs
            if (dealerId <= 0 || vehicleId <= 0 || string.IsNullOrEmpty(date) || string.IsNullOrEmpty(time))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin.";
                return RedirectToPage();
            }

            // Parse date and time
            if (!DateTime.TryParse(date, out var scheduleDate))
            {
                TempData["Error"] = "Ngày không hợp lệ.";
                return RedirectToPage();
            }

            if (!TimeSpan.TryParse(time, out var scheduleTime))
            {
                TempData["Error"] = "Giờ không hợp lệ.";
                return RedirectToPage();
            }

            var scheduleDateTime = scheduleDate.Date.Add(scheduleTime);

            // Validate schedule time is in the future
            if (scheduleDateTime < DateTime.Now)
            {
                TempData["Error"] = "Thời gian đặt lịch phải trong tương lai.";
                return RedirectToPage();
            }

            // Validate dealer exists and is active
            var dealer = await _dealerService.GetDealerByIdAsync(dealerId);
            if (dealer == null || dealer.Status != "ACTIVE")
            {
                TempData["Error"] = "Đại lý không tồn tại hoặc không hoạt động.";
                return RedirectToPage();
            }

            // Validate vehicle exists and is available
            var vehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId);
            if (vehicle == null || vehicle.Status != "AVAILABLE")
            {
                TempData["Error"] = "Mẫu xe không tồn tại hoặc không có sẵn.";
                return RedirectToPage();
            }

            // Update test drive
            testDrive.DealerId = dealerId;
            testDrive.VehicleId = vehicleId;
            testDrive.ScheduleTime = scheduleDateTime;
            testDrive.Note = note;
            // If was CONFIRMED and edited, change back to REQUESTED for dealer to reconfirm
            if (testDrive.Status == "CONFIRMED")
            {
                testDrive.Status = "REQUESTED";
            }

            await _testDriveService.UpdateTestDriveAsync(testDrive);

            TempData["Success"] = "Cập nhật lịch lái thử thành công! Đại lý sẽ xác nhận lại và liên hệ với bạn.";
            return RedirectToPage();
        }

        public class TestDriveViewModel
        {
            public int Id { get; set; }
            public int VehicleId { get; set; }
            public string VehicleName { get; set; } = "";
            public int DealerId { get; set; }
            public string DealerName { get; set; } = "";
            public string DealerAddress { get; set; } = "";
            public DateTime ScheduleTime { get; set; }
            public string Status { get; set; } = "";
            public string? Note { get; set; }
        }

        public class DealerSimple
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Address { get; set; } = "";
        }

        public class VehicleSimple
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }
    }
}


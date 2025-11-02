using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vehicle_Dealer_Management.BLL.IService;
using Vehicle_Dealer_Management.DAL.Models;
using D = Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.Pages.EVM.Dealers
{
    public class EditModel : PageModel
    {
        private readonly IDealerService _dealerService;
        private readonly IActivityLogService _activityLogService;

        public EditModel(
            IDealerService dealerService,
            IActivityLogService activityLogService)
        {
            _dealerService = dealerService;
            _activityLogService = activityLogService;
        }

        public D.Dealer? Dealer { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            if (!id.HasValue)
            {
                return RedirectToPage("/EVM/Dealers");
            }

            Dealer = await _dealerService.GetDealerByIdAsync(id.Value);
            if (Dealer == null)
            {
                TempData["Error"] = "Không tìm thấy đại lý này.";
                return RedirectToPage("/EVM/Dealers");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(
            int id,
            string name,
            string address,
            string? phoneNumber,
            string? email,
            string status)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Validate
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address))
            {
                ErrorMessage = "Vui lòng nhập đầy đủ tên đại lý và địa chỉ.";
                Dealer = await _dealerService.GetDealerByIdAsync(id);
                return Page();
            }

            // Validate status
            if (string.IsNullOrWhiteSpace(status))
            {
                status = "ACTIVE";
            }

            // Get existing dealer
            var dealer = await _dealerService.GetDealerByIdAsync(id);
            if (dealer == null)
            {
                TempData["Error"] = "Không tìm thấy đại lý này.";
                return RedirectToPage("/EVM/Dealers");
            }

            // Update dealer
            dealer.Name = name.Trim();
            dealer.Address = address.Trim();
            dealer.PhoneNumber = phoneNumber?.Trim();
            dealer.Email = email?.Trim();
            dealer.Status = status;
            dealer.IsActive = status == "ACTIVE";
            dealer.UpdatedDate = DateTime.UtcNow;

            await _dealerService.UpdateDealerAsync(dealer);

            // Log activity
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdStr))
            {
                var userIdInt = int.Parse(userIdStr);
                var userRole = HttpContext.Session.GetString("UserRole") ?? "EVM_STAFF";
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                
                await _activityLogService.LogActivityAsync(
                    userId: userIdInt,
                    action: "UPDATE",
                    entityType: "Dealer",
                    entityId: dealer.Id,
                    entityName: dealer.Name,
                    description: "Đã cập nhật thông tin đại lý",
                    userRole: userRole,
                    ipAddress: ipAddress);
            }

            TempData["Success"] = "Đã cập nhật thông tin đại lý thành công!";
            return RedirectToPage("/EVM/Dealers");
        }
    }
}


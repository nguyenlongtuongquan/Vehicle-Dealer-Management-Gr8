using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.Pages.Dealer
{
    public class FeedbackModel : PageModel
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackModel(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        public string TypeFilter { get; set; } = "all";
        public int TotalFeedback { get; set; }
        public int NewCount { get; set; }
        public int InProgressCount { get; set; }
        public int ResolvedCount { get; set; }

        public List<FeedbackViewModel> Feedbacks { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? type)
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            TypeFilter = type ?? "all";
            var dealerIdInt = int.Parse(dealerId);

            // Get feedbacks using service
            IEnumerable<Vehicle_Dealer_Management.DAL.Models.Feedback> feedbacks;

            if (TypeFilter != "all")
            {
                feedbacks = await _feedbackService.GetFeedbacksByTypeAsync(TypeFilter, dealerIdInt);
            }
            else
            {
                feedbacks = await _feedbackService.GetFeedbacksByDealerIdAsync(dealerIdInt);
            }

            var feedbacksList = feedbacks.ToList();

            TotalFeedback = feedbacksList.Count;
            NewCount = feedbacksList.Count(f => f.Status == "NEW");
            InProgressCount = feedbacksList.Count(f => f.Status == "IN_PROGRESS");
            ResolvedCount = feedbacksList.Count(f => f.Status == "RESOLVED");

            // Sử dụng navigation properties đã được include sẵn trong repository
            // Tránh concurrent access đến DbContext
            Feedbacks = feedbacksList.Select(f => new FeedbackViewModel
            {
                Id = f.Id,
                CustomerName = f.Customer?.FullName ?? "N/A",
                Type = f.Type,
                Status = f.Status,
                Content = f.Content,
                CreatedAt = f.CreatedAt,
                ReplyContent = f.ReplyContent,
                ReplyByUserName = f.ReplyByUser?.FullName ?? "N/A",
                ReplyAt = f.ReplyAt
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostStartProcessAsync(int id)
        {
            await _feedbackService.UpdateFeedbackStatusAsync(id, "IN_PROGRESS");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostResolveAsync(int id)
        {
            await _feedbackService.UpdateFeedbackStatusAsync(id, "RESOLVED");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReplyAsync(int id, string replyContent)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var dealerId = HttpContext.Session.GetString("DealerId");
            if (string.IsNullOrEmpty(dealerId))
            {
                return RedirectToPage("/Auth/Login");
            }

            if (string.IsNullOrWhiteSpace(replyContent))
            {
                TempData["Error"] = "Nội dung trả lời không được để trống";
                return RedirectToPage();
            }

            try
            {
                var userIdInt = int.Parse(userId);
                await _feedbackService.ReplyToFeedbackAsync(id, replyContent, userIdInt);
                TempData["Success"] = "Trả lời phản hồi thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToPage();
        }

        public class FeedbackViewModel
        {
            public int Id { get; set; }
            public string CustomerName { get; set; } = "";
            public string Type { get; set; } = "";
            public string Status { get; set; } = "";
            public string Content { get; set; } = "";
            public DateTime CreatedAt { get; set; }
            public string? ReplyContent { get; set; }
            public string ReplyByUserName { get; set; } = "";
            public DateTime? ReplyAt { get; set; }
        }
    }
}


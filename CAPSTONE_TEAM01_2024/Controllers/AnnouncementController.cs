using CAPSTONE_TEAM01_2024.Models;
using CAPSTONE_TEAM01_2024.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Drawing.Printing;
using CAPSTONE_TEAM01_2024.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Drawing;
using System.Security.Claims;

namespace CAPSTONE_TEAM01_2024.Controllers
{
	public class AnnouncementController : Controller
	{
// Database Context
		private readonly ApplicationDbContext _context;
		public AnnouncementController(ApplicationDbContext context)
		{
			_context = context;
		}
// AnnouncementList Actions
	//Render view
		public async Task<IActionResult> AnnouncementList(int pageIndex = 1, int pageSize = 20)
		{
			var announcements = _context.Announcements.OrderByDescending(a => a.StartTime).AsQueryable();
			var paginatedAnnouncements = await PaginatedList<Announcement>.CreateAsync(announcements, pageIndex, pageSize);
			ViewBag.Warning = TempData["Warning"];
			ViewBag.Success = TempData["Success"];
			ViewBag.Error = TempData["Error"];
			return View(paginatedAnnouncements);
		}
    //Add Announcement
        [HttpPost]
        public async Task<IActionResult> CreateAnnouncement(Announcement model)
        {
            var existingAnnouncement = await _context.Announcements.FirstOrDefaultAsync(a =>
                a.Detail == model.Detail &&
                a.StartTime == model.StartTime &&
                a.EndTime == model.EndTime &&
                a.Type == model.Type);

            if (existingAnnouncement == null)
            {
                _context.Announcements.Add(model);
                await _context.SaveChangesAsync();

                // Fetch all classes
                var classes = await _context.Classes.Include(c => c.Advisor).ToListAsync();

                // Convert year terms to DateTime
                var relevantAdvisors = classes
                    .Where(c =>
                    {
                        var termParts = c.Term.Split('-');
                        if (termParts.Length == 2 &&
                            int.TryParse(termParts[0], out int termStartYear) &&
                            int.TryParse(termParts[1], out int termEndYear))
                        {
                            var termStart = new DateTime(termStartYear, 1, 1);
                            var termEnd = new DateTime(termEndYear, 12, 31);
                            return termStart <= model.StartTime && termEnd >= model.StartTime;
                        }
                        return false;
                    })
                    .Select(c => c.Advisor)
                    .Where(a => a != null) // Ensure advisors are not null
                    .Distinct()
                    .ToList();

                foreach (var advisor in relevantAdvisors)
                {
                    if (advisor != null)
                    {
                        var userAnnouncement = new UserAnnouncement
                        {
                            UserId = advisor.Id,
                            AnnouncementId = model.Id,
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.UserAnnouncements.Add(userAnnouncement);
                    }
                }
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Thông báo '{model.Detail}' đã được tạo thành công và thông báo cho cố vấn liên quan!";
                return RedirectToAction("AnnouncementList");
            }

            TempData["Error"] = "Thông báo trùng lặp hoặc đã xảy ra lỗi.";
            return RedirectToAction("AnnouncementList");
        }
    //Edit Announcement
        [HttpPost]
		public async Task<IActionResult> EditAnnouncement(Announcement model)
		{
			var similarAnnouncement = await _context.Announcements.AnyAsync(a =>
				a.Id != model.Id && // Exclude the current announcement
				a.Detail == model.Detail &&
				a.StartTime == model.StartTime &&
				a.EndTime == model.EndTime &&
				a.Type == model.Type);

			if (!similarAnnouncement)
			{
				var announcement = await _context.Announcements.FindAsync(model.Id);
				if (announcement != null)
				{
					announcement.Detail = model.Detail;
					announcement.StartTime = model.StartTime;
					announcement.EndTime = model.EndTime;
					announcement.Type = model.Type;
					await _context.SaveChangesAsync();
					TempData["Success"] = $"Thông báo '{model.Detail}' đã được cập nhật thành công!";
					return RedirectToAction("AnnouncementList");
				}
			}
			TempData["Error"] = "Thông báo trùng lặp hoặc đã xảy ra lỗi.";
			return RedirectToAction("AnnouncementList");
		}
	//Delete Announcement
		[HttpPost]
		public async Task<IActionResult> DeleteAnnouncement(int id)
		{
			var announcement = await _context.Announcements.FindAsync(id);
			if (announcement != null)
			{
				_context.Announcements.Remove(announcement);
				await _context.SaveChangesAsync();
				TempData["Success"] = $"Thông báo '{announcement.Detail}' đã được xóa thành công!";
				return RedirectToAction("AnnouncementList");
			}
			TempData["Error"] = "Đã xảy ra lỗi khi xóa thông báo";
			return RedirectToAction("AnnouncementList");
		}
    //Search Announcement
        public async Task<IActionResult> SearchAnnouncements(string query)
        {
            var announcements = await _context.Announcements
                .Where(a => a.Detail.Contains(query) ||
                            a.Type.Contains(query) ||
                            a.StartTime.ToString().Contains(query) ||
                            a.EndTime.ToString().Contains(query))
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();
            return PartialView("_AnnouncementTable", announcements);
        }
		public async Task<IActionResult> AnnouncementListPartial()
		{
			var announcements = await _context.Announcements.OrderByDescending(a => a.StartTime).ToListAsync();
			return PartialView("_AnnouncementTable", announcements);
		}
	//Notification system
		public async Task<IActionResult> GetNewAnnouncementsCount()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var newAnnouncementsCount = await _context.UserAnnouncements
				.Where(ua => ua.UserId == userId && !ua.IsRead)
				.CountAsync();
			return Json(new { count = newAnnouncementsCount });
		}
        [HttpPost]
        public async Task<IActionResult> MarkAnnouncementAsRead(int announcementId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userAnnouncement = await _context.UserAnnouncements
                .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AnnouncementId == announcementId);

            if (userAnnouncement != null)
            {
                userAnnouncement.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> MarkAllAnnouncementsAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userAnnouncements = await _context.UserAnnouncements
                .Where(ua => ua.UserId == userId && !ua.IsRead)
                .ToListAsync();

            foreach (var ua in userAnnouncements)
            {
                ua.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

    }
}
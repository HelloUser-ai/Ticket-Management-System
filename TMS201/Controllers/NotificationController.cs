using Microsoft.AspNetCore.Mvc;
using TMS201.Data;

namespace TMS201.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetNotifications(int page = 1)
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrWhiteSpace(username))
                return Json(new { data = new List<object>(), total = 0 });

            var cleanUser = username.Trim().ToLower();

            int pageSize = 15;

            var query = _context.Notifications
                .Where(x => (x.UserName ?? string.Empty).ToLower() == cleanUser)
                .OrderByDescending(x => x.CreatedDate);

            var total = query.Count(); // 🔥 total count badge ke liye

            var data = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    title = x.Title,
                    message = x.Message
                })
                .ToList();

            return Json(new { data, total }); // 🔥 IMPORTANT
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notif = await _context.Notifications.FindAsync(id);
            if (notif != null)
            {
                notif.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

    }
}
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.EntityFrameworkCore;
//using TMS201.Data;
//using TMS201.Hubs; // NotificationHub ke liye
//using TMS201.Models;

//namespace TMS201.Controllers
//{
//    public class DashboardController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IHubContext<NotificationHub> _hubContext; // 👈 SignalR zaroori hai

//        public DashboardController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
//        {
//            _context = context;
//            _hubContext = hubContext;
//        }

//        public async Task<IActionResult> Index(string search, string status, DateTime? fromDate, DateTime? toDate, string viewType = "assigned")
//        {
//            var role = HttpContext.Session.GetString("Role");
//            var username = HttpContext.Session.GetString("Username");

//            if (string.IsNullOrEmpty(role)) return RedirectToAction("Login", "Account");

//            // Hum ApplyFilters method ka use karenge taaki code clean rahe
//            var tickets = ApplyFilters(search, status, fromDate, toDate, (role == "Employee" && viewType == "assigned") ? username : null);

//            // 🔹 COUNTS (Top Cards)
//            ViewBag.Total = tickets.Count;
//            ViewBag.PendingCount = tickets.Count(x => x.TicketStatus == "Pending");
//            ViewBag.CompletedCount = tickets.Count(x => x.TicketStatus == "Completed");

//            // 🔥 DYNAMIC STATUS GROUPING
//            var statusData = tickets
//                .GroupBy(x => x.TicketStatus ?? "Unknown")
//                .Select(g => new { Name = g.Key, Count = g.Count() })
//                .ToList();

//            ViewBag.StatusLabels = statusData.Select(x => x.Name).ToList();
//            ViewBag.StatusCounts = statusData.Select(x => x.Count).ToList();

//            // 📈 TREND (Last 7 Days)
//            var last7Days = Enumerable.Range(0, 7)
//                .Select(i => DateTime.Today.AddDays(-i))
//                .OrderBy(d => d).ToList();

//            ViewBag.ChartDates = last7Days.Select(d => d.ToString("dd MMM")).ToList();
//            ViewBag.ChartCounts = last7Days.Select(date => tickets.Count(t => t.TicketDate.Date == date.Date)).ToList();

//            // 🏭 PLANT TYPE COUNT
//            var plantData = tickets.GroupBy(x => x.PlantName ?? "N/A")
//                                   .Select(g => new { Name = g.Key, Count = g.Count() }).ToList();
//            ViewBag.PlantNames = plantData.Select(x => x.Name).ToList();
//            ViewBag.PlantCounts = plantData.Select(x => x.Count).ToList();

//            // 👨‍💼 EMPLOYEE WORKLOAD
//            var empData = tickets.Where(x => !string.IsNullOrEmpty(x.AssignedTo))
//                                 .GroupBy(x => x.AssignedTo)
//                                 .Select(g => new { Name = g.Key, Count = g.Count() })
//                                 .OrderByDescending(x => x.Count).ToList();
//            ViewBag.Employees = empData.Select(x => x.Name).ToList();
//            ViewBag.EmployeeCounts = empData.Select(x => x.Count).ToList();

//            // KEEP FILTER VALUES
//            ViewBag.Search = search;
//            ViewBag.Status = status;
//            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
//            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
//            ViewBag.ViewType = viewType;

//            return View();
//        }

//        // 🔍 Filter Logic (Fixed Return Type)
//        private List<Ticket> ApplyFilters(string search, string status, DateTime? fromDate, DateTime? toDate, string? assignedTo)
//        {
//            var query = _context.Tickets.AsQueryable();

//            if (!string.IsNullOrWhiteSpace(search))
//            {
//                query = query.Where(x =>
//                    x.TicketNo.Contains(search) ||
//                    x.ClientName.Contains(search) ||
//                    x.PlantName.Contains(search) ||
//                    x.PlantType.Contains(search)
//                );
//            }

//            if (!string.IsNullOrWhiteSpace(assignedTo))
//                query = query.Where(x => x.AssignedTo == assignedTo);

//            if (!string.IsNullOrWhiteSpace(status))
//                query = query.Where(x => x.TicketStatus == status);

//            if (fromDate.HasValue)
//                query = query.Where(x => x.TicketDate >= fromDate.Value.Date);

//            if (toDate.HasValue)
//                query = query.Where(x => x.TicketDate < toDate.Value.Date.AddDays(1));

//            return query.OrderByDescending(x => x.Id).ToList();
//        }

//        // Notification Helper (Aapne controller mein use kiya tha toh yahan bhi chahiye)
//        private async Task SendNotification(string title, string message, string user)
//        {
//            if (string.IsNullOrEmpty(user)) return;
//            var notif = new Notification
//            {
//                Title = title,
//                Message = message,
//                UserName = user.ToLower(),
//                CreatedDate = DateTime.Now,
//                IsRead = false
//            };
//            _context.Notifications.Add(notif);
//            await _context.SaveChangesAsync();
//            await _hubContext.Clients.Group(user.ToLower()).SendAsync("ReceiveNotification", new { title, message });
//        }
//    }
//}
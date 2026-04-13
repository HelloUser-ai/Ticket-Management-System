using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMS201.Data;
using TMS201.Filters;
using TMS201.Models;

namespace TMS201.Controllers
{
    [AuthFilter]
    public class TicketBackupController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketBackupController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1) // 1. Page parameter add kiya
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "SuperAdmin") return Unauthorized();

            // 🛠️ Automatic Cleanup (Same as before)
            var cutoffDate = DateTime.Now.AddDays(-15);
            var expiredRecords = _context.TicketBackups.Where(x => x.DeletedDate < cutoffDate);
            if (await expiredRecords.AnyAsync())
            {
                _context.TicketBackups.RemoveRange(expiredRecords);
                await _context.SaveChangesAsync();
            }

            // --- PAGINATION LOGIC START ---
            int pageSize = 10; // Ek page par kitne dikhane hain
            var query = _context.TicketBackups.AsQueryable();
            
            int totalItems = await query.CountAsync(); // 2. Total count nikalna zaroori hai

            var data = await query
                .OrderByDescending(x => x.DeletedDate)
                .Skip((page - 1) * pageSize) // 3. Skip purane records
                .Take(pageSize)              // 4. Take sirf 10 records
                .ToListAsync();

            // 5. ViewBag data populate karein
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.PageSize = pageSize;
            // --- PAGINATION LOGIC END ---

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "SuperAdmin") return Unauthorized();

            var backup = await _context.TicketBackups.FindAsync(id);
            if (backup == null)
            {
                TempData["Error"] = "Bhai, record nahi mila!";
                return RedirectToAction(nameof(Index));
            }

            // 🛑 Expiry Check 
            if (backup.DeletedDate < DateTime.Now.AddDays(-15))
            {
                _context.TicketBackups.Remove(backup);
                await _context.SaveChangesAsync();
                TempData["Error"] = "Record expired ho chuka tha, isliye delete kar diya.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var ticket = new Ticket
                {
                    // ... tera mapping logic (ekdum sahi hai) ...
                    TicketNo = backup.TicketNo,
                    ClientName = backup.ClientName,
                    // etc.
                };

                _context.Tickets.Add(ticket);
                _context.TicketBackups.Remove(backup);

                await _context.SaveChangesAsync();
                TempData["Success"] = "Ticket # " + backup.TicketNo + " restore ho gaya!";
            }
            catch (Exception ex)
            {
                // Agar database error aaye toh pata chale
                TempData["Error"] = "Restore fail: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
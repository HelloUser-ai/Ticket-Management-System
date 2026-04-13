using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TMS201.Data;
using TMS201.Filters;
using TMS201.Hubs;
using TMS201.Models;
using TMS201.Services.Interfaces;
using System.IO;
using System.Net.Http.Headers; // Ye ContentDisposition ke liye hai (agar use karna ho)

namespace TMS201.Controllers
{
    [AuthFilter]
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IReportService _reportService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IExcelImportService _excelService;
        private readonly ILogger<TicketController> _logger;


        public TicketController(
     ApplicationDbContext context,
     IReportService reportService,
     IHubContext<NotificationHub> hubContext,
     IExcelImportService excelService,
     ILogger<TicketController> logger)
        {
            _context = context;
            _reportService = reportService;
            _hubContext = hubContext;
            _excelService = excelService;
            _logger = logger;
        }



        // 🔍 INDEX + FILTER
        // 🔍 INDEX + FILTER
        public async Task<IActionResult> Index(string search, string status, string clientName, DateTime? fromDate, DateTime? toDate, int page = 1, string viewType = "assigned")
        {
            LoadDropdowns(); // Dropdowns populate karne ke liye (Clients, etc.)

            int pageSize = 10;
            var role = HttpContext.Session.GetString("Role") ?? "Employee";
            var username = HttpContext.Session.GetString("Username");

            // 🛠️ 1. ARCHIVE LOGIC 
            // Humne 3 din pehle ki date nikaali
            var cutoffDate = DateTime.Now.AddDays(-3);

            // Filter Logic:
            // Ticket dikhao agar:
            // - Status "Completed" NAHO 
            // - YA PHIR (Status "Completed" HAI aur UpdatedDate 3 din se zyada purani nahi hai)
            var query = _context.Tickets.Where(x =>
                x.TicketStatus != "Completed" ||
                (x.TicketStatus == "Completed" && (x.UpdatedDate == null || x.UpdatedDate > cutoffDate))
            ).AsQueryable();

            // 🔥 2. ROLE BASED FILTERING
            if (role == "Employee")
            {
                // Agar Employee 'Assigned' view dekh raha hai, toh sirf uski tickets dikhao
                if (viewType == "assigned")
                {
                    query = query.Where(x => x.AssignedTo == username);
                }
                // Agar viewType 'all' hai, toh saari active tickets dikhengi (Archive logic upar lag chuka hai)
            }

            // 🔍 3. SEARCH (Plant Name, Ticket No, ya Task Details)
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                 
                    x.PlantName.Contains(search) 
                  
                );
            }

            // 🏢 4. CLIENT FILTER
            if (!string.IsNullOrWhiteSpace(clientName))
            {
                query = query.Where(x => x.ClientName == clientName);
            }

            // 📊 5. STATUS FILTER (Dropdown wala)
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.TicketStatus == status);
            }

            // 📅 6. DATE RANGE FILTER
            if (fromDate.HasValue)
            {
                query = query.Where(x => x.TicketDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                // AddDays(1) isliye taaki us din ki tickets bhi cover ho jayein (time factor)
                query = query.Where(x => x.TicketDate < toDate.Value.Date.AddDays(1));
            }

            // 🚀 7. SORTING & PAGINATION
            query = query.OrderByDescending(x => x.Id);

            int totalItems = await query.CountAsync();
            var tickets = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // 🎁 8. VIEW DATA FOR UI
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.PageSize = pageSize;

            ViewBag.ClientFilter = clientName;
            ViewBag.PlantFilter = search; // Search box value
            ViewBag.StatusFilter = status;
            ViewBag.FromDateFilter = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDateFilter = toDate?.ToString("yyyy-MM-dd");
            ViewBag.ViewType = viewType;

            return View(tickets);
        }

        // ➕ CREATE
        public IActionResult Create()
        {
            LoadDropdowns();
            return View(new Ticket { TicketDate = DateTime.Now });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52428800)]
        public async Task<IActionResult> Create(Ticket ticket, List<IFormFile>? attachments)
        {
            // 1. Validation Clean-up
            ModelState.Remove("TicketNo");
            ModelState.Remove("SerialNo");
            ModelState.Remove("attachments");
            ModelState.Remove("Attachments");

            if (!ModelState.IsValid)
            {
                LoadDropdowns();
                return View(ticket);
            }

            // 2. Database Check
            if (_context == null)
                return Content("Database context is not initialized.");

            // 3. Ticket Number Generation
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            int lastSerial = await _context.Tickets
                .Where(x => x.CreatedDate >= today && x.CreatedDate < tomorrow)
                .OrderByDescending(x => x.SerialNo)
                .Select(x => (int?)x.SerialNo)
                .FirstOrDefaultAsync() ?? 0;

            ticket.SerialNo = lastSerial + 1;
            ticket.TicketNo = GenerateTicketNo(ticket.SerialNo);

            // 4. Default Values
            ticket.TicketStatus ??= "Pending";

            string? sessionUser = HttpContext.Session?.GetString("Username");
            ticket.CreatedBy = !string.IsNullOrEmpty(sessionUser) ? sessionUser : "Admin";

            ticket.CreatedDate = DateTime.Now;
            ticket.UpdatedBy = ticket.CreatedBy;
            ticket.UpdatedDate = ticket.CreatedDate;

            // 5. FILE UPLOAD LOGIC
            if (attachments != null && attachments.Any())
            {
                ticket.Attachments ??= new List<TicketAttachment>();

                string subFolder = Path.Combine("uploads", ticket.TicketNo ?? "Temp");
                string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", subFolder);

                if (!Directory.Exists(rootPath))
                {
                    Directory.CreateDirectory(rootPath);
                }

                foreach (var file in attachments)
                {
                    if (file != null && file.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName).Replace(" ", "_");

                        string finalFilePath = Path.Combine(rootPath, uniqueFileName);

                        using (var stream = new FileStream(finalFilePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        ticket.Attachments.Add(new TicketAttachment
                        {
                            FileName = file.FileName,
                            FilePath = "/" + subFolder.Replace("\\", "/") + "/" + uniqueFileName,
                            FileType = file.ContentType,
                            FileSize = file.Length,
                            UploadedDate = DateTime.Now
                        });
                    }
                }
            }

            // 6. Database Save
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            // 7. Notification
            try
            {
                await SendNotification("New Ticket", $"Ticket {ticket.TicketNo} created", ticket.AssignedTo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification failed for ticket {TicketNo}", ticket.TicketNo);
            }

            TempData["Success"] = "Ticket created successfully!";

            return RedirectToAction("Index");
        }
        // ✏️ EDIT
        // GET: Edit
        public IActionResult Edit(int id)
        {
            var ticket = _context.Tickets.FirstOrDefault(x => x.Id == id);
            if (ticket == null) return NotFound();

            LoadDropdowns(); // important
            return View(ticket);
        }


        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52428800)]
        public async Task<IActionResult> Edit(Ticket ticket, List<IFormFile>? attachments) // attachments parameter add kiya
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns();
                return View(ticket);
            }

            // .Include use karna zaroori hai taaki purane attachments delete na ho jayein list update karte waqt
            var existing = await _context.Tickets
                .Include(t => t.Attachments)
                .FirstOrDefaultAsync(x => x.Id == ticket.Id);

            if (existing == null) return NotFound();

            // 1. Fields Update
            existing.ClientName = ticket.ClientName;
            existing.PlantName = ticket.PlantName;
            existing.PlantType = ticket.PlantType;
            existing.GivenBy = ticket.GivenBy;
            existing.AssignedTo = ticket.AssignedTo;
            existing.TaskDetails = ticket.TaskDetails;
            existing.TicketStatus = ticket.TicketStatus;

            // Audit
            existing.UpdatedBy = HttpContext.Session.GetString("Username") ?? "Admin";
            existing.UpdatedDate = DateTime.Now;

            // 2. Multiple File Upload Logic
            if (attachments != null && attachments.Count > 0)
            {
                // Ticket number ke hisaab se folder path
                string subFolder = Path.Combine("uploads", existing.TicketNo ?? "Temp");
                string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", subFolder);

                if (!Directory.Exists(rootPath))
                {
                    Directory.CreateDirectory(rootPath);
                }

                foreach (var file in attachments)
                {
                    if (file.Length > 0)
                    {
                        // Unique file name generate karna
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName).Replace(" ", "_");
                        string finalFilePath = Path.Combine(rootPath, uniqueFileName);

                        using (var stream = new FileStream(finalFilePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Database entry for attachment
                        existing.Attachments.Add(new TicketAttachment
                        {
                            FileName = file.FileName,
                            FilePath = "/" + subFolder.Replace("\\", "/") + "/" + uniqueFileName,
                            FileType = file.ContentType,
                            FileSize = file.Length,
                            UploadedDate = DateTime.Now
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            // 🔔 NOTIFICATION
            try
            {
                await SendNotification(
                    "Ticket Updated",
                    $"{existing.UpdatedBy} updated Ticket {existing.TicketNo}",
                    existing.AssignedTo
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification failed while updating ticket {TicketNo}", existing.TicketNo);
            }

            TempData["Success"] = "Ticket updated successfully!";

            return RedirectToAction("Index");
        }


        [HttpPost, ActionName("Delete")]
        [RoleFilter]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return RedirectToAction("Index");

            var backup = new TicketBackup
            {
                OriginalTicketId = ticket.Id,
                SerialNo = ticket.SerialNo,
                TicketNo = ticket.TicketNo,
                TicketDate = ticket.TicketDate,
                ClientName = ticket.ClientName,
                PlantType = ticket.PlantType,
                PlantName = ticket.PlantName,
                GivenBy = ticket.GivenBy,
                AssignedTo = ticket.AssignedTo,
                TaskDetails = ticket.TaskDetails,
                TicketStatus = ticket.TicketStatus,
                CreatedBy = ticket.CreatedBy,
                CreatedDate = ticket.CreatedDate,
                UpdatedBy = ticket.UpdatedBy,
                UpdatedDate = ticket.UpdatedDate,
                DeletedBy = HttpContext.Session.GetString("Username") ?? "Admin",
                DeletedDate = DateTime.Now
            };

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.TicketBackups.Add(backup);
                _context.Tickets.Remove(ticket);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            // 🔔 NOTIFICATION
            await SendNotification(
       "Ticket Deleted",
       $"{backup.DeletedBy} deleted Ticket {backup.TicketNo}",
       backup.AssignedTo
   );
            TempData["Success"] = "Ticket deleted successfully!";
            return RedirectToAction("Index");
        }



        // 🔢 Ticket No
        private string GenerateTicketNo(int serial)
        {
            return $"{DateTime.Now:ddMMyyyy}-{serial:D3}";
        }

        public IActionResult Dashboard(string search, string status, DateTime? fromDate, DateTime? toDate, string viewType = "assigned")
        {
            var role = HttpContext.Session.GetString("Role");
            var username = HttpContext.Session.GetString("Username");

            var query = _context.Tickets.AsQueryable();

            // 🔥 ROLE BASED FILTER
            if (role == "Employee")
            {
                query = query.Where(x => x.AssignedTo == username);
            }

            // 🔥 ROLE BASED FILTER
            if (!string.IsNullOrWhiteSpace(search))
            {
                var words = search.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                query = query.Where(t =>
                    words.Any(w =>
                        (t.PlantName != null && t.PlantName.ToLower().Contains(w)) ||
                        (t.PlantType != null && t.PlantType.ToLower().Contains(w)) ||
                        (t.ClientName != null && t.ClientName.ToLower().Contains(w))
                    )
                );
            }

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(x => x.TicketStatus == status);

            if (fromDate.HasValue)
                query = query.Where(x => x.TicketDate >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(x => x.TicketDate < toDate.Value.Date.AddDays(1));

            var tickets = query.ToList();

            // 🔹 COUNTS (Top Cards)
            ViewBag.Total = tickets.Count;
            ViewBag.PendingCount = tickets.Count(x => x.TicketStatus == "Pending");
            ViewBag.CompletedCount = tickets.Count(x => x.TicketStatus == "Completed");

            // 🔥 DYNAMIC STATUS GROUPING (For Bar & Pie Charts)
            var statusData = tickets
                .GroupBy(x => x.TicketStatus ?? "Unknown")
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.StatusLabels = statusData.Select(x => x.Name).ToList();
            ViewBag.StatusCounts = statusData.Select(x => x.Count).ToList();

            // 📈 TREND (Last 7 Days)
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i))
                .OrderBy(d => d).ToList();

            ViewBag.ChartDates = last7Days.Select(d => d.ToString("dd MMM")).ToList();
            ViewBag.ChartCounts = last7Days.Select(date => tickets.Count(t => t.TicketDate.Date == date.Date)).ToList();

            // 🏭 PLANT TYPE COUNT
            var plantData = tickets.GroupBy(x => x.PlantName ?? "N/A")
                                   .Select(g => new { Name = g.Key, Count = g.Count() }).ToList();
            ViewBag.PlantNames = plantData.Select(x => x.Name).ToList();
            ViewBag.PlantCounts = plantData.Select(x => x.Count).ToList();

            // 👨‍💼 EMPLOYEE WORKLOAD
            var empData = tickets.Where(x => !string.IsNullOrEmpty(x.AssignedTo))
                                 .GroupBy(x => x.AssignedTo)
                                 .Select(g => new { Name = g.Key, Count = g.Count() })
                                 .OrderByDescending(x => x.Count).ToList();
            ViewBag.Employees = empData.Select(x => x.Name).ToList();
            ViewBag.EmployeeCounts = empData.Select(x => x.Count).ToList();

            // KEEP FILTER VALUES
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.ViewType = viewType;

            return View();
        }

        // 📥 EXPORT WITH FILTERS
        public IActionResult ExportExcel(string search, string status, string assignedTo, string clientName, DateTime? fromDate, DateTime? toDate, string viewType)
        {
            var role = HttpContext.Session.GetString("Role");
            var username = HttpContext.Session.GetString("Username");

            // Fix: Yahan parameters ka sequence ApplyFilters ke mutabiq set kiya (6 parameters)
            var data = ApplyFilters(search, status, fromDate, toDate, clientName, assignedTo);

            // 🔥 Employee Logic for Export
            if (role == "Employee" && viewType == "assigned")
            {
                data = data.Where(x => x.AssignedTo == username).ToList();
            }

            var bytes = _reportService.ExportToExcel(data);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Tickets_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        public IActionResult ExportPdf(string search, string status, string assignedTo, string clientName, DateTime? fromDate, DateTime? toDate, string viewType)
        {
            var role = HttpContext.Session.GetString("Role");
            var username = HttpContext.Session.GetString("Username");

            // Fix: Yahan bhi parameters match kiye
            var data = ApplyFilters(search, status, fromDate, toDate, clientName, assignedTo);

            if (role == "Employee" && viewType == "assigned")
            {
                data = data.Where(x => x.AssignedTo == username).ToList();
            }

            var bytes = _reportService.ExportToPdf(data);
            return File(bytes, "application/pdf", $"Tickets_{DateTime.Now:yyyyMMdd}.pdf");
        }

        private List<Ticket> ApplyFilters(string search, string status, DateTime? fromDate, DateTime? toDate, string clientName, string assignedTo)
        {
            var query = _context.Tickets.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.TicketNo.Contains(search) || x.PlantName.Contains(search));
            }

            // Dropdown filter logic (Existing code)
            if (!string.IsNullOrWhiteSpace(clientName))
            {
                query = query.Where(x => x.ClientName == clientName);
            }

            // Fix: assignedTo ka filter query mein missing tha, wo add kiya
            if (!string.IsNullOrWhiteSpace(assignedTo))
            {
                query = query.Where(x => x.AssignedTo == assignedTo);
            }

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(x => x.TicketStatus == status);

            if (fromDate.HasValue)
                query = query.Where(x => x.TicketDate >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(x => x.TicketDate < toDate.Value.Date.AddDays(1));

            return query.OrderByDescending(x => x.Id).ToList();
        }

        // 📦 DROPDOWNS (🔥 FIXED FULL)
        private void LoadDropdowns()
        {
            // 🔥 AB SEEDHA CLIENT MASTER TABLE SE NAMES AAYENGE
            ViewBag.ClientList = _context.Clients
                .Where(c => c.IsActive) // Sirf active clients dikhane ke liye
                .OrderBy(c => c.Name)
                .Select(c => c.Name)
                .ToList();

            // Baaki dropdowns pehle jaise hi
            ViewBag.PlantNames = new List<string> { "BMC SCADA", "Gujrat SCADA", "Private SCADA" };
            ViewBag.PlantTypes = new List<string> { "RMC", "ASP" };

            var employees = _context.Employees
                .Where(e => !string.IsNullOrEmpty(e.Name))
                .Select(e => e.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            ViewBag.AssignedToList = employees;
            //ViewBag.GivenByList = employees;
        }


        [HttpPost]
        public async Task<IActionResult> AssignTicket([FromBody] AssignModel model)
        {
            var ticket = _context.Tickets.FirstOrDefault(x => x.Id == model.Id);

            if (ticket == null)
                return Json(new { success = false });

            ticket.AssignedTo = model.AssignedTo;
            ticket.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            var assignedBy = HttpContext.Session.GetString("Username");

            // 🔔 Assigned user ko notify
            await SendNotification(
                "Task Assigned",
                $"{assignedBy} assigned Ticket {ticket.TicketNo} to you",
                ticket.AssignedTo
            );

            // 🔔 Admins ko bhi notify
            var admins = _context.Users
                .Where(u => u.Role == "Admin" || u.Role == "SuperAdmin")
                .ToList();

            foreach (var admin in admins)
            {
                if (admin.Username == assignedBy) continue; // duplicate avoid

                await SendNotification(
                    "Ticket Assigned",
                    $"{assignedBy} assigned Ticket {ticket.TicketNo} to {ticket.AssignedTo}",
                    admin.Username
                );
            }

            return Json(new { success = true });
        }




        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody] StatusModel model)
        {
            var ticket = _context.Tickets.FirstOrDefault(x => x.Id == model.Id);
            if (ticket == null) return Json(new { success = false });

            string oldStatus = ticket.TicketStatus;
            ticket.TicketStatus = model.Status;
            ticket.UpdatedDate = DateTime.Now;

            var updatedBy = HttpContext.Session.GetString("Username");
            ticket.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync();

            // 🔔 Assigned user ko notify
            await SendNotification(
                "Ticket Status Updated",
                $"Ticket {ticket.TicketNo} moved to {model.Status}",
                ticket.AssignedTo
            );

            // 🔔 Admins ko notify
            var admins = _context.Users
                .Where(u => u.Role == "Admin" || u.Role == "SuperAdmin")
                .ToList();

            foreach (var admin in admins)
            {
                if (admin.Username == updatedBy) continue; // duplicate avoid

                await SendNotification(
                    "Ticket Status Updated",
                    $"Ticket {ticket.TicketNo} moved from {oldStatus} to {model.Status} by {updatedBy}",
                    admin.Username
                );
            }

             TempData["Success"] = "Ticket updated successfully!";

            return Json(new { success = true });
        }



        private async Task SendNotification(string title, string message, string user)
        {
            if (string.IsNullOrWhiteSpace(user))
                return;

            var cleanUser = user.Trim().ToLower();

            var notif = new Notification
            {
                Title = title,
                Message = message,
                UserName = cleanUser,
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(cleanUser)
                .SendAsync("ReceiveNotification", new
                {
                    title = title,
                    message = message
                });

            Console.WriteLine("Sending to user: " + cleanUser);
        }

        public IActionResult Details(int id)
        {
            var ticket = _context.Tickets.FirstOrDefault(x => x.Id == id);
            if (ticket == null) return NotFound();
            return View(ticket);
        }
        [HttpPost]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            var result = await _excelService.ImportTickets(file);

            if (!result.success)
            {
                TempData["error"] = result.message;
                return RedirectToAction("Index");
            }

            // 🛠️ FIX: Loop lagakar check karenge ki kaunsi tickets 'Completed' hain
            if (result.data != null)
            {
                foreach (var ticket in result.data)
                {
                    // Agar Excel se status 'Completed' aa raha hai
                    if (ticket.TicketStatus == "Completed")
                    {
                        // Isko aaj ki date de do taaki ye Index page par 3 din tak dikhe
                        ticket.UpdatedDate = DateTime.Now;
                    }
                    else
                    {
                        // Baki tickets ke liye bhi UpdatedDate set karna safe rehta hai
                        ticket.UpdatedDate = DateTime.Now;
                    }
                }

                await _context.Tickets.AddRangeAsync(result.data);
                await _context.SaveChangesAsync();

                TempData["success"] = $"🔥 {result.data.Count} Tickets Imported Successfully";
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        [RequestSizeLimit(52428800)] // 50 MB Limit
        public async Task<IActionResult> UploadAttachment(int ticketId, List<IFormFile> attachments) // IFormFile ki jagah List kiya
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null || attachments == null || attachments.Count == 0)
            {
                return RedirectToAction("ViewAttachments", new { id = ticketId });
            }

            // TicketNo ke hisaab se folder (Professional tarika)
            string subFolder = Path.Combine("uploads", ticket.TicketNo ?? "Temp");
            string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", subFolder);

            if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);

            foreach (var file in attachments)
            {
                if (file.Length > 0)
                {
                    // Unique Name taaki overwrite na ho
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName).Replace(" ", "_");
                    string finalFilePath = Path.Combine(rootPath, uniqueFileName);

                    using (var stream = new FileStream(finalFilePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Database Entry
                    var attachment = new TicketAttachment
                    {
                        TicketId = ticketId,
                        FileName = file.FileName,
                        FilePath = "/" + subFolder.Replace("\\", "/") + "/" + uniqueFileName,
                        FileType = file.ContentType,
                        FileSize = file.Length,
                        UploadedDate = DateTime.Now
                    };

                    _context.TicketAttachments.Add(attachment);
                }
            }

            await _context.SaveChangesAsync();

            // Wapas usi page pe redirect (ViewAttachments ya Details jaha tu chahe)
            return RedirectToAction("ViewAttachments", new { id = ticketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAttachment(int id)
        {
            var file = _context.TicketAttachments.Find(id);

            if (file == null) return NotFound();

            // TicketId pehle hi save kar lo taaki baad mein redirect kar sakein
            int ticketId = file.TicketId;

            // 1. Physical file delete karna
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + file.FilePath.Replace("/", "\\"));

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            // 2. Database entry remove karna
            _context.TicketAttachments.Remove(file);
            _context.SaveChanges();

            // 3. Wapas usi Attachment wale page par bhejo (Behtar UX)
            return RedirectToAction("ViewAttachments", new { id = ticketId });
        }


        public async Task<IActionResult> ViewAttachments(int id)
        {
            // id yahan TicketId hai
            var attachments = await _context.TicketAttachments
                                            .Where(a => a.TicketId == id)
                                            .ToListAsync();

            if (attachments == null || !attachments.Any())
            {
                // Agar koi attachment nahi hai toh error dikhao ya Index pe bhejo
                TempData["error"] = "No attachments found for this ticket.";
                return RedirectToAction("Index");
            }

            ViewBag.TicketId = id;
            return View(attachments); // Iske liye tujhe ek View banana padega
        }

        public async Task<IActionResult> CompletedHistory(int page = 1)
        {
            var cutoffDate = DateTime.Now.AddDays(-3);
            var query = _context.Tickets.Where(x =>
                x.TicketStatus == "Completed" && x.UpdatedDate <= cutoffDate
            ).OrderByDescending(x => x.UpdatedDate);

            // Pagination logic same Index jaisa hi rahega
            int pageSize = 10;
            var tickets = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return View(tickets);
        }

    }
}

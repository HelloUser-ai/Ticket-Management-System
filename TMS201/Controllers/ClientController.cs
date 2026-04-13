using Microsoft.AspNetCore.Mvc;
using TMS201.Data;
using TMS201.Filters;
using TMS201.Models;

namespace TMS201.Controllers
{
    [AuthFilter]
    [RoleFilter]
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ClientController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(int page = 1) // 1. Page parameter liya
        {
            int pageSize = 10; // 2. Ek page par kitne records dikhane hain

            var query = _db.Clients.AsQueryable();

            int totalItems = query.Count(); // 3. Total counts calculate kiye

            var clients = query
                .OrderByDescending(c => c.Id)
                .Skip((page - 1) * pageSize) // 4. Purane records skip kiye
                .Take(pageSize)              // 5. Sirf 10 records liye
                .ToList();

            // 6. ViewBag mein data bhara taaki Partial View use kar sake
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(clients);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Client obj)
        {
            if (string.IsNullOrWhiteSpace(obj.Email)) obj.Email = null;
            if (string.IsNullOrWhiteSpace(obj.ContactPerson)) obj.ContactPerson = null;

            if (ModelState.IsValid)
            {
                _db.Clients.Add(obj);
                _db.SaveChanges();
                TempData["success"] = "Client added successfully!";
            }
            return RedirectToAction("Index");
        }

        // --- EDIT SECTION START ---

        // 1. GET: Fetch client data for Modal
        [HttpGet]
        public IActionResult GetClientDetails(int id)
        {
            var client = _db.Clients.Find(id);
            if (client == null) return NotFound();

            // JSON bhej rahe hain taaki JavaScript modal fill kar sake
            return Json(new
            {
                id = client.Id,
                name = client.Name,
                contactPerson = client.ContactPerson,
                email = client.Email,
                isActive = client.IsActive
            });
        }

        // 2. POST: Save updated data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Client obj)
        {
            if (string.IsNullOrWhiteSpace(obj.Email)) obj.Email = null;
            if (string.IsNullOrWhiteSpace(obj.ContactPerson)) obj.ContactPerson = null;

            if (ModelState.IsValid)
            {
                _db.Clients.Update(obj);
                _db.SaveChanges();
                TempData["success"] = "Client updated successfully!";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        // --- EDIT SECTION END ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var client = _db.Clients.Find(id);
            if (client != null)
            {
                _db.Clients.Remove(client);
                _db.SaveChanges();
                TempData["success"] = "Client removed!";
            }
            return RedirectToAction("Index");
        }
    }
}
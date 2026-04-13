using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TMS201.Data; 
using TMS201.Models; 

namespace TMS201.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1)
        {
            int pageSize = 10;

            var totalUsers = _context.Users.Count();

            var users = _context.Users
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

            return View(users);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public IActionResult Login(string Username, string Password)
        {
            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
            {
             
                var user = _context.Users.FirstOrDefault(u => u.Username == Username && u.Password == Password);
                if (user != null)
                {
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Role", user.Role);

                    return RedirectToAction("Index", "Ticket");
                }

                ViewBag.Error = "Invalid username or password";
                return View();
            }

            ViewBag.Error = "Please enter username and password";
            return View();
        }

       
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult Profile()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login");

            // 1. User Table se data nikalo
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            // 2. Employee Table se match karo (Designation nikalne ke liye)
            var employee = _context.Employees.FirstOrDefault(e => e.Name == username);

            // 3. ViewModel mein data fill karo
            var viewModel = new UserProfileViewModel
            {
                UserData = user,
                EmployeeId = employee?.Id ?? 0,
                EmployeeName = employee?.Name ?? "N/A", // Yahan comma lagana mat bhulna
                Designation = employee?.Designation ?? "Not Assigned"
            };

            return View(viewModel);
        }

        // GET: Add User Page
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        // POST: Save User Logic
        [HttpPost]
        public async Task<IActionResult> Add(User model)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Add(model);
                await _context.SaveChangesAsync();
                TempData["success"] = "User Added Successfully! 🎉";
                return RedirectToAction("Index"); // Wapas user list par bhej dega
            }
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> GetUserPassword(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user != null)
            {
                return Json(new { success = true, password = user.Password });
            }
            return Json(new { success = false, message = "Username not found!" });
        }


    }
}
using Microsoft.AspNetCore.Mvc;
using TMS201.Data;
using TMS201.Filters;
using TMS201.Models;

[AuthFilter]
[RoleFilter]
public class EmployeeController : Controller
{
    private readonly ApplicationDbContext _context;

    public EmployeeController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 🔹 LIST
    public IActionResult Index(int page = 1)
    {
        int pageSize = 10;

        var totalRecords = _context.Employees.Count();

        var employees = _context.Employees
            .OrderBy(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

        return View(employees);
    }

    // 🔹 CREATE GET
    public IActionResult Create()
    {
        return View();
    }

    // 🔹 CREATE POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Employee emp)
    {
        if (ModelState.IsValid)
        {
            emp.CreatedDate = DateTime.Now;
            _context.Employees.Add(emp);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(emp);
    }

    // 🔹 EDIT GET
    public IActionResult Edit(int id)
    {
        var emp = _context.Employees.Find(id);
        if (emp == null) return NotFound();

        return View(emp);
    }

    // 🔹 EDIT POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Employee emp)
    {
        if (ModelState.IsValid)
        {
            _context.Employees.Update(emp);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(emp);
    }

    // 🔹 DELETE
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var emp = _context.Employees.Find(id);
        if (emp != null)
        {
            _context.Employees.Remove(emp);
            _context.SaveChanges();
        }
        return RedirectToAction("Index");
    }
}

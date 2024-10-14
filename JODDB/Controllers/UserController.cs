using Microsoft.AspNetCore.Mvc;
using JODDB.Data;
using JODDB.Models;
using System.Linq;
using OfficeOpenXml;

namespace JODDB.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // GET: User List
        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }
        // GET: Create User Form
public IActionResult Create()
{
    return View();
}

// POST: Add New User
[HttpPost]
public IActionResult Create(User user)
{
    if (ModelState.IsValid)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }
    return View(user);
}
[HttpPost]
public IActionResult Import(IFormFile file)
{
    if (file != null && file.Length > 0)
    {
        using (var package = new ExcelPackage(file.OpenReadStream()))
        {
            var worksheet = package.Workbook.Worksheets[0];
            for (int row = 2; row <= worksheet.Dimension.Rows; row++)
            {
                var user = new User
                {
                    Name = worksheet.Cells[row, 1].Text,
                    Email = worksheet.Cells[row, 2].Text,
                    MobileNumber = worksheet.Cells[row, 3].Text,
                    Password = Guid.NewGuid().ToString() // Auto-generate password
                };
                _context.Users.Add(user);
            }
            _context.SaveChanges();
        }
    }
    return RedirectToAction("Index");
}

    }
}

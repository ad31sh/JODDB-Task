using Microsoft.AspNetCore.Mvc;
using JODDB.Data;
using JODDB.Models;
using System.Linq;
using OfficeOpenXml;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System;
using System.IO;

namespace JODDB.Controllers
{
     [Authorize]  // This will require users to be logged in
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

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
  // GET: Import Users View
        public IActionResult Import()
        {
            return View();
        }

       [HttpPost]
public async Task<IActionResult> Import(IFormFile file)
{
    if (file == null || file.Length == 0)
    {
        ModelState.AddModelError(string.Empty, "Please upload a valid Excel file.");
        return View();
    }

    var users = new List<User>();
    var invalidRows = 0;

    using (var stream = new MemoryStream())
    {
        await file.CopyToAsync(stream);

        using (var package = new ExcelPackage(stream))
        {
            var worksheet = package.Workbook.Worksheets[0];  // First worksheet
            var rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var name = worksheet.Cells[row, 2].Text;     // Name (Column B)
                var email = worksheet.Cells[row, 3].Text;    // Email (Column C)
                var mobileNo = worksheet.Cells[row, 4].Text; // MobileNo (Column D)

                // Validate the data
                if (!IsValidName(name) || !IsValidEmail(email) || string.IsNullOrWhiteSpace(mobileNo))
                {
                    invalidRows++;
                    continue;  // Skip invalid rows
                }

                // Generate random password
                var password = GeneratePassword();

                var user = new User
                {
                    Name = name,
                    Email = email,
                    MobileNumber = mobileNo,
                    Password = password,  // Assuming you have a Password field in your User model
                    Photo = "default.png"  // Placeholder for Photo
                };

                users.Add(user);
            }
        }
    }

    // Bulk insert
    if (users.Any())
    {
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        ViewBag.Message = $"{users.Count} users imported successfully!";
    }
    else
    {
        ViewBag.Message = "No valid users were imported.";
    }

    ViewBag.InvalidRows = invalidRows;
    return View();
}

// Helper function to generate random passwords
private string GeneratePassword()
{
    var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    var random = new Random();
    return new string(Enumerable.Repeat(characters, 8).Select(s => s[random.Next(s.Length)]).ToArray());
}

// Helper function to validate name
private bool IsValidName(string name)
{
    return !string.IsNullOrWhiteSpace(name);
}

// Helper function to validate email format
private bool IsValidEmail(string email)
{
    try
    {
        var addr = new System.Net.Mail.MailAddress(email);
        return addr.Address == email;
    }
    catch
    {
        return false;
    }
}
        
    }
}

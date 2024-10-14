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
 // GET: Excel Import Page
        public IActionResult Import()
        {
            return View();
        }

         // POST: Handle Excel File Upload and Import Users
        [HttpPost]
        public IActionResult Import(IFormFile excelFile)
        {
            if (excelFile != null && excelFile.Length > 0)
            {
                try
                {
                    using (var stream = new MemoryStream())
                    {
                        excelFile.CopyTo(stream);
                        using (var package = new ExcelPackage(stream))
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                            if (worksheet != null)
                            {
                                int rowCount = worksheet.Dimension.Rows;
                                int batchSize = 500;
                                List<User> usersBatch = new List<User>();

                                for (int row = 2; row <= rowCount; row++) // Start from row 2 to skip the header
                                {
                                    var name = worksheet.Cells[row, 1].Value?.ToString().Trim();
                                    var email = worksheet.Cells[row, 2].Value?.ToString().Trim();
                                    var mobile = worksheet.Cells[row, 3].Value?.ToString().Trim();

                                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email))
                                    {
                                        var password = GeneratePassword();

                                        var user = new User
                                        {
                                            Name = name,
                                            Email = email,
                                            MobileNumber = mobile,
                                            Password = password
                                        };

                                        usersBatch.Add(user);

                                        // Save batch of users when the batch size is reached
                                        if (usersBatch.Count >= batchSize)
                                        {
                                            _context.Users.AddRange(usersBatch);
                                            _context.SaveChanges();
                                            usersBatch.Clear();
                                        }
                    Console.WriteLine($"Name: {name}, Email: {email}, Mobile: {mobile}");
                }
                                }

                                // Save any remaining users after the loop
                                if (usersBatch.Count > 0)
                                {
                                    _context.Users.AddRange(usersBatch);
                                    _context.SaveChanges();
                                }
                            }
                        }
                    }

                    return RedirectToAction("Index"); // Redirect back to user list after successful import
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while processing the file: " + ex.Message);
                }
            }
            else
            {
                ModelState.AddModelError("", "Please select a valid Excel file.");
            }

            return View(); // Return to the import page if there was an error
        }

        // Helper function to generate random passwords
        private string GeneratePassword()
        {
            // You can improve this logic to generate stronger passwords
            var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(characters, 8).Select(s => s[random.Next(s.Length)]).ToArray());
        }
         // GET: Edit User
        public IActionResult Edit(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();  // Return 404 if user is not found
            }
            return View(user);
        }

        // POST: Edit User
        [HttpPost]
        public IActionResult Edit(User user)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Update(user);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Delete Confirmation Page
        public IActionResult Delete(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();  // Return 404 if user is not found
            }
            return View(user);
        }

        // POST: Delete User
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();  // Return 404 if user is not found
            }
            _context.Users.Remove(user);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}

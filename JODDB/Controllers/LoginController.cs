using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using JODDB.Models;
using JODDB.Data; // Import your data context for DB access
using System.Linq;

namespace JODDB.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context; // Add the context to access the database

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Login Page
        public IActionResult Index()
        {
            return View();
        }

        // POST: Handle Login Form Submission
        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check the database for a user with the provided Name and Password
                var user = _context.Users.FirstOrDefault(u => u.Name == model.Username && u.Password == model.Password);

                if (user != null)
                {
                    // If user exists and password matches, create claims and sign the user in
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Name)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    // Sign in the user
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return RedirectToAction("Index", "User");  // Redirect to User List after login
                }

                // If login failed, show an error message
                ModelState.AddModelError("", "Invalid username or password.");
            }
            return View(model);
        }

        // Logout functionality
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }
    }
}

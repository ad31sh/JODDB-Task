using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using JODDB.Models;

namespace JODDB.Controllers
{
    public class LoginController : Controller
    {
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
                // Simple example of login validation, replace this with real logic (like checking against a DB)
                if (model.Username == "admin" && model.Password == "password")
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.Username)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    // Sign in the user
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return RedirectToAction("Index", "User");  // Redirect to User List after login
                }

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

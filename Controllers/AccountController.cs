using Microsoft.AspNetCore.Mvc;
using QLDuLichRBAC_Upgrade.Models;
using QLDuLichRBAC_Upgrade.Models.Entities;
using QLDuLichRBAC_Upgrade.Utils;

namespace QLDuLichRBAC_Upgrade.Controllers
{
    public class AccountController : Controller
    {
        private readonly QLJumaparenaContext _context;
        public AccountController(QLJumaparenaContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string Username, string Password)
        {
            // Validate input
            var validation = ValidationHelper.ValidateLogin(Username, Password);
            if (!validation.IsValid)
            {
                ViewBag.ErrorAlert = AlertHelper.Error(validation.ErrorMessage);
                return View();
            }

            // Sanitize input
            Username = AuthHelper.SanitizeInput(Username);
            
            // Hash password và ki?m tra
            string hashed = AuthHelper.HashPassword(Password);
            
            // Debug logging
            Console.WriteLine($"Username: {Username}");
            Console.WriteLine($"Password Hash: {hashed}");
            
            var user = _context.User
                .FirstOrDefault(u => u.Username == Username);
            
            if (user != null)
            {
                Console.WriteLine($"User found. DB Hash: {user.PasswordHash}");
                Console.WriteLine($"Hash match: {user.PasswordHash == hashed}");
            }
            else
            {
                Console.WriteLine("User not found in database");
            }

            user = _context.User
                .FirstOrDefault(u => u.Username == Username && u.PasswordHash == hashed);

            if (user == null)
            {
                ViewBag.ErrorAlert = AlertHelper.Error("Invalid username or password!");
                return View();
            }

            // Set session
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("FullName", user.FullName);

            return user.Role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Staff" => RedirectToAction("MySchedule", "Staff"),
                "Customer" => RedirectToAction("BookTickets", "Customer"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using QLDuLichRBAC_Upgrade.Models;
using QLDuLichRBAC_Upgrade.Models.Entities;
using QLDuLichRBAC_Upgrade.Utils;
using System.Linq;

namespace QLDuLichRBAC_Upgrade.Controllers
{
    public class RegisterController : Controller
    {
        private readonly QLDuLichContext _context;
        public RegisterController(QLDuLichContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Views/Account/Register.cshtml");
        }

        [HttpPost]
        public IActionResult Index(string Username, string Password, string FullName, string Email, string Phone)
        {
            // Validate tất cả các trường
            var validation = ValidationHelper.ValidateRegistration(Username, Password, FullName, Email, Phone);
            if (!validation.IsValid)
            {
                ViewBag.ErrorAlert = AlertHelper.Error(validation.ErrorMessage);
                return View("~/Views/Account/Register.cshtml");
            }

            // Sanitize input
            Username = AuthHelper.SanitizeInput(Username);
            FullName = AuthHelper.SanitizeInput(FullName);
            Email = AuthHelper.SanitizeInput(Email);
            Phone = AuthHelper.SanitizeInput(Phone);

            // Kiểm tra username đã tồn tại
            if (_context.Users.Any(u => u.Username == Username))
            {
                ViewBag.ErrorAlert = AlertHelper.Error("Username already exists!");
                return View("~/Views/Account/Register.cshtml");
            }

            // Kiểm tra email đã tồn tại
            if (_context.Users.Any(u => u.Email == Email))
            {
                ViewBag.ErrorAlert = AlertHelper.Error("Email already in use!");
                return View("~/Views/Account/Register.cshtml");
            }

            // Hash password
            string hash = AuthHelper.HashPassword(Password);

            var user = new User
            {
                Username = Username,
                PasswordHash = hash,
                FullName = FullName,
                Email = Email,
                Phone = Phone,
                Role = "Customer"
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            // Tạo bản ghi KhachHang liên kết
            var newKH = new KhachHang
            {
                MaKH = _context.KhachHang.Any() ? _context.KhachHang.Max(k => k.MaKH) + 1 : 1,
                TenKH = FullName ?? Username,
                GioiTinh = "Nam",
                SDT = Phone ?? "",
                DiaChi = "",
                CCCD = null,
                UserId = user.UserId
            };
            _context.KhachHang.Add(newKH);
            _context.SaveChanges();

            ViewBag.SuccessAlert = AlertHelper.Success("Registration successful! Please login to continue.");
            return View("~/Views/Account/Register.cshtml");
        }
    }
}

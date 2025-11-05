using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDuLichRBAC_Upgrade.Models;
using QLDuLichRBAC_Upgrade.Models.Entities;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace QLDuLichRBAC_Upgrade.Controllers
{
    public class GuideController : Controller
    {
        private readonly QLDuLichContext _context;

        public GuideController(QLDuLichContext context)
        {
            _context = context;
        }

        // ================================
        // Index - Redirect to AvailableTours
        // ================================
        public IActionResult Index()
        {
            return RedirectToAction("AvailableTours");
        }

        // ================================
        // 1. Danh sách Tour có thể đăng ký
        // ================================
        public IActionResult AvailableTours()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = _context.Users
                .Include(u => u.NhanVien)
                .FirstOrDefault(u => u.Username == username);

            if (user?.NhanVien == null)
                return RedirectToAction("Login", "Account");

            int maNV = user.NhanVien.MaNV;

            // Lấy danh sách tour chưa đăng ký và còn hạn
            var today = DateTime.Today;
            var registeredTourIds = _context.NhanVien_Tour
                .Where(nt => nt.MaNV == maNV)
                .Select(nt => nt.MaTour)
                .ToList();

            var availableTours = _context.Tour
                .Include(t => t.Tour_Anh)
                .Include(t => t.Tour_DiaDiem).ThenInclude(td => td.DiaDiem)
                .Include(t => t.NhanVien_Tour).ThenInclude(nt => nt.NhanVien)
                .Where(t => t.NgayBatDau >= today && !registeredTourIds.Contains(t.MaTour))
                .ToList();

            ViewBag.GuideId = maNV;
            return View(availableTours);
        }

        // ================================
        // 2. Đăng ký làm hướng dẫn viên cho tour
        // ================================
        [HttpPost]
        public IActionResult RegisterForTour(int tourId)
        {
            try
            {
                var username = HttpContext.Session.GetString("Username");
                if (string.IsNullOrEmpty(username))
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });

                var user = _context.Users
                    .Include(u => u.NhanVien)
                    .FirstOrDefault(u => u.Username == username);

                if (user?.NhanVien == null)
                    return Json(new { success = false, message = "Không tìm thấy thông tin nhân viên" });

                int maNV = user.NhanVien.MaNV;

                // Kiểm tra xem đã đăng ký chưa
                var exists = _context.NhanVien_Tour
                    .Any(nt => nt.MaNV == maNV && nt.MaTour == tourId);

                if (exists)
                    return Json(new { success = false, message = "Bạn đã đăng ký tour này rồi" });

                // Đăng ký tour
                var registration = new NhanVien_Tour
                {
                    MaNV = maNV,
                    MaTour = tourId
                };

                _context.NhanVien_Tour.Add(registration);
                _context.SaveChanges();

                return Json(new { success = true, message = "Đăng ký thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // ================================
        // 3. Danh sách Tour đã đăng ký và lương
        // ================================
        public IActionResult RegisteredTours()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = _context.Users
                .Include(u => u.NhanVien)
                .FirstOrDefault(u => u.Username == username);

            if (user?.NhanVien == null)
                return RedirectToAction("Login", "Account");

            int maNV = user.NhanVien.MaNV;

            var registeredTours = _context.NhanVien_Tour
                .Where(nt => nt.MaNV == maNV)
                .Include(nt => nt.Tour)
                    .ThenInclude(t => t.Tour_Anh)
                .Include(nt => nt.Tour)
                    .ThenInclude(t => t.Tour_DiaDiem)
                    .ThenInclude(td => td.DiaDiem)
                .OrderByDescending(nt => nt.Tour.NgayBatDau)
                .ToList();

            return View(registeredTours);
        }

        // ================================
        // 4. Hủy đăng ký tour
        // ================================
        [HttpPost]
        public IActionResult UnregisterTour(int tourId)
        {
            try
            {
                var username = HttpContext.Session.GetString("Username");
                if (string.IsNullOrEmpty(username))
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });

                var user = _context.Users
                    .Include(u => u.NhanVien)
                    .FirstOrDefault(u => u.Username == username);

                if (user?.NhanVien == null)
                    return Json(new { success = false, message = "Không tìm thấy thông tin nhân viên" });

                int maNV = user.NhanVien.MaNV;

                var registration = _context.NhanVien_Tour
                    .FirstOrDefault(nt => nt.MaNV == maNV && nt.MaTour == tourId);

                if (registration == null)
                    return Json(new { success = false, message = "Không tìm thấy đăng ký" });

                _context.NhanVien_Tour.Remove(registration);
                _context.SaveChanges();

                return Json(new { success = true, message = "Hủy đăng ký thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // ================================
        // 5. Hồ sơ cá nhân
        // ================================
        public IActionResult Profile()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = _context.Users
                .Include(u => u.NhanVien)
                .FirstOrDefault(u => u.Username == username);

            if (user?.NhanVien == null)
                return RedirectToAction("Login", "Account");

            ViewBag.User = user;
            return View(user.NhanVien);
        }

        // ================================
        // 6. Cập nhật hồ sơ
        // ================================
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(NhanVien model, IFormFile? avatarFile)
        {
            try
            {
                var nhanVien = _context.NhanVien.Find(model.MaNV);
                if (nhanVien == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin nhân viên.";
                    return RedirectToAction("Profile");
                }

                var username = HttpContext.Session.GetString("Username");
                var user = _context.Users.FirstOrDefault(u => u.Username == username);

                // Xử lý upload ảnh
                if (avatarFile != null && avatarFile.Length > 0)
                {
                    // Kiểm tra định dạng file
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["Error"] = "Chỉ chấp nhận file ảnh (.jpg, .jpeg, .png, .gif)";
                        return RedirectToAction("Profile");
                    }

                    // Kiểm tra kích thước file (tối đa 5MB)
                    if (avatarFile.Length > 5 * 1024 * 1024)
                    {
                        TempData["Error"] = "Kích thước file không được vượt quá 5MB";
                        return RedirectToAction("Profile");
                    }

                    // Tên file theo format: {username}_{role}.{extension}
                    var fileName = $"{username}_Guide{extension}";
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
                    
                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Xóa ảnh cũ nếu có (với tên file cũ)
                    if (!string.IsNullOrEmpty(nhanVien.AnhDaiDien))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", nhanVien.AnhDaiDien);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                            catch (Exception ex)
                            {
                                // Log lỗi nhưng vẫn tiếp tục upload ảnh mới
                                Console.WriteLine($"Không thể xóa ảnh cũ: {ex.Message}");
                            }
                        }
                    }

                    // Lưu ảnh mới
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(stream);
                    }

                    // Lưu đường dẫn tương đối vào database
                    nhanVien.AnhDaiDien = $"avatars/{fileName}";
                }

                // Cập nhật thông tin khác
                nhanVien.TenNV = model.TenNV;
                nhanVien.SDT = model.SDT;
                nhanVien.DiaChi = model.DiaChi;
                nhanVien.GioiTinh = model.GioiTinh;

                _context.SaveChanges();
                TempData["Success"] = "Cập nhật thông tin thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToAction("Profile");
        }
    }
}

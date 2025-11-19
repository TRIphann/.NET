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
    public class StaffController : Controller
    {
        private readonly QLJumaparenaContext _context;

        public StaffController(QLJumaparenaContext context)
        {
            _context = context;
        }

        // ================================
        // Index - Redirect to MySchedule
        // ================================
        public IActionResult Index()
        {
            return RedirectToAction("MySchedule");
        }

        // ================================
        // 1. L?ch l�m vi?c c?a nh�n vi�n
        // ================================
        public IActionResult MySchedule(DateTime? startDate)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = _context.User
                .Include(u => u.NhanVien)
                .FirstOrDefault(u => u.Username == username);

            if (user?.NhanVien == null)
                return RedirectToAction("Login", "Account");

            int maNV = user.NhanVien.MaNV;

            // Hỗ trợ chọn tuần/ngày: nếu không truyền startDate thì lấy tuần hiện tại (bắt đầu từ ngày được truyền hoặc ngày hôm nay)
            var start = startDate?.Date ?? DateTime.Today;
            // Chu kỳ hiển thị: 7 ngày bắt đầu từ `start`
            var end = start.AddDays(6);

            var mySchedule = _context.NhanVien_Ca
                .Where(nc => nc.MaNV == maNV && nc.NgayLamViec.Date >= start && nc.NgayLamViec.Date <= end)
                .Include(nc => nc.Ca)
                .OrderBy(nc => nc.NgayLamViec)
                .ThenBy(nc => nc.Ca.GioBatDau)
                .ToList();

            ViewBag.StartDate = start;
            ViewBag.EndDate = end;

            ViewBag.StaffName = user.NhanVien.HoTen;
            return View(mySchedule);
        }

        // API: Đăng ký ca (nhân viên tự đăng ký ca cho ngày cụ thể)
        [HttpPost]
        public IActionResult RegisterShift(DateTime date, int maCa)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return Json(new { success = false, message = "Chưa đăng nhập" });

            var user = _context.User.Include(u => u.NhanVien).FirstOrDefault(u => u.Username == username);
            if (user?.NhanVien == null)
                return Json(new { success = false, message = "Không tìm thấy nhân viên" });

            int maNV = user.NhanVien.MaNV;

            try
            {
                var d = date.Date;
                // Kiểm tra đã có ca này chưa
                var exists = _context.NhanVien_Ca.Any(nc => nc.MaNV == maNV && nc.MaCa == maCa && nc.NgayLamViec.Date == d);
                if (exists)
                    return Json(new { success = false, message = "Bạn đã đăng ký ca này rồi" });

                // Tạo bản ghi mới
                var nvCa = new NhanVien_Ca
                {
                    MaNV = maNV,
                    MaCa = maCa,
                    NgayLamViec = d,
                    TrangThaiDiemDanh = "Chưa điểm danh",
                    DiMuonCoPhep = false
                };
                _context.NhanVien_Ca.Add(nvCa);
                _context.SaveChanges();

                return Json(new { success = true, message = "Đăng ký ca thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // ================================
        // 2. Danh s�ch v� c?n check-in h�m nay
        // ================================
        public IActionResult CheckInTickets()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = _context.User
                .Include(u => u.NhanVien)
                .FirstOrDefault(u => u.Username == username);

            if (user?.NhanVien == null)
                return RedirectToAction("Login", "Account");

            int maNV = user.NhanVien.MaNV;

            // Lấy tất cả vé cần check-in hôm nay
            var today = DateTime.Today;
            var tickets = _context.Ve
                .Where(v => v.NgaySuDung.Date == today &&
                           v.TrangThai == "Đã thanh toán")
                .Include(v => v.GoiDichVu)
                .Include(v => v.KhachHang)
                .OrderBy(v => v.NgayDat)
                .ToList();

            ViewBag.StaffName = user.NhanVien.HoTen;
            return View(tickets);
        }

        // ================================
        // 3. Check-in v�
        // ================================
        [HttpPost]
        public IActionResult CheckIn(string ticketCode)
        {
            try
            {
                var ticket = _context.Ve
                    .Include(v => v.GoiDichVu)
                    .FirstOrDefault(v => v.MaVeCode == ticketCode);

                if (ticket == null)
                    return Json(new { success = false, message = "Không tìm thấy vé" });

                if (ticket.TrangThai == "Đã sử dụng")
                    return Json(new { success = false, message = "Vé đã được sử dụng" });

                if (ticket.NgaySuDung.Date != DateTime.Today)
                    return Json(new { success = false, message = "Vé không hợp lệ cho hôm nay" });

                // Check-in
                ticket.TrangThai = "Đã sử dụng";
                _context.SaveChanges();

                return Json(new { 
                    success = true, 
                    message = "Check-in thành công!",
                    packageName = ticket.GoiDichVu?.TenGoi,
                    timeSlot = "N/A",
                    people = ticket.SoNguoi
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"L?i: {ex.Message}" });
            }
        }

        // ================================
        // 4. Qu?n l� tr� choi v� thi?t b? an to�n
        // ================================
        public IActionResult Activities()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var activities = _context.TroChoi
                .Include(t => t.TroChoi_Anh)
                .Include(t => t.KhuVuiChoi_TroChoi)
                    .ThenInclude(kt => kt.KhuVuiChoi)
                .OrderBy(t => t.DanhMuc)
                .ThenBy(t => t.TenTroChoi)
                .ToList();

            return View(activities);
        }

        // ================================
        // 5. Th?ng k� c�ng vi?c
        // ================================
        public IActionResult WorkStats(string? month)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = _context.User
                .Include(u => u.NhanVien)
                .FirstOrDefault(u => u.Username == username);

            if (user?.NhanVien == null)
                return RedirectToAction("Login", "Account");

            int maNV = user.NhanVien.MaNV;

            // Parse month parameter (format: yyyy-MM) or use current month
            DateTime targetDate;
            if (!string.IsNullOrEmpty(month) && DateTime.TryParse(month + "-01", out targetDate))
            {
                // Month parameter is valid
            }
            else
            {
                targetDate = DateTime.Now;
            }

            var firstDayOfMonth = new DateTime(targetDate.Year, targetDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var monthlyShifts = _context.NhanVien_Ca
                .Where(nc => nc.MaNV == maNV && 
                           nc.NgayLamViec >= firstDayOfMonth && 
                           nc.NgayLamViec <= lastDayOfMonth)
                .Include(nc => nc.Ca)
                .OrderBy(nc => nc.NgayLamViec)
                .ToList();

            // Calculate statistics
            int totalShifts = monthlyShifts.Count;
            double totalHours = 0;
            foreach (var shift in monthlyShifts)
            {
                if (shift.Ca != null)
                {
                    var duration = shift.Ca.GioKetThuc.ToTimeSpan() - shift.Ca.GioBatDau.ToTimeSpan();
                    totalHours += duration.TotalHours;
                }
            }

            // Get total customers served (tickets in the same dates)
            var shiftDates = monthlyShifts.Select(s => s.NgayLamViec.Date).Distinct().ToList();
            
            int totalCustomers = _context.Ve
                .Where(v => shiftDates.Contains(v.NgaySuDung.Date) && 
                           v.TrangThai == "Đã sử dụng")
                .Sum(v => (int?)v.SoNguoi) ?? 0;

            // Calculate performance score (attendance rate)
            int performanceScore = totalShifts > 0 ? Math.Min(100, (int)((totalShifts / 20.0) * 100)) : 0;

            ViewBag.TotalShifts = totalShifts;
            ViewBag.TotalHours = (int)totalHours;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.PerformanceScore = performanceScore;
            ViewBag.Shifts = monthlyShifts;
            ViewBag.StaffName = user.NhanVien.HoTen;

            return View();
        }

        // ================================
        // 5. H? so c� nh�n
        // ================================
        public IActionResult Profile()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = _context.User
                .Include(u => u.NhanVien)
                .FirstOrDefault(u => u.Username == username);

            if (user?.NhanVien == null)
                return RedirectToAction("Login", "Account");

            ViewBag.User = user;
            return View(user.NhanVien);
        }

        // ================================
        // 6. C?p nh?t h? so
        // ================================
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(NhanVien model, IFormFile? avatarFile)
        {
            try
            {
                var nhanVien = _context.NhanVien.Find(model.MaNV);
                if (nhanVien == null)
                {
                    TempData["Error"] = "Kh�ng t�m th?y th�ng tin nh�n vi�n.";
                    return RedirectToAction("Profile");
                }

                var username = HttpContext.Session.GetString("Username");
                var user = _context.User.FirstOrDefault(u => u.Username == username);

                // X? l� upload ?nh
                if (avatarFile != null && avatarFile.Length > 0)
                {
                    // Ki?m tra d?nh d?ng file
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["Error"] = "Ch? ch?p nh?n file ?nh (.jpg, .jpeg, .png, .gif)";
                        return RedirectToAction("Profile");
                    }

                    // Ki?m tra k�ch thu?c file (t?i da 5MB)
                    if (avatarFile.Length > 5 * 1024 * 1024)
                    {
                        TempData["Error"] = "K�ch thu?c file kh�ng du?c vu?t qu� 5MB";
                        return RedirectToAction("Profile");
                    }

                    // T�n file theo format: {username}_{role}.{extension}
                    var fileName = $"{username}_Staff{extension}";
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
                    
                    // T?o thu m?c n?u chua t?n t?i
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // X�a ?nh cu n?u c� (v?i t�n file cu)
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
                                // Log l?i nhung v?n ti?p t?c upload ?nh m?i
                                Console.WriteLine($"Kh�ng th? x�a ?nh cu: {ex.Message}");
                            }
                        }
                    }

                    // Luu ?nh m?i
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(stream);
                    }

                    // Luu du?ng d?n tuong d?i v�o database
                    nhanVien.AnhDaiDien = $"avatars/{fileName}";
                }

                // C?p nh?t th�ng tin kh�c
                nhanVien.HoTen = model.HoTen;
                nhanVien.Email = model.Email;
                nhanVien.SDT = model.SDT;
                nhanVien.NgaySinh = model.NgaySinh;
                nhanVien.DiaChi = model.DiaChi;
                nhanVien.GioiTinh = model.GioiTinh;

                _context.SaveChanges();
                TempData["Success"] = "C?p nh?t th�ng tin th�nh c�ng!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"L?i: {ex.Message}";
            }

            return RedirectToAction("Profile");
        }

        // ================================
        // API: Lấy danh sách ca trực
        // ================================
        [HttpGet]
        public IActionResult GetCas()
        {
            var cas = _context.Ca
                .Where(c => c.TrangThai == "Hoạt động")
                .OrderBy(c => c.GioBatDau)
                .Select(c => new
                {
                    maCa = c.MaCa,
                    tenCa = c.TenCa,
                    gioBatDau = c.GioBatDau.ToString(@"hh\:mm"),
                    gioKetThuc = c.GioKetThuc.ToString(@"hh\:mm")
                })
                .ToList();

            return Json(cas);
        }
    }
}

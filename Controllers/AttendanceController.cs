using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDuLichRBAC_Upgrade.Models;
using QLDuLichRBAC_Upgrade.Models.Entities;
using QLDuLichRBAC_Upgrade.Utils;

namespace QLDuLichRBAC_Upgrade.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly QLJumaparenaContext _context;

        public AttendanceController(QLJumaparenaContext context)
        {
            _context = context;
        }

        // ==============================================
        // STAFF: Xem lịch trực và điểm danh
        // Redirect to StaffController for unified experience
        // ==============================================
        public IActionResult MySchedule()
        {
            // Redirect to new unified schedule view in StaffController
            return RedirectToAction("MySchedule", "Staff");
        }

        // API: Lấy lịch trực theo tuần
        [HttpGet]
        public IActionResult GetWeekSchedule(int maNV, DateTime startDate)
        {
            var endDate = startDate.AddDays(7);

            var schedules = _context.NhanVien_Ca
                .Include(nc => nc.Ca)
                .Include(nc => nc.NhanVien)
                .Where(nc => nc.MaNV == maNV && nc.NgayLamViec >= startDate && nc.NgayLamViec < endDate)
                .OrderBy(nc => nc.NgayLamViec)
                .ThenBy(nc => nc.Ca.GioBatDau)
                .Select(nc => new
                {
                    maNV = nc.MaNV,
                    maCa = nc.MaCa,
                    ngayLamViec = nc.NgayLamViec.ToString("yyyy-MM-dd"),
                    tenCa = nc.Ca.TenCa,
                    gioBatDau = nc.Ca.GioBatDau.ToString(@"hh\:mm"),
                    gioKetThuc = nc.Ca.GioKetThuc.ToString(@"hh\:mm"),
                    thoiGianCheckIn = nc.ThoiGianCheckIn.HasValue ? nc.ThoiGianCheckIn.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    thoiGianCheckOut = nc.ThoiGianCheckOut.HasValue ? nc.ThoiGianCheckOut.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    trangThaiDiemDanh = nc.TrangThaiDiemDanh,
                    diMuonCoPhep = nc.DiMuonCoPhep,
                    ghiChu = nc.GhiChu
                })
                .ToList();

            return Json(schedules);
        }

        // API: Điểm danh check-in
        [HttpPost]
        public IActionResult CheckIn(int maNV, int maCa, string ngayLamViec)
        {
            if (!DateTime.TryParse(ngayLamViec, out DateTime ngay))
                return Json(new { success = false, message = "Ngày không hợp lệ" });

            var schedule = _context.NhanVien_Ca
                .Include(nc => nc.Ca)
                .FirstOrDefault(nc => nc.MaNV == maNV && nc.MaCa == maCa && nc.NgayLamViec.Date == ngay.Date);

            if (schedule == null)
                return Json(new { success = false, message = "Không tìm thấy ca trực" });

            if (schedule.ThoiGianCheckIn != null)
                return Json(new { success = false, message = "Bạn đã điểm danh rồi" });

            var now = DateTime.Now;
            var shiftStart = ngay.Date.Add(schedule.Ca.GioBatDau.ToTimeSpan());
            var checkInWindow = shiftStart.AddMinutes(-30); // 30 phút trước ca

            // Kiểm tra thời gian check-in
            if (now < checkInWindow)
                return Json(new { success = false, message = $"Chưa tới giờ điểm danh. Vui lòng check-in sau {checkInWindow:HH:mm}" });

            schedule.ThoiGianCheckIn = now;

            // Xác định trạng thái
            if (now <= shiftStart)
            {
                schedule.TrangThaiDiemDanh = "Đang trực";
            }
            else
            {
                schedule.TrangThaiDiemDanh = "Đi muộn";
            }

            _context.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Điểm danh thành công",
                thoiGianCheckIn = now.ToString("yyyy-MM-dd HH:mm:ss"),
                trangThaiDiemDanh = schedule.TrangThaiDiemDanh
            });
        }

        // API: Điểm danh check-out
        [HttpPost]
        public IActionResult CheckOut(int maNV, int maCa, string ngayLamViec)
        {
            if (!DateTime.TryParse(ngayLamViec, out DateTime ngay))
                return Json(new { success = false, message = "Ngày không hợp lệ" });

            var schedule = _context.NhanVien_Ca
                .Include(nc => nc.Ca)
                .FirstOrDefault(nc => nc.MaNV == maNV && nc.MaCa == maCa && nc.NgayLamViec.Date == ngay.Date);

            if (schedule == null)
                return Json(new { success = false, message = "Không tìm thấy ca trực" });

            if (schedule.ThoiGianCheckIn == null)
                return Json(new { success = false, message = "Bạn chưa check-in" });

            if (schedule.ThoiGianCheckOut != null)
                return Json(new { success = false, message = "Bạn đã check-out rồi" });

            var now = DateTime.Now;
            var shiftEnd = ngay.Date.Add(schedule.Ca.GioKetThuc.ToTimeSpan());

            schedule.ThoiGianCheckOut = now;

            // Cập nhật trạng thái
            if (now < shiftEnd)
            {
                schedule.TrangThaiDiemDanh = "Về sớm";
            }
            else
            {
                schedule.TrangThaiDiemDanh = "Đã hoàn thành";
            }

            _context.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Check-out thành công",
                thoiGianCheckOut = now.ToString("yyyy-MM-dd HH:mm:ss"),
                trangThaiDiemDanh = schedule.TrangThaiDiemDanh
            });
        }

        // ==============================================
        // STAFF: Đăng ký lịch trực cho tuần sau
        // ==============================================
        [HttpPost]
        public IActionResult RegisterShift(int maNV, int maCa, string ngayTruc)
        {
            if (!DateTime.TryParse(ngayTruc, out DateTime ngay))
                return Json(new { success = false, message = "Ngày không hợp lệ" });

            // Kiểm tra đã đăng ký chưa
            var existing = _context.DangKyLichTruc
                .FirstOrDefault(d => d.MaNV == maNV && d.MaCa == maCa && d.NgayTruc.Date == ngay.Date);

            if (existing != null)
                return Json(new { success = false, message = "Bạn đã đăng ký ca này rồi" });

            // Kiểm tra số lượng người đăng ký (max 3)
            var registrationCount = _context.DangKyLichTruc
                .Count(d => d.MaCa == maCa && d.NgayTruc.Date == ngay.Date && d.TrangThai == "Chờ duyệt");

            if (registrationCount >= 3)
                return Json(new { success = false, message = "Ca này đã đủ 3 người đăng ký" });

            // Tạo đăng ký mới
            var dangKy = new DangKyLichTruc
            {
                MaNV = maNV,
                MaCa = maCa,
                NgayDangKy = DateTime.Now.Date,
                NgayTruc = ngay,
                ThoiGianDangKy = DateTime.Now,
                TrangThai = "Chờ duyệt"
            };

            _context.DangKyLichTruc.Add(dangKy);
            _context.SaveChanges();

            return Json(new { success = true, message = "Đăng ký thành công, chờ admin duyệt" });
        }

        // API: Lấy danh sách đăng ký của nhân viên
        [HttpGet]
        public IActionResult GetMyRegistrations(int maNV)
        {
            var registrations = _context.DangKyLichTruc
                .Include(d => d.Ca)
                .Where(d => d.MaNV == maNV)
                .OrderByDescending(d => d.ThoiGianDangKy)
                .Select(d => new
                {
                    maDangKy = d.MaDangKy,
                    maCa = d.MaCa,
                    tenCa = d.Ca.TenCa,
                    ngayDangKy = d.NgayDangKy.ToString("yyyy-MM-dd"),
                    ngayTruc = d.NgayTruc.ToString("yyyy-MM-dd"),
                    trangThai = d.TrangThai,
                    ghiChu = d.GhiChu
                })
                .ToList();

            return Json(registrations);
        }

        // ==============================================
        // ADMIN: Quản lý lịch trực và duyệt đăng ký
        // ==============================================
        public IActionResult ManageSchedule()
        {
            if (!AuthHelper.IsAuthenticated(HttpContext))
                return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
                return RedirectToAction("Index", "Home");

            return View();
        }

        // API: Lấy tất cả lịch trực theo tuần
        [HttpGet]
        public IActionResult GetAllSchedules(DateTime startDate)
        {
            var endDate = startDate.AddDays(7);

            var schedules = _context.NhanVien_Ca
                .Include(nc => nc.Ca)
                .Include(nc => nc.NhanVien)
                .Where(nc => nc.NgayLamViec >= startDate && nc.NgayLamViec < endDate)
                .OrderBy(nc => nc.NgayLamViec)
                .ThenBy(nc => nc.Ca.GioBatDau)
                .ThenBy(nc => nc.NhanVien.HoTen)
                .Select(nc => new
                {
                    maNV = nc.MaNV,
                    hoTen = nc.NhanVien.HoTen,
                    maCa = nc.MaCa,
                    tenCa = nc.Ca.TenCa,
                    ngayLamViec = nc.NgayLamViec.ToString("yyyy-MM-dd"),
                    gioBatDau = nc.Ca.GioBatDau.ToString(@"hh\:mm"),
                    gioKetThuc = nc.Ca.GioKetThuc.ToString(@"hh\:mm"),
                    thoiGianCheckIn = nc.ThoiGianCheckIn.HasValue ? nc.ThoiGianCheckIn.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    thoiGianCheckOut = nc.ThoiGianCheckOut.HasValue ? nc.ThoiGianCheckOut.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    trangThaiDiemDanh = nc.TrangThaiDiemDanh,
                    diMuonCoPhep = nc.DiMuonCoPhep,
                    ghiChu = nc.GhiChu
                })
                .ToList();

            return Json(schedules);
        }

        // API: Đánh dấu đi muộn có phép
        [HttpPost]
        public IActionResult ApproveLateArrival(int maNV, int maCa, string ngayLamViec)
        {
            if (!DateTime.TryParse(ngayLamViec, out DateTime ngay))
                return Json(new { success = false, message = "Ngày không hợp lệ" });

            var schedule = _context.NhanVien_Ca
                .FirstOrDefault(nc => nc.MaNV == maNV && nc.MaCa == maCa && nc.NgayLamViec.Date == ngay.Date);

            if (schedule == null)
                return Json(new { success = false, message = "Không tìm thấy ca trực" });

            schedule.DiMuonCoPhep = true;
            _context.SaveChanges();

            return Json(new { success = true, message = "Đã đánh dấu đi muộn có phép" });
        }

        // API: Lấy danh sách đăng ký chờ duyệt
        [HttpGet]
        public IActionResult GetPendingRegistrations()
        {
            var registrations = _context.DangKyLichTruc
                .Include(d => d.NhanVien)
                .Include(d => d.Ca)
                .Where(d => d.TrangThai == "Chờ duyệt")
                .OrderBy(d => d.NgayTruc)
                .ThenBy(d => d.ThoiGianDangKy)
                .Select(d => new
                {
                    maDangKy = d.MaDangKy,
                    maNV = d.MaNV,
                    hoTen = d.NhanVien.HoTen,
                    maCa = d.MaCa,
                    tenCa = d.Ca.TenCa,
                    gioBatDau = d.Ca.GioBatDau.ToString(@"hh\:mm"),
                    gioKetThuc = d.Ca.GioKetThuc.ToString(@"hh\:mm"),
                    ngayDangKy = d.NgayDangKy.ToString("yyyy-MM-dd"),
                    ngayTruc = d.NgayTruc.ToString("yyyy-MM-dd"),
                    thoiGianDangKy = d.ThoiGianDangKy.ToString("yyyy-MM-dd HH:mm:ss"),
                    ghiChu = d.GhiChu
                })
                .ToList();

            return Json(registrations);
        }

        // API: Duyệt/Từ chối đăng ký
        [HttpPost]
        public IActionResult ApproveRegistration(int maDangKy, string action, string ghiChu = "")
        {
            var registration = _context.DangKyLichTruc
                .FirstOrDefault(d => d.MaDangKy == maDangKy);

            if (registration == null)
                return Json(new { success = false, message = "Không tìm thấy đăng ký" });

            if (action == "approve")
            {
                // Kiểm tra xem đã có lịch trực chưa
                var existing = _context.NhanVien_Ca
                    .FirstOrDefault(nc => nc.MaNV == registration.MaNV && nc.MaCa == registration.MaCa && nc.NgayLamViec.Date == registration.NgayTruc.Date);

                if (existing != null)
                    return Json(new { success = false, message = "Nhân viên này đã có lịch trực vào ngày này" });

                // Tạo lịch trực
                var schedule = new NhanVien_Ca
                {
                    MaNV = registration.MaNV,
                    MaCa = registration.MaCa,
                    NgayLamViec = registration.NgayTruc,
                    TrangThaiDiemDanh = "Chưa điểm danh"
                };

                _context.NhanVien_Ca.Add(schedule);
                registration.TrangThai = "Đã duyệt";
                registration.GhiChu = ghiChu;
            }
            else if (action == "reject")
            {
                registration.TrangThai = "Từ chối";
                registration.GhiChu = ghiChu;
            }
            else
            {
                return Json(new { success = false, message = "Hành động không hợp lệ" });
            }

            _context.SaveChanges();

            return Json(new { success = true, message = action == "approve" ? "Đã duyệt đăng ký" : "Đã từ chối đăng ký" });
        }
    }
}

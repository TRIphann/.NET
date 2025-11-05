using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using QLDuLichRBAC_Upgrade.Models;
using QLDuLichRBAC_Upgrade.Models.Entities;
using QLDuLichRBAC_Upgrade.Models.ViewModels;
using QLDuLichRBAC_Upgrade.Services;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QLDuLichRBAC_Upgrade.Controllers
{
    public class CustomerController : Controller
    {
        private readonly QLDuLichContext _context;
        private readonly PaymentService _paymentService;

        public CustomerController(QLDuLichContext context, PaymentService paymentService)
        {
            _context = context;
            _paymentService = paymentService;
        }

        // Khi đăng nhập xong => chuyển đến trang đăng ký tour
        public IActionResult Index()
        {
            return RedirectToAction("RegisterTour");
        }

        // ================================
        // GET: Hiển thị danh sách tour
        // ================================
        public IActionResult RegisterTour()
        {
            try
            {
                var username = HttpContext.Session.GetString("Username");
                int? customerId = null;
                
                if (!string.IsNullOrEmpty(username))
                {
                    var user = _context.Users.FirstOrDefault(u => u.Username == username);
                    if (user != null)
                    {
                        var khachHang = _context.KhachHang.FirstOrDefault(k => k.UserId == user.UserId);
                        if (khachHang != null)
                        {
                            customerId = khachHang.MaKH;
                        }
                    }
                }

                var today = DateTime.Today;
                var tours = _context.Tour
                    .Include(t => t.Tour_Anh)
                    .Include(t => t.Tour_PhuongTien).ThenInclude(tp => tp.PhuongTien)
                    .Include(t => t.Tour_DiaDiem).ThenInclude(td => td.DiaDiem)
                    .Include(t => t.Tour_LichTrinhCT).ThenInclude(lt => lt.LichTrinhCT)
                    .Include(t => t.NhanVien_Tour).ThenInclude(nt => nt.NhanVien)
                    .Where(t => t.NgayBatDau >= today) // Lọc tour có ngày bắt đầu từ hôm nay trở đi
                    .ToList();

                // Nếu có khách hàng đăng nhập, lọc bỏ các tour đã đăng ký
                if (customerId.HasValue)
                {
                    var registeredTourIds = _context.HoaDon
                        .Where(hd => hd.MaKH == customerId.Value)
                        .Select(hd => hd.MaTour)
                        .ToList();
                    
                    tours = tours.Where(t => !registeredTourIds.Contains(t.MaTour)).ToList();
                }

                return View(tours); // ✅ Quan trọng — truyền Model vào view
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi tải danh sách tour: " + ex.Message;
                return View(new List<Tour>()); // tránh null model
            }
        }

        // ================================
        // POST: Đăng ký tour (GIỮ NGUYÊN)
        // ================================
        [HttpPost]
        public IActionResult RegisterTour(int maKH, int maTour, int soLuongVe, string danhSachDV)
        {
            try
            {
                var p1 = new SqlParameter("@MaKH", maKH);
                var p2 = new SqlParameter("@MaTour", maTour);
                var p3 = new SqlParameter("@SoLuongVe", soLuongVe);
                var p4 = new SqlParameter("@DanhSachDV", danhSachDV ?? (object)DBNull.Value);

                _context.Database.ExecuteSqlRaw(
                    "EXEC sp_DangKyTour @MaKH, @MaTour, @SoLuongVe, @DanhSachDV",
                    p1, p2, p3, p4
                );

                TempData["Success"] = "Đăng ký tour thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
            }

            // Lấy lại danh sách tour đầy đủ để hiển thị
            var tours = _context.Tour
                .Include(t => t.Tour_Anh)
                .Include(t => t.Tour_PhuongTien).ThenInclude(tp => tp.PhuongTien)
                .Include(t => t.Tour_DiaDiem).ThenInclude(td => td.DiaDiem)
                .Include(t => t.Tour_LichTrinhCT).ThenInclude(lt => lt.LichTrinhCT)
                .Include(t => t.NhanVien_Tour)   // Chú ý có NhanVien_Tour
                    .ThenInclude(nt => nt.NhanVien)
                .ToList();


            return View(tours);
        }

        // ================================
        // GET: Xem chi tiết tour
        // ================================
        public IActionResult Details(int id)
        {
            var tour = _context.Tour
                .Include(t => t.Tour_Anh)
                .Include(t => t.Tour_PhuongTien).ThenInclude(tp => tp.PhuongTien)
                .Include(t => t.Tour_DiaDiem).ThenInclude(td => td.DiaDiem)
                .Include(t => t.Tour_LichTrinhCT)
                .Include(t => t.NhanVien_Tour).ThenInclude(nv => nv.NhanVien)
                .FirstOrDefault(t => t.MaTour == id);

            if (tour == null)
            {
                TempData["Error"] = "Không tìm thấy tour.";
                return RedirectToAction("RegisterTour");
            }

            // Lấy danh sách dịch vụ đi kèm
            ViewBag.DichVuList = _context.DichVuDiKem.ToList();

            return View(tour);
        }

        // ================================
        // GET: API lấy chi tiết tour
        // ================================
        [HttpGet]
        public IActionResult GetTourDetails(int id)
        {
            try
            {
                var tour = _context.Tour
                    .Include(t => t.Tour_Anh)
                    .Include(t => t.Tour_PhuongTien).ThenInclude(tp => tp.PhuongTien)
                    .Include(t => t.Tour_DiaDiem).ThenInclude(td => td.DiaDiem)
                    .Include(t => t.Tour_LichTrinhCT).ThenInclude(lt => lt.LichTrinhCT)
                    .Include(t => t.NhanVien_Tour).ThenInclude(nt => nt.NhanVien)
                    .FirstOrDefault(t => t.MaTour == id);

                if (tour == null)
                {
                    return NotFound(new { error = "Không tìm thấy tour" });
                }

                var guide = tour.NhanVien_Tour?.FirstOrDefault()?.NhanVien;
                var soNgay = (tour.NgayKetThuc - tour.NgayBatDau).Days + 1;
                var soDem = soNgay - 1;

                var result = new
                {
                    maTour = tour.MaTour,
                    tenTour = tour.TenTour,
                    ngayBatDau = tour.NgayBatDau,
                    ngayKetThuc = tour.NgayKetThuc,
                    gia = tour.Gia,
                    soNgay = soNgay,
                    soDem = soDem,
                    huongDanVien = guide != null ? new
                    {
                        tenNV = guide.TenNV,
                        sdt = guide.SDT,
                        gioiTinh = guide.GioiTinh,
                        anhDaiDien = guide.AnhDaiDien
                    } : null,
                    diaDiems = tour.Tour_DiaDiem?.Select(td => new
                    {
                        tenDD = td.DiaDiem?.TenDD,
                        tinh = td.DiaDiem?.Tinh
                    }).ToList(),
                    lichTrinhs = tour.Tour_LichTrinhCT?.Select(tlt => new
                    {
                        ngayThu = tlt.LichTrinhCT?.NgayThu,
                        noiDung = tlt.LichTrinhCT?.NoiDung
                    }).OrderBy(x => x.ngayThu).ToList(),
                    phuongTiens = tour.Tour_PhuongTien?.Select(tp => new
                    {
                        tenPT = tp.PhuongTien?.TenPT,
                        loaiPT = tp.PhuongTien?.LoaiPT,
                        soCho = tp.PhuongTien?.SoCho
                    }).ToList(),
                    anhAlbum = tour.Tour_Anh?.Select(a => new
                    {
                        duongDan = a.DuongDan
                    }).ToList()
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ================================
        // GET: API lấy danh sách dịch vụ
        // ================================
        [HttpGet]
        public IActionResult GetServices()
        {
            try
            {
                var services = _context.DichVuDiKem
                    .Select(dv => new
                    {
                        maDV = dv.MaDV,
                        tenDV = dv.TenDV,
                        moTa = dv.MoTa,
                        donGia = dv.DonGia
                    })
                    .ToList();

                return Json(services);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ================================
        // GET: Xem các tour đã thanh toán
        // ================================
        public IActionResult PaidTours()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var khachHang = _context.KhachHang.FirstOrDefault(k => k.UserId == user.UserId);
            if (khachHang == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("RegisterTour");
            }

            var paidTours = _context.HoaDon
                .Where(hd => hd.MaKH == khachHang.MaKH)
                .Include(hd => hd.Tour)
                    .ThenInclude(t => t.Tour_Anh)
                .Include(hd => hd.Tour)
                    .ThenInclude(t => t.Tour_DiaDiem)
                    .ThenInclude(td => td.DiaDiem)
                .Include(hd => hd.Tour)
                    .ThenInclude(t => t.NhanVien_Tour)
                    .ThenInclude(nt => nt.NhanVien)
                .OrderByDescending(hd => hd.NgayTT)
                .ToList();

            return View(paidTours);
        }

        // ================================
        // GET: Hồ sơ cá nhân
        // ================================
        public IActionResult Profile()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var khachHang = _context.KhachHang.FirstOrDefault(k => k.UserId == user.UserId);
            if (khachHang == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("RegisterTour");
            }

            ViewBag.User = user;
            return View(khachHang);
        }

        // ================================
        // POST: Cập nhật hồ sơ
        // ================================
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(KhachHang model, IFormFile? avatarFile)
        {
            try
            {
                var khachHang = _context.KhachHang.Find(model.MaKH);
                if (khachHang == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
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

                    if (avatarFile.Length > 5 * 1024 * 1024)
                    {
                        TempData["Error"] = "Kích thước file không được vượt quá 5MB";
                        return RedirectToAction("Profile");
                    }

                    // Tên file theo format: {username}_{role}.{extension}
                    var fileName = $"{username}_Customer{extension}";
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
                    
                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Xóa ảnh cũ nếu có (với tên file cũ)
                    if (!string.IsNullOrEmpty(khachHang.AnhDaiDien))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", khachHang.AnhDaiDien);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Lưu file mới
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(stream);
                    }

                    // Lưu đường dẫn tương đối vào database
                    khachHang.AnhDaiDien = $"avatars/{fileName}";
                }

                // Cập nhật thông tin khác
                khachHang.TenKH = model.TenKH;
                khachHang.SDT = model.SDT;
                khachHang.DiaChi = model.DiaChi;
                khachHang.GioiTinh = model.GioiTinh;

                _context.SaveChanges();
                TempData["Success"] = "Cập nhật thông tin thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToAction("Profile");
        }

        // ================================
        // GET: Trang thanh toán với QR
        // ================================
        [HttpGet]
        public IActionResult Payment(int tourId, int soLuongVe = 1, string? dichVuIds = null)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var khachHang = _context.KhachHang.FirstOrDefault(k => k.UserId == user.UserId);
            if (khachHang == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("RegisterTour");
            }

            var tour = _context.Tour.Find(tourId);
            if (tour == null)
            {
                TempData["Error"] = "Không tìm thấy tour.";
                return RedirectToAction("RegisterTour");
            }

            // Tính tổng tiền
            decimal tongTien = tour.Gia * soLuongVe;
            var dichVuList = new List<DichVuViewModel>();

            if (!string.IsNullOrEmpty(dichVuIds))
            {
                var dvIds = dichVuIds.Split(',').Select(int.Parse).ToList();
                var dichVus = _context.DichVuDiKem.Where(dv => dvIds.Contains(dv.MaDV)).ToList();
                
                foreach (var dv in dichVus)
                {
                    dichVuList.Add(new DichVuViewModel
                    {
                        MaDV = dv.MaDV,
                        TenDV = dv.TenDV,
                        DonGia = dv.DonGia
                    });
                    tongTien += dv.DonGia;
                }
            }

            // Tạo mã giao dịch và QR code
            var paymentInfo = _paymentService.CreatePaymentInfo(
                khachHang.MaKH,
                tourId,
                tongTien,
                tour.TenTour
            );

            var viewModel = new PaymentViewModel
            {
                MaTour = tourId,
                TenTour = tour.TenTour,
                SoLuongVe = soLuongVe,
                GiaTour = tour.Gia,
                DichVuDaChon = dichVuList,
                TongTien = tongTien,
                QRCodeBase64 = paymentInfo.QRCodeBase64,
                MaGiaoDich = paymentInfo.TransactionCode
            };

            // Lưu thông tin thanh toán vào session để kiểm tra sau
            HttpContext.Session.SetString("PendingPayment_" + khachHang.MaKH, 
                System.Text.Json.JsonSerializer.Serialize(paymentInfo));

            return View(viewModel);
        }

        // ================================
        // POST: Kiểm tra trạng thái thanh toán
        // ================================
        [HttpPost]
        public async Task<IActionResult> CheckPaymentStatus(string maGiaoDich, int tongTien)
        {
            try
            {
                bool isSuccess = await _paymentService.CheckTransactionHistory(maGiaoDich, tongTien);

                return Json(new PaymentCheckResponse
                {
                    IsSuccess = isSuccess,
                    Message = isSuccess ? "Thanh toán thành công!" : "Chưa nhận được thanh toán",
                    TransactionId = maGiaoDich
                });
            }
            catch (Exception ex)
            {
                return Json(new PaymentCheckResponse
                {
                    IsSuccess = false,
                    Message = $"Lỗi kiểm tra thanh toán: {ex.Message}"
                });
            }
        }

        // ================================
        // POST: Xác nhận thanh toán thành công
        // ================================
        [HttpPost]
        public IActionResult ConfirmPayment(int tourId, int soLuongVe, string dichVuIds)
        {
            try
            {
                var username = HttpContext.Session.GetString("Username");
                if (string.IsNullOrEmpty(username))
                    return RedirectToAction("Login", "Account");

                var user = _context.Users.FirstOrDefault(u => u.Username == username);
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var khachHang = _context.KhachHang.FirstOrDefault(k => k.UserId == user.UserId);
                if (khachHang == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                    return RedirectToAction("RegisterTour");
                }

                // Gọi stored procedure để đăng ký tour
                var p1 = new SqlParameter("@MaKH", khachHang.MaKH);
                var p2 = new SqlParameter("@MaTour", tourId);
                var p3 = new SqlParameter("@SoLuongVe", soLuongVe);
                var p4 = new SqlParameter("@DanhSachDV", dichVuIds ?? (object)DBNull.Value);

                _context.Database.ExecuteSqlRaw(
                    "EXEC sp_DangKyTour @MaKH, @MaTour, @SoLuongVe, @DanhSachDV",
                    p1, p2, p3, p4
                );

                TempData["Success"] = "Thanh toán và đăng ký tour thành công!";
                return RedirectToAction("PaidTours");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi xác nhận thanh toán: {ex.Message}";
                return RedirectToAction("RegisterTour");
            }
        }
    }
}

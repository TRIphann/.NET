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
using QRCoder;

namespace QLDuLichRBAC_Upgrade.Controllers
{
    public class CustomerController : Controller
    {
        private readonly QLJumaparenaContext _context;
        private readonly PaymentService _paymentService;

        public CustomerController(QLJumaparenaContext context, PaymentService paymentService)
        {
            _context = context;
            _paymentService = paymentService;
        }

        // ================================
        // GET: Trang chủ Customer - Redirect to BookTickets
        // ================================
        public IActionResult Index()
        {
            return RedirectToAction("BookTickets");
        }

        // ================================
        // GET: Đặt vé - Hiển thị danh sách gói dịch vụ
        // ================================
        public IActionResult BookTickets()
        {
            try
            {
                var packages = _context.GoiDichVu
                    .Where(g => g.TrangThai == "Đang hoạt động")
                    .OrderBy(g => g.Gia)
                    .ToList();

                return View(packages);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải danh sách gói dịch vụ: {ex.Message}";
                return View(new List<GoiDichVu>());
            }
        }

        // ================================
        // GET: API lấy danh sách ca
        // ================================
        [HttpGet]
        public IActionResult GetTimeSlots()
        {
            try
            {
                var timeSlots = _context.Ca
                    .Where(c => c.TrangThai == "Đang hoạt động")
                    .Select(c => new
                    {
                        maCa = c.MaCa,
                        tenCa = c.TenCa,
                        gioBatDau = c.GioBatDau.ToString(@"hh\:mm"),
                        gioKetThuc = c.GioKetThuc.ToString(@"hh\:mm")
                    })
                    .ToList();

                return Json(timeSlots);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ================================
        // GET: API lấy danh sách dịch vụ thêm
        // ================================
        [HttpGet]
        public IActionResult GetExtraServices()
        {
            try
            {
                var services = _context.DichVuThem
                    .Where(dv => dv.TrangThai == "Đang hoạt động")
                    .Select(dv => new
                    {
                        maDVThem = dv.MaDVThem,
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
        // GET: Vé của tôi
        // ================================
        public IActionResult MyTickets()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = _context.User.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var khachHang = _context.KhachHang.FirstOrDefault(k => k.UserId == user.UserId);
            if (khachHang == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("BookTickets");
            }

            var tickets = _context.Ve
                .Where(v => v.MaKH == khachHang.MaKH)
                .Include(v => v.GoiDichVu)
                .Include(v => v.Ca)
                .OrderByDescending(v => v.NgayDat)
                .Select(v => new MyTicketViewModel
                {
                    MaVe = v.MaVe,
                    MaVeCode = v.MaVeCode,
                    TenGoi = v.GoiDichVu.TenGoi,
                    TenCa = v.Ca.TenCa,
                    NgayDat = v.NgayDat,
                    NgaySuDung = v.NgaySuDung,
                    SoNguoi = v.SoNguoi,
                    TongTien = v.TongTien,
                    TrangThai = v.TrangThai,
                    NgayCheckIn = v.NgayCheckIn,
                    QRCodeBase64 = GenerateQRCode(v.MaVeCode),
                    DichVuThem = _context.Ve_DichVuThem
                        .Where(tg => tg.MaVe == v.MaVe)
                        .Include(tg => tg.DichVuThem)
                        .Select(tg => new DichVuThemViewModel
                        {
                            MaDVThem = tg.MaDVThem,
                            TenDV = tg.DichVuThem.TenDV,
                            DonGia = tg.DichVuThem.DonGia,
                            SoLuong = 1
                        })
                        .ToList()
                })
                .ToList();

            return View(tickets);
        }

        // ================================
        // GET: H? so c� nh�n
        // ================================
        public IActionResult Profile()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = _context.User.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var khachHang = _context.KhachHang.FirstOrDefault(k => k.UserId == user.UserId);
            if (khachHang == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("BookTickets");
            }

            ViewBag.User = user;
            return View(khachHang);
        }

        // ================================
        // POST: C?p nh?t h? so
        // ================================
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(KhachHang model, IFormFile? avatarFile)
        {
            try
            {
                var khachHang = _context.KhachHang.Find(model.MaKH);
                if (khachHang == null)
                {
                    TempData["Error"] = "Kh�ng t�m th?y th�ng tin kh�ch h�ng.";
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

                    if (avatarFile.Length > 5 * 1024 * 1024)
                    {
                        TempData["Error"] = "K�ch thu?c file kh�ng du?c vu?t qu� 5MB";
                        return RedirectToAction("Profile");
                    }

                    // T�n file theo format: {username}_{role}.{extension}
                    var fileName = $"{username}_Customer{extension}";
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
                    
                    // T?o thu m?c n?u chua t?n t?i
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // X�a ?nh cu n?u c� (v?i t�n file cu)
                    if (!string.IsNullOrEmpty(khachHang.AnhDaiDien))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", khachHang.AnhDaiDien);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Luu file m?i
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(stream);
                    }

                    // Luu du?ng d?n tuong d?i v�o database
                    khachHang.AnhDaiDien = $"avatars/{fileName}";
                }

                // C?p nh?t th�ng tin kh�c
                khachHang.TenKH = model.TenKH;
                khachHang.SDT = model.SDT;
                khachHang.DiaChi = model.DiaChi;
                khachHang.GioiTinh = model.GioiTinh;

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
        // GET: Trang thanh toán Jumparena
        // ================================
        [HttpGet]
        public IActionResult PaymentJumparena(int packageId, int timeSlotId, string bookingDate, int numberOfPeople, string? extraServices)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = _context.User.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var khachHang = _context.KhachHang.FirstOrDefault(k => k.UserId == user.UserId);
            if (khachHang == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("BookTickets");
            }

            var package = _context.GoiDichVu.Find(packageId);
            if (package == null)
            {
                TempData["Error"] = "Không tìm thấy gói dịch vụ.";
                return RedirectToAction("BookTickets");
            }

            var timeSlot = _context.Ca.Find(timeSlotId);
            if (timeSlot == null)
            {
                TempData["Error"] = "Không tìm thấy ca.";
                return RedirectToAction("BookTickets");
            }

            // Tính tổng tiền
            decimal tongTien = package.Gia * numberOfPeople;
            var dichVuList = new List<DichVuThemViewModel>();

            if (!string.IsNullOrEmpty(extraServices))
            {
                var dvIds = extraServices.Split(',').Select(int.Parse).ToList();
                var dichVus = _context.DichVuThem.Where(dv => dvIds.Contains(dv.MaDVThem)).ToList();
                
                foreach (var dv in dichVus)
                {
                    dichVuList.Add(new DichVuThemViewModel
                    {
                        MaDVThem = dv.MaDVThem,
                        TenDV = dv.TenDV,
                        DonGia = dv.DonGia
                    });
                    tongTien += dv.DonGia;
                }
            }

            // Tạo mã vé và mã giao dịch
            var maVeCode = $"JMP{DateTime.Now:yyyyMMddHHmmss}{khachHang.MaKH}";
            var maGiaoDich = $"GD{DateTime.Now:yyyyMMddHHmmss}";

            // Tạo QR code cho thanh toán
            var paymentInfo = _paymentService.CreatePaymentInfo(
                khachHang.MaKH,
                packageId,
                tongTien,
                package.TenGoi
            );

            var viewModel = new JumparenaPaymentViewModel
            {
                MaGoi = packageId,
                TenGoi = package.TenGoi,
                MaCa = timeSlotId,
                TenCa = timeSlot.TenCa,
                NgayDat = DateTime.Now,
                NgaySuDung = DateTime.Parse(bookingDate),
                SoNguoi = numberOfPeople,
                GiaGoi = package.Gia,
                DichVuDaChon = dichVuList,
                TongTien = tongTien,
                QRCodeBase64 = paymentInfo.QRCodeBase64,
                MaGiaoDich = paymentInfo.TransactionCode,
                MaVeCode = maVeCode
            };

            // Lưu thông tin booking vào session
            HttpContext.Session.SetString("PendingBooking_" + khachHang.MaKH, 
                System.Text.Json.JsonSerializer.Serialize(viewModel));

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
        // POST: Xác nhận đặt vé sau thanh toán
        // ================================
        [HttpPost]
        public IActionResult ConfirmBooking(int packageId, int timeSlotId, string bookingDate, int numberOfPeople, string? extraServices)
        {
            try
            {
                var username = HttpContext.Session.GetString("Username");
                if (string.IsNullOrEmpty(username))
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });

                var user = _context.User.FirstOrDefault(u => u.Username == username);
                if (user == null)
                    return Json(new { success = false, message = "Không tìm thấy tài khoản" });

                var khachHang = _context.KhachHang.FirstOrDefault(k => k.UserId == user.UserId);
                if (khachHang == null)
                    return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng" });

                var package = _context.GoiDichVu.Find(packageId);
                if (package == null)
                    return Json(new { success = false, message = "Không tìm thấy gói dịch vụ" });

                // Tạo mã vé
                var maVeCode = $"JMP{DateTime.Now:yyyyMMddHHmmss}{khachHang.MaKH}";

                // Tính tổng tiền
                decimal tongTien = package.Gia * numberOfPeople;

                // Tạo vé
                var ve = new Ve
                {
                    MaVeCode = maVeCode,
                    MaKH = khachHang.MaKH,
                    MaGoi = packageId,
                    MaCa = timeSlotId,
                    NgayDat = DateTime.Now,
                    NgaySuDung = DateTime.Parse(bookingDate),
                    SoNguoi = numberOfPeople,
                    TongTien = tongTien,
                    TrangThai = "Đã thanh toán"
                };

                _context.Ve.Add(ve);
                _context.SaveChanges();

                // Thêm dịch vụ thêm nếu có
                if (!string.IsNullOrEmpty(extraServices))
                {
                    var dvIds = extraServices.Split(',').Select(int.Parse).ToList();
                    foreach (var dvId in dvIds)
                    {
                        var dichVu = _context.DichVuThem.Find(dvId);
                        if (dichVu != null)
                        {
                            _context.Ve_DichVuThem.Add(new Ve_DichVuThem
                            {
                                MaVe = ve.MaVe,
                                MaDVThem = dvId
                            });
                            ve.TongTien += dichVu.DonGia;
                        }
                    }
                }

                // Tạo hóa đơn
                var hoaDon = new HoaDon
                {
                    MaKH = ve.MaKH,
                    NgayTT = DateTime.Now,
                    TongTien = ve.TongTien,
                    HinhThucTT = "Chuyển khoản",
                    TrangThai = "Đã thanh toán"
                };

                _context.HoaDon.Add(hoaDon);
                ve.MaHD = hoaDon.MaHD;
                
                _context.SaveChanges();

                return Json(new { 
                    success = true, 
                    message = "Đặt vé thành công!",
                    ticketCode = maVeCode
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"Lỗi xác nhận đặt vé: {ex.Message}" 
                });
            }
        }

        // ================================
        // Helper: Tạo QR Code từ mã vé
        // ================================
        private string GenerateQRCode(string ticketCode)
        {
            try
            {
                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(ticketCode, QRCodeGenerator.ECCLevel.Q);
                    using (var qrCode = new PngByteQRCode(qrCodeData))
                    {
                        byte[] qrCodeBytes = qrCode.GetGraphic(10);
                        return Convert.ToBase64String(qrCodeBytes);
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}

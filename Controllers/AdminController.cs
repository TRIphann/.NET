using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDuLichRBAC_Upgrade.Models;
using QLDuLichRBAC_Upgrade.Models.Entities;

namespace QLDuLichRBAC_Upgrade.Controllers
{
    public class AdminController : Controller
    {
        private readonly QLDuLichContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminController(QLDuLichContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ================================
        // Kiểm tra Admin
        // ================================
        private bool IsAdmin()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username)) return false;

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            return user?.Role == "Admin";
        }

        // ================================
        // Dashboard - Redirect đến Statistics
        // ================================
        public IActionResult Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return RedirectToAction("Statistics", new { tab = "tours" });
        }

        // ================================
        // 1. STATISTICS - Thống kê
        // ================================
        public IActionResult Statistics(string tab = "tours")
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.CurrentTab = tab;

            // Thống kê Tour
            var topToursData = _context.Tour
                .Select(t => new
                {
                    t.MaTour,
                    t.TenTour,
                    t.Gia,
                    SoLuotDangKy = _context.KhachHang_Tour.Count(k => k.MaTour == t.MaTour),
                    DoanhThu = _context.HoaDon
                        .Where(h => h.MaTour == t.MaTour)
                        .Sum(h => (decimal?)h.TongTien) ?? 0
                })
                .OrderByDescending(x => x.SoLuotDangKy)
                .Take(10)
                .ToList();

            var toursByMonthRaw = _context.Tour
                .GroupBy(t => new { t.NgayBatDau.Year, t.NgayBatDau.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    SoTour = g.Count()
                })
                .ToList() // Execute query first
                .Select(x => new
                {
                    Thang = $"{x.Month}/{x.Year}", // Format after execution
                    x.SoTour
                })
                .OrderBy(x => x.Thang)
                .ToList();

            var toursByPriceData = _context.Tour
                .GroupBy(t => t.Gia < 4000000 ? "< 4tr" : 
                              t.Gia < 6000000 ? "4-6tr" : "> 6tr")
                .Select(g => new
                {
                    Range = g.Key,
                    Count = g.Count()
                })
                .ToList();

            var tourStats = new
            {
                TotalTours = _context.Tour.Count(),
                ActiveTours = _context.Tour.Count(t => t.NgayBatDau >= DateTime.Today),
                CompletedTours = _context.Tour.Count(t => t.NgayKetThuc < DateTime.Today),
                TopTours = topToursData,
                ToursByMonth = toursByMonthRaw,
                ToursByPrice = toursByPriceData
            };

            // Thống kê Nhân viên
            var topEmployeesData = _context.NhanVien
                .Select(nv => new
                {
                    nv.MaNV,
                    nv.TenNV,
                    nv.GioiTinh,
                    nv.SDT,
                    SoTour = _context.NhanVien_Tour.Count(nt => nt.MaNV == nv.MaNV),
                    TongLuong = _context.NhanVien_Tour
                        .Where(nt => nt.MaNV == nv.MaNV)
                        .Join(_context.Tour, nt => nt.MaTour, t => t.MaTour, (nt, t) => t.Gia * 0.5m)
                        .Sum()
                })
                .OrderByDescending(x => x.SoTour)
                .Take(10)
                .ToList();

            var employeesByGenderData = _context.NhanVien
                .GroupBy(nv => nv.GioiTinh)
                .Select(g => new
                {
                    GioiTinh = g.Key,
                    Count = g.Count()
                })
                .ToList();

            var employeeWorkloadData = _context.NhanVien
                .Select(nv => new
                {
                    nv.TenNV,
                    SoTour = _context.NhanVien_Tour.Count(nt => nt.MaNV == nv.MaNV)
                })
                .OrderByDescending(x => x.SoTour)
                .Take(10)
                .ToList();

            var employeeStats = new
            {
                TotalEmployees = _context.NhanVien.Count(),
                ActiveEmployees = _context.NhanVien
                    .Count(nv => _context.NhanVien_Tour.Any(nt => nt.MaNV == nv.MaNV)),
                TopEmployees = topEmployeesData,
                EmployeesByGender = employeesByGenderData,
                EmployeeWorkload = employeeWorkloadData
            };

            // Thống kê Khách hàng
            var topCustomersData = _context.KhachHang
                .Select(kh => new
                {
                    kh.MaKH,
                    kh.TenKH,
                    kh.SDT,
                    kh.GioiTinh,
                    SoTourDaDangKy = _context.HoaDon.Count(h => h.MaKH == kh.MaKH),
                    TongChiTieu = _context.HoaDon
                        .Where(h => h.MaKH == kh.MaKH)
                        .Sum(h => (decimal?)h.TongTien) ?? 0
                })
                .OrderByDescending(x => x.TongChiTieu)
                .Take(10)
                .ToList();

            var customersByGenderData = _context.KhachHang
                .GroupBy(kh => kh.GioiTinh)
                .Select(g => new
                {
                    GioiTinh = g.Key,
                    Count = g.Count()
                })
                .ToList();

            var newCustomersByMonthRaw = _context.KhachHang
                .GroupBy(kh => new { Year = 2026, Month = 1 })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .ToList() // Execute first
                .Select(x => new
                {
                    Thang = $"{x.Month}/{x.Year}", // Format after
                    x.Count
                })
                .ToList();

            var customerStats = new
            {
                TotalCustomers = _context.KhachHang.Count(),
                ActiveCustomers = _context.HoaDon
                    .Select(h => h.MaKH)
                    .Distinct()
                    .Count(),
                TopCustomers = topCustomersData,
                CustomersByGender = customersByGenderData,
                NewCustomersByMonth = newCustomersByMonthRaw
            };

            // Thống kê Doanh thu
            var revenueStatsRaw = _context.HoaDon
                .GroupBy(h => new { h.NgayTT.Year, h.NgayTT.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    DoanhThu = g.Sum(x => x.TongTien),
                    SoHoaDon = g.Count()
                })
                .ToList() // Execute first
                .Select(x => new
                {
                    Thang = $"{x.Month}/{x.Year}", // Format after
                    x.DoanhThu,
                    x.SoHoaDon
                })
                .OrderBy(x => x.Thang)
                .ToList();

            ViewBag.TourStats = tourStats;
            ViewBag.EmployeeStats = employeeStats;
            ViewBag.CustomerStats = customerStats;
            ViewBag.RevenueStats = revenueStatsRaw;

            return View();
        }

        // ================================
        // 2. ACCOUNTS - Quản lý tài khoản
        // ================================
        public IActionResult Accounts(string tab = "employees", int page = 1, int pageSize = 50)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.CurrentTab = tab;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            if (tab == "employees")
            {
                // Đếm tổng số
                var totalEmployees = _context.NhanVien.Count();
                ViewBag.TotalPages = (int)Math.Ceiling(totalEmployees / (double)pageSize);

                // TỐI ƯU: Tính SoTour bằng GroupJoin thay vì subquery
                var employeeIds = _context.NhanVien
                    .OrderBy(nv => nv.MaNV)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(nv => nv.MaNV)
                    .ToList();

                var tourCounts = _context.NhanVien_Tour
                    .Where(nt => employeeIds.Contains(nt.MaNV))
                    .GroupBy(nt => nt.MaNV)
                    .Select(g => new { MaNV = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.MaNV, x => x.Count);

                var employees = _context.NhanVien
                    .Include(nv => nv.User)
                    .Where(nv => employeeIds.Contains(nv.MaNV))
                    .OrderBy(nv => nv.MaNV)
                    .AsEnumerable() // Switch to client-side evaluation
                    .Select(nv => new
                    {
                        nv.MaNV,
                        nv.TenNV,
                        nv.GioiTinh,
                        nv.SDT,
                        nv.DiaChi,
                        nv.VaiTro,
                        nv.AnhDaiDien,
                        Username = nv.User != null ? nv.User.Username : null,
                        UserId = nv.UserId,
                        SoTour = tourCounts.ContainsKey(nv.MaNV) ? tourCounts[nv.MaNV] : 0
                    })
                    .ToList();

                ViewBag.Employees = employees;
                ViewBag.Customers = new List<object>();
            }
            else
            {
                // Đếm tổng số
                var totalCustomers = _context.KhachHang.Count();
                ViewBag.TotalPages = (int)Math.Ceiling(totalCustomers / (double)pageSize);

                // TỐI ƯU: Tính SoTour bằng GroupJoin thay vì subquery
                var customerIds = _context.KhachHang
                    .OrderBy(kh => kh.MaKH)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(kh => kh.MaKH)
                    .ToList();

                var tourCounts = _context.HoaDon
                    .Where(h => customerIds.Contains(h.MaKH))
                    .GroupBy(h => h.MaKH)
                    .Select(g => new { MaKH = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.MaKH, x => x.Count);

                var customers = _context.KhachHang
                    .Include(kh => kh.User)
                    .Where(kh => customerIds.Contains(kh.MaKH))
                    .OrderBy(kh => kh.MaKH)
                    .AsEnumerable() // Switch to client-side evaluation
                    .Select(kh => new
                    {
                        kh.MaKH,
                        kh.TenKH,
                        kh.GioiTinh,
                        kh.SDT,
                        kh.DiaChi,
                        kh.AnhDaiDien,
                        Username = kh.User != null ? kh.User.Username : null,
                        UserId = kh.UserId,
                        SoTour = tourCounts.ContainsKey(kh.MaKH) ? tourCounts[kh.MaKH] : 0
                    })
                    .ToList();

                ViewBag.Employees = new List<object>();
                ViewBag.Customers = customers;
            }

            return View();
        }

        // ================================
        // Tạo tài khoản nhân viên
        // ================================
        [HttpPost]
        public IActionResult CreateEmployeeAccount(string username, string password, string tenNV, 
            string gioiTinh, string sdt, string diaChi)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Không có quyền" });

            try
            {
                // Kiểm tra username đã tồn tại
                if (_context.Users.Any(u => u.Username == username))
                {
                    return Json(new { success = false, message = "Username đã tồn tại" });
                }

                // Tạo User
                var user = new User
                {
                    Username = username,
                    PasswordHash = password, // Trong thực tế nên hash password
                    Role = "Guide"
                };
                _context.Users.Add(user);
                _context.SaveChanges();

                // Tạo NhanVien
                var maNV = _context.NhanVien.Any() 
                    ? _context.NhanVien.Max(nv => nv.MaNV) + 1 
                    : 1;

                var nhanVien = new NhanVien
                {
                    MaNV = maNV,
                    TenNV = tenNV,
                    GioiTinh = gioiTinh,
                    SDT = sdt,
                    DiaChi = diaChi,
                    VaiTro = "Hướng dẫn viên",
                    UserId = user.UserId
                };
                _context.NhanVien.Add(nhanVien);
                _context.SaveChanges();

                return Json(new { success = true, message = "Tạo tài khoản thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // ================================
        // Xóa tài khoản
        // ================================
        [HttpPost]
        public IActionResult DeleteAccount(int userId)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Không có quyền" });

            try
            {
                var user = _context.Users.Find(userId);
                if (user == null)
                    return Json(new { success = false, message = "Không tìm thấy tài khoản" });

                _context.Users.Remove(user);
                _context.SaveChanges();

                return Json(new { success = true, message = "Xóa tài khoản thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // ================================
        // API: Lấy chi tiết nhân viên (AJAX)
        // ================================
        [HttpGet]
        public IActionResult GetEmployeeDetail(int id)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Không có quyền" });

            var employee = _context.NhanVien
                .Where(nv => nv.MaNV == id)
                .Select(nv => new
                {
                    nv.MaNV,
                    nv.TenNV,
                    nv.GioiTinh,
                    nv.SDT,
                    nv.DiaChi,
                    nv.VaiTro,
                    nv.AnhDaiDien,
                    Username = nv.User != null ? nv.User.Username : null,
                    SoTour = _context.NhanVien_Tour.Count(nt => nt.MaNV == nv.MaNV),
                    Tours = _context.NhanVien_Tour
                        .Where(nt => nt.MaNV == nv.MaNV)
                        .Join(_context.Tour, nt => nt.MaTour, t => t.MaTour, (nt, t) => new
                        {
                            t.MaTour,
                            t.TenTour,
                            t.NgayBatDau,
                            t.Gia
                        })
                        .Take(20) // Giới hạn 20 tour
                        .ToList()
                })
                .FirstOrDefault();

            if (employee == null)
                return Json(new { success = false, message = "Không tìm thấy nhân viên" });

            return Json(new { success = true, data = employee });
        }

        // ================================
        // API: Lấy chi tiết khách hàng (AJAX)
        // ================================
        [HttpGet]
        public IActionResult GetCustomerDetail(int id)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Không có quyền" });

            var customer = _context.KhachHang
                .Where(kh => kh.MaKH == id)
                .Select(kh => new
                {
                    kh.MaKH,
                    kh.TenKH,
                    kh.GioiTinh,
                    kh.SDT,
                    kh.DiaChi,
                    kh.AnhDaiDien,
                    Username = kh.User != null ? kh.User.Username : null,
                    SoTour = _context.HoaDon.Count(h => h.MaKH == kh.MaKH),
                    Tours = _context.HoaDon
                        .Where(h => h.MaKH == kh.MaKH)
                        .Join(_context.Tour, h => h.MaTour, t => t.MaTour, (h, t) => new
                        {
                            t.MaTour,
                            t.TenTour,
                            t.NgayBatDau,
                            t.Gia,
                            SoLuongVe = 1
                        })
                        .Take(20) // Giới hạn 20 tour
                        .ToList()
                })
                .FirstOrDefault();

            if (customer == null)
                return Json(new { success = false, message = "Không tìm thấy khách hàng" });

            return Json(new { success = true, data = customer });
        }

        // ================================
        // 3. TOURS - Quản lý Tour
        // ================================
        public IActionResult Tours(int page = 1, int pageSize = 10)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var totalTours = _context.Tour.Count();
            var totalPages = (int)Math.Ceiling(totalTours / (double)pageSize);

            var tours = _context.Tour
                .Include(t => t.Tour_Anh)
                .Include(t => t.NhanVien_Tour).ThenInclude(nt => nt.NhanVien)
                .OrderBy(t => t.MaTour)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(tours);
        }

        // ================================
        // GET: Tạo tour mới
        // ================================
        [HttpGet]
        public IActionResult CreateTour()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.NhanViens = _context.NhanVien.ToList();
            ViewBag.DiaDiems = _context.DiaDiem.ToList();
            ViewBag.PhuongTiens = _context.PhuongTien.ToList();
            
            return View();
        }

        // ================================
        // POST: Tạo tour mới
        // ================================
        [HttpPost]
        public async Task<IActionResult> CreateTour(Tour tour, IFormFile? anhDaiDien, List<IFormFile>? albumAnhs, 
            string? diaDiemIds, string? phuongTienIds, int? maNV)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            try
            {
                // Tạo MaTour mới
                var maxMaTour = _context.Tour.Any() ? _context.Tour.Max(t => t.MaTour) : 0;
                tour.MaTour = maxMaTour + 1;

                // Xử lý ảnh đại diện
                if (anhDaiDien != null && anhDaiDien.Length > 0)
                {
                    var fileName = await SaveTourImage(anhDaiDien, tour.MaTour);
                    tour.AnhDaiDien = $"tours/{fileName}";
                }

                _context.Tour.Add(tour);
                _context.SaveChanges();

                // Thêm ảnh album
                if (albumAnhs != null && albumAnhs.Count > 0)
                {
                    foreach (var file in albumAnhs)
                    {
                        if (file != null && file.Length > 0)
                        {
                            var fileName = await SaveTourImage(file, tour.MaTour);
                            _context.Tour_Anh.Add(new Tour_Anh
                            {
                                MaTour = tour.MaTour,
                                DuongDan = $"tours/{fileName}"
                            });
                        }
                    }
                    _context.SaveChanges();
                }

                // Thêm địa điểm
                if (!string.IsNullOrEmpty(diaDiemIds))
                {
                    var ddIds = diaDiemIds.Split(',').Select(int.Parse).ToList();
                    foreach (var ddId in ddIds)
                    {
                        _context.Tour_DiaDiem.Add(new Tour_DiaDiem
                        {
                            MaTour = tour.MaTour,
                            MaDD = ddId
                        });
                    }
                    _context.SaveChanges();
                }

                // Thêm phương tiện
                if (!string.IsNullOrEmpty(phuongTienIds))
                {
                    var ptIds = phuongTienIds.Split(',').Select(int.Parse).ToList();
                    foreach (var ptId in ptIds)
                    {
                        _context.Tour_PhuongTien.Add(new Tour_PhuongTien
                        {
                            MaTour = tour.MaTour,
                            MaPT = ptId
                        });
                    }
                    _context.SaveChanges();
                }

                // Thêm nhân viên hướng dẫn
                if (maNV.HasValue)
                {
                    _context.NhanVien_Tour.Add(new NhanVien_Tour
                    {
                        MaTour = tour.MaTour,
                        MaNV = maNV.Value
                    });
                    _context.SaveChanges();
                }

                TempData["Success"] = "Tạo tour thành công!";
                return RedirectToAction("Tours");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("CreateTour");
            }
        }

        // ================================
        // GET: Chỉnh sửa tour
        // ================================
        [HttpGet]
        public IActionResult EditTour(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var tour = _context.Tour
                .Include(t => t.Tour_Anh)
                .Include(t => t.Tour_DiaDiem)
                .Include(t => t.Tour_PhuongTien)
                .Include(t => t.NhanVien_Tour)
                .FirstOrDefault(t => t.MaTour == id);

            if (tour == null)
            {
                TempData["Error"] = "Không tìm thấy tour";
                return RedirectToAction("Tours");
            }

            ViewBag.NhanViens = _context.NhanVien.ToList();
            ViewBag.DiaDiems = _context.DiaDiem.ToList();
            ViewBag.PhuongTiens = _context.PhuongTien.ToList();

            return View(tour);
        }

        // ================================
        // POST: Cập nhật tour
        // ================================
        [HttpPost]
        public async Task<IActionResult> EditTour(Tour model, IFormFile? anhDaiDien, List<IFormFile>? albumAnhs,
            string? diaDiemIds, string? phuongTienIds, int? maNV)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            try
            {
                var tour = _context.Tour.Find(model.MaTour);
                if (tour == null)
                {
                    TempData["Error"] = "Không tìm thấy tour";
                    return RedirectToAction("Tours");
                }

                // Cập nhật thông tin cơ bản
                tour.TenTour = model.TenTour;
                tour.NgayBatDau = model.NgayBatDau;
                tour.NgayKetThuc = model.NgayKetThuc;
                tour.Gia = model.Gia;
                tour.MoTa = model.MoTa;

                // Xử lý ảnh đại diện mới
                if (anhDaiDien != null && anhDaiDien.Length > 0)
                {
                    // Xóa ảnh cũ
                    if (!string.IsNullOrEmpty(tour.AnhDaiDien))
                    {
                        DeleteTourImage(tour.AnhDaiDien);
                    }

                    var fileName = await SaveTourImage(anhDaiDien, tour.MaTour);
                    tour.AnhDaiDien = $"tours/{fileName}";
                }

                // Cập nhật địa điểm
                var existingDiaDiems = _context.Tour_DiaDiem.Where(td => td.MaTour == tour.MaTour).ToList();
                _context.Tour_DiaDiem.RemoveRange(existingDiaDiems);

                if (!string.IsNullOrEmpty(diaDiemIds))
                {
                    var ddIds = diaDiemIds.Split(',').Select(int.Parse).ToList();
                    foreach (var ddId in ddIds)
                    {
                        _context.Tour_DiaDiem.Add(new Tour_DiaDiem { MaTour = tour.MaTour, MaDD = ddId });
                    }
                }

                // Cập nhật phương tiện
                var existingPhuongTiens = _context.Tour_PhuongTien.Where(tp => tp.MaTour == tour.MaTour).ToList();
                _context.Tour_PhuongTien.RemoveRange(existingPhuongTiens);

                if (!string.IsNullOrEmpty(phuongTienIds))
                {
                    var ptIds = phuongTienIds.Split(',').Select(int.Parse).ToList();
                    foreach (var ptId in ptIds)
                    {
                        _context.Tour_PhuongTien.Add(new Tour_PhuongTien { MaTour = tour.MaTour, MaPT = ptId });
                    }
                }

                // Cập nhật nhân viên
                var existingNhanVien = _context.NhanVien_Tour.Where(nt => nt.MaTour == tour.MaTour).ToList();
                _context.NhanVien_Tour.RemoveRange(existingNhanVien);

                if (maNV.HasValue)
                {
                    _context.NhanVien_Tour.Add(new NhanVien_Tour { MaTour = tour.MaTour, MaNV = maNV.Value });
                }

                _context.SaveChanges();

                TempData["Success"] = "Cập nhật tour thành công!";
                return RedirectToAction("Tours");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("EditTour", new { id = model.MaTour });
            }
        }

        // ================================
        // POST: Xóa tour
        // ================================
        [HttpPost]
        public IActionResult DeleteTour(int id)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Không có quyền" });

            try
            {
                var tour = _context.Tour
                    .Include(t => t.Tour_Anh)
                    .FirstOrDefault(t => t.MaTour == id);

                if (tour == null)
                    return Json(new { success = false, message = "Không tìm thấy tour" });

                // Xóa ảnh đại diện
                if (!string.IsNullOrEmpty(tour.AnhDaiDien))
                {
                    DeleteTourImage(tour.AnhDaiDien);
                }

                // Xóa ảnh album
                foreach (var anh in tour.Tour_Anh)
                {
                    DeleteTourImage(anh.DuongDan);
                }

                _context.Tour.Remove(tour);
                _context.SaveChanges();

                return Json(new { success = true, message = "Xóa tour thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // ================================
        // Helper: Lưu ảnh tour
        // ================================
        private async Task<string> SaveTourImage(IFormFile file, int tourId)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new Exception("Chỉ chấp nhận file ảnh (.jpg, .jpeg, .png, .gif)");

            if (file.Length > 10 * 1024 * 1024)
                throw new Exception("Kích thước file không được vượt quá 10MB");

            var fileName = $"tour_{tourId}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "tours");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        // ================================
        // Helper: Xóa ảnh tour
        // ================================
        private void DeleteTourImage(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            var filePath = Path.Combine(_environment.WebRootPath, "images", relativePath);
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Không thể xóa ảnh: {ex.Message}");
                }
            }
        }
    }
}

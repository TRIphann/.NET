using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDuLichRBAC_Upgrade.Models;
using QLDuLichRBAC_Upgrade.Models.Entities;

namespace QLDuLichRBAC_Upgrade.Controllers
{
    public class AdminController : Controller
    {
        private readonly QLJumaparenaContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminController(QLJumaparenaContext context, IWebHostEnvironment environment)
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

            var user = _context.User.FirstOrDefault(u => u.Username == username);
            return user?.Role == "Admin";
        }

        // ================================
        // Dashboard - Redirect đến Statistics
        // ================================
        public IActionResult Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return RedirectToAction("Statistics", new { tab = "packages" });
        }

        // ================================
        // 1. STATISTICS - Thống kê
        // ================================
        [HttpGet]
        public IActionResult Statistics(string tab = "packages", string period = "week", DateTime? startDate = null)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.CurrentTab = tab;
            ViewBag.Period = period;
            
            DateTime start, end;
            if (startDate.HasValue)
            {
                start = startDate.Value;
                end = period == "week" ? start.AddDays(6) : start.AddMonths(1).AddDays(-1);
            }
            else
            {
                end = DateTime.Today;
                start = period == "week" ? end.AddDays(-6) : new DateTime(end.Year, end.Month, 1);
            }
            
            ViewBag.StartDate = start;
            ViewBag.EndDate = end;

            // Thống kê Gói Dịch Vụ (Packages)
            var topPackagesData = _context.GoiDichVu
                .GroupJoin(_context.Ve.Where(v => v.TrangThai != "Đã hủy"),
                    goi => goi.MaGoi,
                    ve => ve.MaGoi,
                    (goi, ves) => new
                    {
                        goi.MaGoi,
                        goi.TenGoi,
                        goi.Gia,
                        goi.ThoiGian,
                        goi.SoLuotChoi,
                        SoLuotMua = ves.Count(),
                        DoanhThu = ves.Sum(v => (decimal?)v.TongTien) ?? 0
                    })
                .OrderByDescending(x => x.SoLuotMua)
                .Take(10)
                .ToList();

            var packagesByPriceData = _context.GoiDichVu
                .GroupBy(g => g.Gia < 100000 ? "< 100k" : 
                              g.Gia < 200000 ? "100-200k" : 
                              g.Gia < 300000 ? "200-300k" : "> 300k")
                .Select(g => new
                {
                    Range = g.Key,
                    Count = g.Count()
                })
                .ToList();

            // Doanh thu theo tháng (6 tháng gần nhất)
            var revenueByMonthData = _context.HoaDon
                .Where(h => h.NgayTT >= DateTime.Now.AddMonths(-6))
                .GroupBy(h => new { h.NgayTT.Year, h.NgayTT.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    DoanhThu = g.Sum(x => x.TongTien)
                })
                .ToList()
                .Select(x => new
                {
                    Thang = $"{x.Month:D2}/{x.Year}",
                    DoanhThu = x.DoanhThu
                })
                .OrderBy(x => x.Thang)
                .ToList();

            // Vé bán theo ngày trong tuần (tuần hiện tại)
            var today = DateTime.Today;
            var dayOfWeek = (int)today.DayOfWeek;
            // Nếu Chủ nhật (0), chuyển thành 7 để tính toán đúng
            if (dayOfWeek == 0) dayOfWeek = 7;
            var monday = today.AddDays(1 - dayOfWeek);
            var sunday = monday.AddDays(6);
            
            // Lấy tất cả vé trong tuần một lần
            var weekTickets = _context.Ve
                .Where(v => v.NgayDat >= monday && v.NgayDat < sunday.AddDays(1) && v.TrangThai != "Đã hủy")
                .Select(v => v.NgayDat)
                .ToList();
            
            var ticketsByDayData = Enumerable.Range(0, 7)
                .Select(i => {
                    var date = monday.AddDays(i);
                    var count = weekTickets.Count(d => d.Date == date);
                    return new
                    {
                        Day = date.ToString("ddd dd/MM", new System.Globalization.CultureInfo("vi-VN")),
                        Count = count,
                        IsToday = date.Date == today
                    };
                })
                .ToList();

            var packageStats = new
            {
                TotalPackages = _context.GoiDichVu.Count(),
                ActivePackages = _context.GoiDichVu.Count(g => g.TrangThai == "Đang hoạt động"),
                TotalTicketsSold = _context.Ve.Count(v => v.TrangThai != "Đã hủy"),
                TotalRevenue = _context.Ve
                    .Where(v => v.TrangThai != "Đã hủy")
                    .Sum(v => (decimal?)v.TongTien) ?? 0,
                TopPackages = topPackagesData,
                PackagesByPrice = packagesByPriceData,
                RevenueByMonth = revenueByMonthData,
                TicketsByDayOfWeek = ticketsByDayData
            };

            // Thống kê Nhân viên
            var topEmployeesData = _context.NhanVien
                .Select(nv => new
                {
                    nv.MaNV,
                    TenNV = nv.HoTen,
                    nv.GioiTinh,
                    nv.SDT,
                    SoCa = _context.NhanVien_Ca
                        .Count(nc => nc.MaNV == nv.MaNV && 
                               nc.NgayLamViec >= start && nc.NgayLamViec <= end),
                    CaHoanThanh = _context.NhanVien_Ca
                        .Count(nc => nc.MaNV == nv.MaNV && 
                               nc.TrangThaiDiemDanh == "Đã điểm danh" &&
                               nc.NgayLamViec >= start && nc.NgayLamViec <= end)
                })
                .OrderByDescending(x => x.SoCa)
                .Take(10)
                .ToList();

            var employeesByGenderData = _context.NhanVien
                .GroupBy(nv => nv.GioiTinh)
                .Select(g => new
                {
                    GioiTinh = g.Key ?? "Không rõ",
                    Count = g.Count()
                })
                .ToList();

            var employeeShiftData = _context.NhanVien_Ca
                .Where(nc => nc.NgayLamViec >= start && nc.NgayLamViec <= end)
                .GroupBy(nc => nc.TrangThaiDiemDanh)
                .Select(g => new
                {
                    TrangThai = g.Key ?? "Chưa điểm danh",
                    Count = g.Count()
                })
                .ToList();

            // Thống kê ca theo ngày (30 ngày gần nhất)
            var shiftsByDateData = Enumerable.Range(0, 30)
                .Select(i => {
                    var date = DateTime.Today.AddDays(-29 + i);
                    var count = _context.NhanVien_Ca.Count(nc => nc.NgayLamViec.Date == date);
                    return new
                    {
                        Date = date.ToString("dd/MM"),
                        Count = count
                    };
                })
                .ToList();

            // Thống kê ca theo loại ca
            var shiftsByCaData = _context.NhanVien_Ca
                .Where(nc => nc.NgayLamViec >= start && nc.NgayLamViec <= end)
                .GroupBy(nc => nc.Ca.TenCa)
                .Select(g => new
                {
                    TenCa = g.Key ?? "Không rõ",
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            var staffStats = new
            {
                TotalStaff = _context.NhanVien.Count(),
                ActiveStaff = _context.NhanVien_Ca
                    .Where(nc => nc.NgayLamViec >= start && nc.NgayLamViec <= end)
                    .Select(nc => nc.MaNV)
                    .Distinct()
                    .Count(),
                ActiveToday = _context.NhanVien_Ca
                    .Count(nc => nc.NgayLamViec.Date == DateTime.Today),
                TopStaff = topEmployeesData,
                StaffByGender = employeesByGenderData,
                StaffByRole = new[] { new { Role = "Nhân viên", Count = _context.NhanVien.Count() } },
                ShiftsByStatus = employeeShiftData,
                ShiftsByCa = shiftsByCaData,
                ShiftsByDate = shiftsByDateData
            };

            // Thống kê Khách hàng
            var topCustomersData = _context.KhachHang
                .GroupJoin(_context.Ve.Where(v => v.TrangThai != "Đã hủy"),
                    kh => kh.MaKH,
                    ve => ve.MaKH,
                    (kh, ves) => new
                    {
                        kh.MaKH,
                        kh.TenKH,
                        kh.SDT,
                        kh.GioiTinh,
                        SoVe = ves.Count(),
                        TongChiTieu = ves.Sum(v => (decimal?)v.TongTien) ?? 0
                    })
                .OrderByDescending(x => x.TongChiTieu)
                .Take(10)
                .ToList();

            var customersByGenderData = _context.KhachHang
                .GroupBy(kh => kh.GioiTinh)
                .Select(g => new
                {
                    GioiTinh = g.Key ?? "Không rõ",
                    Count = g.Count()
                })
                .ToList();

            var ticketsByStatusData = _context.Ve
                .Where(v => v.TrangThai != "Đã hủy" && v.NgayDat >= start && v.NgayDat <= end)
                .GroupBy(v => v.TrangThai)
                .Select(g => new
                {
                    TrangThai = g.Key ?? "Chưa sử dụng",
                    Count = g.Count()
                })
                .ToList();

            // Doanh thu và số khách theo tháng (6 tháng gần nhất)
            var customerRevenueByMonthData = _context.HoaDon
                .Where(h => h.NgayTT >= DateTime.Now.AddMonths(-6))
                .GroupBy(h => new { h.NgayTT.Year, h.NgayTT.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    DoanhThu = g.Sum(x => x.TongTien),
                    SoKhach = g.Select(x => x.MaKH).Distinct().Count()
                })
                .ToList()
                .Select(x => new
                {
                    Thang = $"{x.Month:D2}/{x.Year}",
                    DoanhThu = x.DoanhThu,
                    SoKhach = x.SoKhach
                })
                .OrderBy(x => x.Thang)
                .ToList();

            // Phương thức thanh toán
            var paymentMethodsData = _context.HoaDon
                .Where(h => h.NgayTT >= start && h.NgayTT <= end)
                .GroupBy(h => h.HinhThucTT)
                .Select(g => new
                {
                    Method = g.Key ?? "Không rõ",
                    Count = g.Count()
                })
                .ToList();

            var customerStats = new
            {
                TotalCustomers = _context.KhachHang.Count(),
                ActiveCustomers = _context.Ve
                    .Where(v => v.TrangThai != "Đã hủy" && v.NgayDat >= start && v.NgayDat <= end)
                    .Select(v => v.MaKH)
                    .Distinct()
                    .Count(),
                TopCustomers = topCustomersData,
                CustomersByGender = customersByGenderData,
                TicketsByStatus = ticketsByStatusData,
                RevenueByMonth = customerRevenueByMonthData,
                PaymentMethods = paymentMethodsData
            };

            // Thống kê Doanh thu
            var revenueData = _context.HoaDon
                .Where(h => h.NgayTT >= start && h.NgayTT <= end)
                .GroupBy(h => new { h.NgayTT.Year, h.NgayTT.Month, h.NgayTT.Day })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Day = g.Key.Day,
                    DoanhThu = g.Sum(x => x.TongTien),
                    SoHoaDon = g.Count()
                })
                .ToList()
                .Select(x => new
                {
                    Date = new DateTime(x.Year, x.Month, x.Day),
                    x.DoanhThu,
                    x.SoHoaDon
                })
                .OrderBy(x => x.Date)
                .ToList();

            var revenueStats = new
            {
                TotalRevenue = _context.HoaDon
                    .Where(h => h.NgayTT >= start && h.NgayTT <= end)
                    .Sum(h => (decimal?)h.TongTien) ?? 0,
                TotalInvoices = _context.HoaDon
                    .Count(h => h.NgayTT >= start && h.NgayTT <= end),
                RevenueByDay = revenueData
            };

            ViewBag.PackageStats = packageStats;
            ViewBag.StaffStats = staffStats;
            ViewBag.CustomerStats = customerStats;
            ViewBag.RevenueStats = revenueStats;

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

                // Get employee IDs for pagination
                var employeeIds = _context.NhanVien
                    .OrderBy(nv => nv.MaNV)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(nv => nv.MaNV)
                    .ToList();

                var shiftCounts = _context.NhanVien_Ca
                    .Where(nc => employeeIds.Contains(nc.MaNV))
                    .GroupBy(nc => nc.MaNV)
                    .Select(g => new { MaNV = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.MaNV, x => x.Count);

                var employees = _context.NhanVien
                    .Include(nv => nv.User)
                    .Where(nv => employeeIds.Contains(nv.MaNV))
                    .OrderBy(nv => nv.MaNV)
                    .AsEnumerable()
                    .Select(nv => new
                    {
                        nv.MaNV,
                        TenNV = nv.HoTen,
                        nv.GioiTinh,
                        nv.SDT,
                        nv.DiaChi,
                        nv.AnhDaiDien,
                        Username = nv.User != null ? nv.User.Username : null,
                        UserId = nv.UserId,
                        SoCa = shiftCounts.ContainsKey(nv.MaNV) ? shiftCounts[nv.MaNV] : 0
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

                // Get customer IDs for pagination
                var customerIds = _context.KhachHang
                    .OrderBy(kh => kh.MaKH)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(kh => kh.MaKH)
                    .ToList();

                var ticketCounts = _context.Ve
                    .Where(v => customerIds.Contains(v.MaKH) && v.TrangThai != "Đã hủy")
                    .GroupBy(v => v.MaKH)
                    .Select(g => new { MaKH = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.MaKH, x => x.Count);

                var customers = _context.KhachHang
                    .Include(kh => kh.User)
                    .Where(kh => customerIds.Contains(kh.MaKH))
                    .OrderBy(kh => kh.MaKH)
                    .AsEnumerable()
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
                        SoVe = ticketCounts.ContainsKey(kh.MaKH) ? ticketCounts[kh.MaKH] : 0
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
                if (_context.User.Any(u => u.Username == username))
                {
                    return Json(new { success = false, message = "Username đã tồn tại" });
                }

                // Tạo User
                var user = new User
                {
                    Username = username,
                    PasswordHash = password, // Trong thực tế nên hash password
                    Role = "Staff"
                };
                _context.User.Add(user);
                _context.SaveChanges();

                // Tạo NhanVien
                var maNV = _context.NhanVien.Any() 
                    ? _context.NhanVien.Max(nv => nv.MaNV) + 1 
                    : 1;

                var nhanVien = new NhanVien
                {
                    MaNV = maNV,
                    HoTen = tenNV,
                    SDT = sdt,
                    DiaChi = diaChi,
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
                var user = _context.User.Find(userId);
                if (user == null)
                    return Json(new { success = false, message = "Không tìm thấy tài khoản" });

                _context.User.Remove(user);
                _context.SaveChanges();

                return Json(new { success = true, message = "Xóa tài khoản thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // ================================
        // Xóa nhân viên
        // ================================
        [HttpPost]
        public IActionResult DeleteEmployee(int maNV, int? userId)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Không có quyền" });

            try
            {
                var employee = _context.NhanVien.Find(maNV);
                if (employee == null)
                    return Json(new { success = false, message = "Không tìm thấy nhân viên" });

                // Xóa nhân viên (cascade sẽ xóa lịch làm việc)
                _context.NhanVien.Remove(employee);

                // Xóa tài khoản nếu có
                if (userId.HasValue)
                {
                    var user = _context.User.Find(userId.Value);
                    if (user != null)
                    {
                        _context.User.Remove(user);
                    }
                }

                _context.SaveChanges();

                return Json(new { success = true, message = "Xóa nhân viên thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // ================================
        // Xóa khách hàng
        // ================================
        [HttpPost]
        public IActionResult DeleteCustomer(int maKH, int? userId)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Không có quyền" });

            try
            {
                var customer = _context.KhachHang.Find(maKH);
                if (customer == null)
                    return Json(new { success = false, message = "Không tìm thấy khách hàng" });

                // Kiểm tra xem khách hàng có vé chưa sử dụng không
                var hasActiveTickets = _context.Ve.Any(v => v.MaKH == maKH && v.TrangThai != "Đã hủy" && v.TrangThai != "Đã sử dụng");
                if (hasActiveTickets)
                {
                    return Json(new { success = false, message = "Không thể xóa khách hàng có vé chưa sử dụng" });
                }

                // Xóa khách hàng
                _context.KhachHang.Remove(customer);

                // Xóa tài khoản nếu có
                if (userId.HasValue)
                {
                    var user = _context.User.Find(userId.Value);
                    if (user != null)
                    {
                        _context.User.Remove(user);
                    }
                }

                _context.SaveChanges();

                return Json(new { success = true, message = "Xóa khách hàng thành công" });
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
                    TenNV = nv.HoTen,
                    nv.GioiTinh,
                    nv.SDT,
                    nv.DiaChi,
                    nv.AnhDaiDien,
                    Username = nv.User != null ? nv.User.Username : null,
                    SoCa = _context.NhanVien_Ca.Count(nc => nc.MaNV == nv.MaNV),
                    RecentShifts = _context.NhanVien_Ca
                        .Where(nc => nc.MaNV == nv.MaNV)
                        .OrderByDescending(nc => nc.NgayLamViec)
                        .Select(nc => new
                        {
                            nc.MaCa,
                            TenCa = nc.Ca != null ? nc.Ca.TenCa : null,
                            nc.NgayLamViec,
                            TrangThai = nc.TrangThaiDiemDanh ?? "Chưa điểm danh"
                        })
                        .Take(20)
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
                    SoVe = _context.Ve.Count(v => v.MaKH == kh.MaKH),
                    RecentTickets = _context.Ve
                        .Where(v => v.MaKH == kh.MaKH)
                        .OrderByDescending(v => v.NgayDat)
                        .Select(v => new
                        {
                            v.MaVe,
                            TenGoi = v.GoiDichVu != null ? v.GoiDichVu.TenGoi : null,
                            NgayMua = v.NgayDat,
                            v.TrangThai,
                            GiaVe = v.GoiDichVu != null ? v.GoiDichVu.Gia : 0
                        })
                        .Take(20)
                        .ToList()
                })
                .FirstOrDefault();

            if (customer == null)
                return Json(new { success = false, message = "Không tìm thấy khách hàng" });

            return Json(new { success = true, data = customer });
        }

        // ================================
        // 3. API: Thêm Gói Dịch Vụ
        // ================================
        [HttpPost]
        public IActionResult AddPackage([FromBody] AddPackageRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.TenGoi))
                    return Json(new { success = false, message = "Tên gói không được để trống" });

                if (request.Gia <= 0)
                    return Json(new { success = false, message = "Giá gói phải lớn hơn 0" });

                var package = new GoiDichVu
                {
                    TenGoi = request.TenGoi.Trim(),
                    MoTa = request.MoTa?.Trim(),
                    Gia = request.Gia,
                    ThoiGian = request.ThoiGian > 0 ? request.ThoiGian : null,
                    SoLuotChoi = request.SoLuotChoi > 0 ? request.SoLuotChoi : null,
                    TrangThai = "Đang bán"
                };

                _context.GoiDichVu.Add(package);
                _context.SaveChanges();

                return Json(new { 
                    success = true, 
                    message = "Thêm gói dịch vụ thành công!",
                    data = new {
                        maGoi = package.MaGoi,
                        tenGoi = package.TenGoi,
                        gia = package.Gia,
                        thoiGian = package.ThoiGian,
                        soLuotChoi = package.SoLuotChoi
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Request model cho AddPackage
        public class AddPackageRequest
        {
            public string TenGoi { get; set; }
            public string? MoTa { get; set; }
            public decimal Gia { get; set; }
            public int? ThoiGian { get; set; }
            public int? SoLuotChoi { get; set; }
        }

        // API để lấy dữ liệu vé theo tuần
        [HttpGet]
        public IActionResult GetWeeklyTickets(string startDate)
        {
            try
            {
                // Parse startDate (là thứ 2 của tuần)
                DateTime monday;
                if (!DateTime.TryParse(startDate, out monday))
                {
                    monday = DateTime.Today;
                    var dayOfWeek = (int)monday.DayOfWeek;
                    if (dayOfWeek == 0) dayOfWeek = 7;
                    monday = monday.AddDays(1 - dayOfWeek);
                }

                var sunday = monday.AddDays(6);
                var today = DateTime.Today;
                
                // Lấy tất cả vé trong tuần
                var weekTickets = _context.Ve
                    .Where(v => v.NgayDat >= monday && v.NgayDat < sunday.AddDays(1) && v.TrangThai != "Đã hủy")
                    .Select(v => v.NgayDat)
                    .ToList();
                
                var ticketsByDay = Enumerable.Range(0, 7)
                    .Select(i => {
                        var date = monday.AddDays(i);
                        var count = weekTickets.Count(d => d.Date == date);
                        return new
                        {
                            Day = date.ToString("ddd dd/MM", new System.Globalization.CultureInfo("vi-VN")),
                            Count = count,
                            IsToday = date.Date == today
                        };
                    })
                    .ToList();

                return Json(ticketsByDay);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // API test để debug
        [HttpGet]
        public IActionResult TestWeekData()
        {
            var today = DateTime.Today;
            var dayOfWeek = (int)today.DayOfWeek;
            if (dayOfWeek == 0) dayOfWeek = 7;
            var monday = today.AddDays(1 - dayOfWeek);
            var sunday = monday.AddDays(6);
            
            var weekTickets = _context.Ve
                .Where(v => v.NgayDat >= monday && v.NgayDat < sunday.AddDays(1) && v.TrangThai != "Đã hủy")
                .Select(v => new { v.MaVe, v.NgayDat, v.TrangThai })
                .ToList();
            
            var ticketsByDay = Enumerable.Range(0, 7)
                .Select(i => {
                    var date = monday.AddDays(i);
                    var tickets = weekTickets.Where(d => d.NgayDat.Date == date).ToList();
                    return new
                    {
                        Date = date.ToString("yyyy-MM-dd"),
                        Day = date.ToString("ddd dd/MM", new System.Globalization.CultureInfo("vi-VN")),
                        Count = tickets.Count,
                        IsToday = date.Date == today,
                        Tickets = tickets
                    };
                })
                .ToList();

            return Json(new { 
                today = today.ToString("yyyy-MM-dd"),
                monday = monday.ToString("yyyy-MM-dd"),
                sunday = sunday.ToString("yyyy-MM-dd"),
                dayOfWeek,
                totalTickets = weekTickets.Count,
                ticketsByDay
            });
        }

        // API: Lấy lịch làm việc nhân viên theo tuần
        [HttpGet]
        public IActionResult GetStaffWeeklySchedule(string startDate)
        {
            try
            {
                DateTime selectedDate;
                if (!DateTime.TryParse(startDate, out selectedDate))
                {
                    selectedDate = DateTime.Today;
                }

                var today = DateTime.Today;

                // Lấy tất cả nhân viên
                var staffList = _context.NhanVien
                    .OrderBy(nv => nv.HoTen)
                    .Select(nv => new
                    {
                        MaNV = nv.MaNV,
                        TenNV = nv.HoTen
                    })
                    .ToList();

                // Lấy tất cả ca làm việc trong tuần
                var weekShiftsRaw = _context.NhanVien_Ca
                    .Where(nc => nc.NgayLamViec >= selectedDate && nc.NgayLamViec < selectedDate.AddDays(7))
                    .Select(nc => new
                    {
                        nc.MaNV,
                        nc.NgayLamViec,
                        nc.MaCa,
                        nc.TrangThaiDiemDanh
                    })
                    .ToList();

                // Lấy thông tin ca
                var caIds = weekShiftsRaw.Select(ws => ws.MaCa).Distinct().ToList();
                var caList = _context.Ca
                    .Where(c => caIds.Contains(c.MaCa))
                    .AsEnumerable()
                    .Select(c => new
                    {
                        c.MaCa,
                        c.TenCa,
                        c.GioBatDau,
                        c.GioKetThuc
                    })
                    .ToDictionary(c => c.MaCa);
                
                var weekShifts = weekShiftsRaw.Select(nc => {
                    var ca = caList.ContainsKey(nc.MaCa) ? caList[nc.MaCa] : null;
                    return new
                    {
                        nc.MaNV,
                        nc.NgayLamViec,
                        TenCa = ca?.TenCa ?? "N/A",
                        GioBatDau = ca?.GioBatDau ?? TimeOnly.MinValue,
                        GioKetThuc = ca?.GioKetThuc ?? TimeOnly.MinValue,
                        TrangThai = nc.TrangThaiDiemDanh ?? "Chưa điểm danh"
                    };
                }).ToList();

                // Tạo dữ liệu cho từng nhân viên
                var scheduleData = staffList.Select(staff => {
                    var weekData = Enumerable.Range(0, 7).Select(i => {
                        var date = selectedDate.AddDays(i);
                        
                        // Chỉ hiển thị ca cho ngày hiện tại và quá khứ
                        List<object> dayShifts;
                        if (date.Date <= today)
                        {
                            // Ngày hiện tại hoặc quá khứ: hiển thị ca thực tế từ database
                            dayShifts = weekShifts
                                .Where(s => s.MaNV == staff.MaNV && s.NgayLamViec.Date == date)
                                .Select(s => new
                                {
                                    TenCa = s.TenCa,
                                    GioBatDau = s.GioBatDau.ToString("HH:mm"),
                                    GioKetThuc = s.GioKetThuc.ToString("HH:mm"),
                                    TrangThai = s.TrangThai,
                                    ShiftType = GetShiftType(s.GioBatDau),
                                    IsPast = true
                                })
                                .Cast<object>()
                                .ToList();
                        }
                        else
                        {
                            // Ngày tương lai: để trống
                            dayShifts = new List<object>();
                        }

                        return new
                        {
                            Date = date.ToString("yyyy-MM-dd"),
                            Shifts = dayShifts,
                            IsFuture = date.Date > today
                        };
                    }).ToList();

                    return new
                    {
                        MaNV = staff.MaNV,
                        TenNV = staff.TenNV,
                        WeekData = weekData
                    };
                }).ToList();

                // Tạo header cho các ngày trong tuần
                var weekDays = Enumerable.Range(0, 7).Select(i => {
                    var date = selectedDate.AddDays(i);
                    return new
                    {
                        Date = date.ToString("yyyy-MM-dd"),
                        DayName = date.ToString("ddd", new System.Globalization.CultureInfo("vi-VN")),
                        DayMonth = date.ToString("dd/MM"),
                        IsToday = date.Date == today
                    };
                }).ToList();

                return Json(new
                {
                    success = true,
                    weekDays = weekDays,
                    scheduleData = scheduleData,
                    startDate = selectedDate.ToString("yyyy-MM-dd"),
                    endDate = selectedDate.AddDays(6).ToString("yyyy-MM-dd")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        private string GetShiftType(TimeOnly gioBatDau)
        {
            var hour = gioBatDau.Hour;
            if (hour >= 7 && hour < 12) return "morning";
            if (hour >= 12 && hour < 17) return "afternoon";
            if (hour >= 17 && hour < 22) return "evening";
            return "full-day";
        }

        // ================================
        // FIX VIETNAMESE ENCODING
        // ================================
        [HttpGet]
        public IActionResult FixVietnameseEncoding()
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Không có quyền" });

            try
            {
                // Fix Gói Dịch Vụ
                var packages = _context.GoiDichVu.ToList();
                var packageNames = new Dictionary<int, string>
                {
                    { 1, "Gói Cơ Bản 1 Giờ" },
                    { 2, "Gói Tiêu Chuẩn 2 Giờ" },
                    { 3, "Gói VIP 3 Giờ" },
                    { 4, "Gói Premium Cả Ngày" },
                    { 5, "Gói Gia Đình 4 Người" },
                    { 6, "Gói Học Sinh Sinh Viên" },
                    { 7, "Gói Cuối Tuần Ảo Diệu" },
                    { 8, "Gói 10 Lượt Chơi" },
                    { 9, "Gói 20 Lượt Chơi" },
                    { 10, "Gói Nhóm 10 Người" }
                };

                foreach (var pkg in packages)
                {
                    if (packageNames.ContainsKey(pkg.MaGoi))
                    {
                        pkg.TenGoi = packageNames[pkg.MaGoi];
                    }
                }

                // Fix Nhân Viên
                var employees = _context.NhanVien.ToList();
                var employeeNames = new Dictionary<int, string>
                {
                    { 1, "Nguyễn Văn A" },
                    { 2, "Trần Thảo B" },
                    { 3, "Lê Thị Cẩm" },
                    { 4, "Phạm Văn Dũng" },
                    { 5, "Hoàng Thị Lan" },
                    { 6, "Đặng Văn Hùng" },
                    { 7, "Vũ Thị Mai" },
                    { 8, "Bùi Văn Tài" },
                    { 9, "Trương Thị Hoa" },
                    { 10, "Ngô Văn Khoa" }
                };

                foreach (var emp in employees)
                {
                    if (employeeNames.ContainsKey(emp.MaNV))
                    {
                        emp.HoTen = employeeNames[emp.MaNV];
                    }
                }

                // Fix Ca
                var shifts = _context.Ca.ToList();
                var shiftNames = new Dictionary<int, string>
                {
                    { 1, "Ca Sáng" },
                    { 2, "Ca Chiều" },
                    { 3, "Ca Tối" }
                };

                foreach (var shift in shifts)
                {
                    if (shiftNames.ContainsKey(shift.MaCa))
                    {
                        shift.TenCa = shiftNames[shift.MaCa];
                    }
                }

                _context.SaveChanges();

                return Json(new { 
                    success = true, 
                    message = "Đã fix encoding tiếng Việt thành công!",
                    packagesFixed = packages.Count,
                    employeesFixed = employees.Count,
                    shiftsFixed = shifts.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}

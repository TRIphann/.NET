# Jumparena Ticket Booking System

## 📋 Giới thiệu
Hệ thống đặt vé trực tuyến cho khu vui chơi Jumparena (https://jumparena.vn/) với phân quyền RBAC (Role-Based Access Control), được xây dựng bằng ASP.NET Core MVC và SQL Server.

## 🚀 Tính năng chính

### 👤 3 loại người dùng:
1. **Admin** - Quản trị viên hệ thống
2. **Staff** - Nhân viên khu vui chơi
3. **Customer** - Khách hàng

### 🎯 Chức năng cho Customer:
- ✅ Xem danh sách gói dịch vụ (Vé lẻ, Team Building, Sinh nhật, Trường học)
- ✅ Đặt vé online: chọn ca giờ, số người, dịch vụ thêm
- ✅ Thanh toán qua QR code (VietQR + SePay API)
- ✅ Xem vé đã đặt với mã QR check-in
- ✅ Quản lý hồ sơ cá nhân, upload avatar

### 🎯 Chức năng cho Staff:
- ✅ Xem lịch làm việc theo ca
- ✅ Check-in vé: quét QR hoặc nhập mã thủ công
- ✅ Xem danh sách trò chơi và yêu cầu an toàn
- ✅ Thống kê giờ làm, số khách phục vụ

### 🎯 Chức năng cho Admin:
- ✅ Dashboard tổng quan doanh thu, vé bán
- ✅ Quản lý tài khoản người dùng
- ✅ Thống kê chi tiết theo ngày/tháng

## 📦 Cài đặt

### Yêu cầu hệ thống:
- .NET 8.0 SDK
- SQL Server 2019 trở lên
- Visual Studio 2022 hoặc VS Code

### Bước 1: Clone repository
```bash
git clone <repository-url>
cd QLDuLichRBAC_Final
```

### Bước 2: Cài đặt database
```bash
# Mở SQL Server Management Studio
# Chạy file: lib/QLJumparenal.sql
```

### Bước 3: Cấu hình connection string
Mở file `appsettings.json` và cập nhật:
```json
{
  "ConnectionStrings": {
    "QLJumparena": "Server=YOUR_SERVER;Database=QLJumparena;User ID=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True"
  }
}
```

### Bước 4: Chạy ứng dụng
```bash
dotnet restore
dotnet build
dotnet run
```

Hoặc sử dụng script:
```bash
dotnet watch run --project "D:\sql\PhanTichThietKEYC\QLDuLichRBAC_Final\QLDuLichRBAC_Final.csproj"
```

## 🔐 Tài khoản mặc định

| Username | Password | Role | Mô tả |
|----------|----------|------|-------|
| admin | 123 | Admin | Quản trị viên |
| staff1 | 123 | Staff | Nhân viên |
| customer1 | 123 | Customer | Khách hàng |

## 📂 Cấu trúc thư mục

```
QLDuLichRBAC_Final/
├── Controllers/           # Controllers (Admin, Customer, Staff, Account, Home)
├── Models/               
│   ├── Entities/         # Database entities (KhuVuiChoi, TroChoi, GoiDichVu, Ca, Ve...)
│   ├── ViewModels/       # View models (PaymentViewModel, MyTicketViewModel...)
│   └── QLJumaparenaContext.cs  # EF Core DbContext
├── Views/
│   ├── Admin/            # Admin views (Dashboard, Statistics, Tours...)
│   ├── Customer/         # Customer views (BookTickets, Payment, MyTickets...)
│   ├── Staff/            # Staff views (MySchedule, CheckInTickets, Activities...)
│   ├── Account/          # Login/Register views
│   └── Shared/           # Layout views (_CustomerLayout, _StaffLayout, _AdminLayout)
├── Services/             # Business logic services (PaymentService)
├── Utils/                # Helper classes (Security, AuthHelper, AlertHelper...)
├── wwwroot/
│   ├── css/              # Stylesheets (customer.css, guide-layout.css...)
│   ├── js/               # JavaScript files
│   └── images/           # Avatar images, album photos
├── lib/                  # SQL scripts
│   └── QLJumparenal.sql  # Main database script
├── appsettings.json      # Configuration
├── Program.cs            # Application entry point
└── README.md             # Tài liệu này
```

## 🛠️ Công nghệ sử dụng

- **Backend**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server 2022
- **ORM**: Entity Framework Core
- **Frontend**: HTML, CSS, JavaScript, Bootstrap
- **Payment**: VietQR API
- **Authentication**: ASP.NET Core Identity (custom)

## 🗄️ Cấu trúc Database

### Bảng chính:
- **KhuVuiChoi**: Khu vực trong công viên (Adventure Zone, Kids Zone...)
- **TroChoi**: Trò chơi cụ thể (Trampoline, Ninja Course, Ball Pool...)
- **GoiDichVu**: Gói dịch vụ (Vé lẻ, Team Building, Sinh nhật, Trường học)
- **Ca**: Ca giờ (30 phút/ca) với số lượng tối đa
- **Ve**: Vé với mã QR unique (Format: JPA{userId:D4}{packageId:D3}{guid})
- **DichVuThem**: Dịch vụ bổ sung (Nước uống, Giữ đồ, Bảo hiểm...)

### Bảng phụ trợ:
- **NhanVien_Ca**: Phân công ca làm việc
- **Ve_DichVuThem**: Dịch vụ thêm theo vé
- **KhuVuiChoi_TroChoi**: Trò chơi trong khu vực
- **GoiDichVu_TroChoi**: Trò chơi trong gói dịch vụ

## 📝 Các tính năng đã cập nhật

### Phiên bản 2.0 - Jumparena Transformation:
- ✅ **Database redesign**: Chuyển từ tour du lịch sang khu vui chơi
- ✅ **Payment integration**: VietQR + SePay API auto-check
- ✅ **QR Code check-in**: Mỗi vé có QR code unique
- ✅ **Staff management**: Lịch làm việc, check-in, thống kê
- ✅ **Responsive UI**: Giao diện Jumparena theme với gradient đẹp mắt
- ✅ **Time slot booking**: Đặt vé theo ca giờ 30 phút
- ✅ **Extra services**: Thêm dịch vụ bổ sung khi đặt vé

## 📖 Hướng dẫn sử dụng

### Đăng nhập Customer:
1. Truy cập: `http://localhost:5000` (hoặc port được config)
2. Đăng nhập với `customer1 / 123`
3. Xem danh sách gói dịch vụ
4. Chọn gói → chọn ngày, ca giờ, số người, dịch vụ thêm
5. Thanh toán qua QR code (tự động check mỗi 10 giây)
6. Xem vé đã đặt với QR code check-in

### Đăng nhập Staff:
1. Đăng nhập với `staff1 / 123`
2. Xem lịch làm việc của mình
3. Check-in vé khách: quét QR hoặc nhập mã
4. Xem danh sách hoạt động và yêu cầu an toàn
5. Xem thống kê giờ làm và số khách phục vụ

### Đăng nhập Admin:
1. Đăng nhập với `admin / 123`
2. Xem dashboard tổng quan
3. Quản lý tài khoản người dùng
4. Xem thống kê doanh thu chi tiết

## 🐛 Troubleshooting

### Lỗi kết nối database:
```
Kiểm tra connection string trong appsettings.json
Đảm bảo SQL Server đang chạy
Kiểm tra firewall và quyền truy cập
```

### Lỗi build:
```bash
# Dọn dẹp build
dotnet clean
dotnet restore
dotnet build
```

### Lỗi encoding tiếng Việt:
```
File đã được sửa với encoding UTF-8
Nếu vẫn lỗi, kiểm tra font chữ trong browser
```

## 📞 Liên hệ & Hỗ trợ

Nếu gặp vấn đề, vui lòng tạo issue trên repository hoặc liên hệ với nhóm phát triển.

## 📄 License

Dự án này được phát triển cho mục đích học tập.

## 🎨 Theme & Design
- **Primary Gradient**: #667eea → #764ba2 (Jumparena purple)
- **Success Gradient**: #43e97b → #38f9d7 (Green)
- **Font**: System fonts (optimal loading speed)
- **Responsive**: Mobile, Tablet, Desktop friendly

## 💳 Payment System
- **VietQR Integration**: Generate QR code for bank transfer
- **SePay API**: Auto-check transaction every 10 seconds
- **Transaction Code**: Unique per booking
- **Ticket Code Format**: JPA{userId:D4}{packageId:D3}{guid}

---

**Phát triển bởi**: Nhóm Phân tích Thiết kế Hệ thống
**Phiên bản**: 2.0.0 - Jumparena Edition
**Năm**: 2025


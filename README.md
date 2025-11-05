# QLDuLichRBAC - Hệ thống Quản lý Du lịch

## 📋 Giới thiệu
Hệ thống quản lý tour du lịch với phân quyền RBAC (Role-Based Access Control), được xây dựng bằng ASP.NET Core MVC và SQL Server.

## 🚀 Tính năng chính

### 👤 3 loại người dùng:
1. **Admin** - Quản trị viên hệ thống
2. **Guide** - Hướng dẫn viên du lịch  
3. **Customer** - Khách hàng

### 🎯 Chức năng cho Customer:
- ✅ Xem danh sách tour (chỉ hiển thị tour chưa đăng ký và còn hạn)
- ✅ Đăng ký và thanh toán tour
- ✅ Xem lịch sử tour đã thanh toán
- ✅ Quản lý hồ sơ cá nhân
- ✅ Thanh toán qua QR code (VietQR)

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
# Chạy file: lib/QLDuLichRBAC_Final.sql
```

### Bước 3: Cấu hình connection string
Mở file `appsettings.json` và cập nhật:
```json
{
  "ConnectionStrings": {
    "QLDuLich": "Server=YOUR_SERVER;Database=QLDuLich;User ID=sa;Password=YOUR_PASSWORD;..."
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
| guide1 | 123 | Guide | Hướng dẫn viên |
| user1 | 123 | Customer | Khách hàng |

## 📂 Cấu trúc thư mục

```
QLDuLichRBAC_Final/
├── Controllers/           # Controllers (Admin, Customer, Guide, Account)
├── Models/               
│   ├── Entities/         # Database entities
│   └── ViewModels/       # View models
├── Views/
│   ├── Admin/            # Admin views
│   ├── Customer/         # Customer views
│   ├── Guide/            # Guide views
│   ├── Account/          # Login/Register views
│   └── Shared/           # Layout views
├── Services/             # Business logic services
├── wwwroot/
│   ├── css/              # Stylesheets
│   ├── js/               # JavaScript files
│   └── images/           # Images và album tour
├── lib/                  # SQL scripts
│   └── QLDuLichRBAC_Final.sql  # Main database script
├── appsettings.json      # Configuration
├── Program.cs            # Application entry point
├── README.md             # Tài liệu này
└── CHANGES_SUMMARY.md    # Tổng hợp các thay đổi
```

## 🛠️ Công nghệ sử dụng

- **Backend**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server 2022
- **ORM**: Entity Framework Core
- **Frontend**: HTML, CSS, JavaScript, Bootstrap
- **Payment**: VietQR API
- **Authentication**: ASP.NET Core Identity (custom)

## 📝 Các tính năng đã cập nhật

### Phiên bản mới nhất:
- ✅ **Lọc tour thông minh**: Tour đã đăng ký và quá hạn sẽ không hiển thị
- ✅ **Sửa lỗi encoding**: Tất cả tiếng Việt hiển thị chính xác
- ✅ **Giá tour hợp lý**: Giá đã được điều chỉnh (3.500đ - 6.500đ)
- ✅ **UI/UX cải thiện**: Giao diện đẹp hơn, dễ sử dụng hơn
- ✅ **Thanh toán QR**: Tích hợp VietQR cho thanh toán nhanh

## 📖 Hướng dẫn sử dụng

### Đăng nhập Customer:
1. Truy cập: `http://localhost:7180`
2. Đăng nhập với `user1 / 123`
3. Xem danh sách tour
4. Chọn tour và đặt vé
5. Thanh toán qua QR code hoặc tiền mặt

### Xem tour đã thanh toán:
1. Sau khi đăng nhập
2. Click menu "Tour đã thanh toán"
3. Xem chi tiết booking

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

---

**Phát triển bởi**: Nhóm Phân tích Thiết kế Hệ thống
**Năm**: 2025


-- =====================================================
-- HỆ THỐNG QUẢN LÝ ĐẶT VÉ KHU VUI CHƠI JUMPARENA
-- Version: 5.0 - Complete All-in-One Edition
-- Tương thích: SQL Server 2019+
-- Ngày cập nhật: 2025-11-18
-- Mô tả: File SQL hoàn chỉnh - chỉ cần Execute 1 lần
-- =====================================================

SET NOCOUNT ON;
GO

PRINT N'=====================================================';
PRINT N'BẮT ĐẦU CÀI ĐẶT HỆ THỐNG QLJumparena2';
PRINT N'=====================================================';
PRINT N'';

-- =====================================================
-- BƯỚC 1: XÓA VÀ TẠO DATABASE MỚI
-- =====================================================

-- Xóa database cũ nếu tồn tại
IF DB_ID('QLJumparena2') IS NOT NULL
BEGIN
    ALTER DATABASE QLJumparena2 SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QLJumparena2;
    PRINT N'✓ Đã xóa database cũ';
END

-- Tạo database mới với collation tiếng Việt
CREATE DATABASE QLJumparena2
COLLATE Vietnamese_100_CI_AS;
GO

USE QLJumparena2;
GO

PRINT N'✓ Đã tạo database QLJumparena2';
PRINT N'';

-- =====================================================
-- BƯỚC 2: TẠO CẤU TRÚC BẢNG
-- =====================================================

PRINT N'Đang tạo cấu trúc bảng...';

-- Bảng Users (Phân quyền)
CREATE TABLE [User] (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(128) NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NULL,
    Phone NVARCHAR(20) NULL,
    Role NVARCHAR(50) CHECK (Role IN (N'Admin', N'Staff', N'Customer')) NOT NULL
);

-- Bảng Khách hàng
CREATE TABLE KhachHang (
    MaKH INT IDENTITY(1,1) PRIMARY KEY,
    TenKH NVARCHAR(100) NOT NULL,
    GioiTinh NVARCHAR(10) CHECK (GioiTinh IN (N'Nam', N'Nữ', N'Khác')) NOT NULL,
    SDT VARCHAR(15) NOT NULL,
    Email NVARCHAR(150) NULL,
    DiaChi NVARCHAR(255),
    CCCD NVARCHAR(20) NULL,
    NgaySinh DATE NULL,
    AnhDaiDien NVARCHAR(500) NULL,
    UserId INT NULL FOREIGN KEY REFERENCES [User](UserId) ON DELETE SET NULL
);

-- Bảng Nhân viên
CREATE TABLE NhanVien (
    MaNV INT IDENTITY(1,1) PRIMARY KEY,
    TenNV NVARCHAR(100) NOT NULL,
    HoTen NVARCHAR(150) NULL,
    Email NVARCHAR(150) NULL,
    GioiTinh NVARCHAR(10) CHECK (GioiTinh IN (N'Nam', N'Nữ')) NOT NULL,
    NgaySinh DATE NULL,
    SDT VARCHAR(15),
    DiaChi NVARCHAR(255),
    VaiTro NVARCHAR(50) CHECK (VaiTro IN (N'Nhân viên hỗ trợ', N'Nhân viên an toàn', N'Quản lý ca')) NOT NULL,
    NgayVaoLam DATE DEFAULT GETDATE(),
    TrangThai NVARCHAR(20) CHECK (TrangThai IN (N'Đang làm việc', N'Nghỉ phép', N'Đã nghỉ')) DEFAULT N'Đang làm việc',
    AnhDaiDien NVARCHAR(500) NULL,
    UserId INT NULL FOREIGN KEY REFERENCES [User](UserId) ON DELETE SET NULL
);

-- Bảng Khu vui chơi
CREATE TABLE KhuVuiChoi (
    MaKhu INT IDENTITY(1,1) PRIMARY KEY,
    TenKhu NVARCHAR(150) NOT NULL,
    MoTa NVARCHAR(MAX) NULL,
    DiaChi NVARCHAR(255) NULL,
    GioMoCua TIME NOT NULL DEFAULT '08:00:00',
    GioDongCua TIME NOT NULL DEFAULT '22:00:00',
    TrangThai NVARCHAR(20) CHECK (TrangThai IN (N'Hoạt động', N'Bảo trì', N'Đóng cửa')) DEFAULT N'Hoạt động'
);

-- Bảng Trò chơi
CREATE TABLE TroChoi (
    MaTroChoi INT IDENTITY(1,1) PRIMARY KEY,
    TenTroChoi NVARCHAR(100) NOT NULL,
    DanhMuc NVARCHAR(100) NULL,
    MoTa NVARCHAR(MAX) NULL,
    DoKho DECIMAL(3,1) NULL CHECK (DoKho BETWEEN 1 AND 5),
    DoTuoiToiThieu INT NULL CHECK (DoTuoiToiThieu >= 0),
    DoTuoiToiDa INT NULL,
    YeuCauAnToan NVARCHAR(MAX) NULL,
    TrangThai NVARCHAR(20) CHECK (TrangThai IN (N'Hoạt động', N'Bảo trì', N'Tạm đóng')) DEFAULT N'Hoạt động'
);

-- Bảng Gói dịch vụ
CREATE TABLE GoiDichVu (
    MaGoi INT IDENTITY(1,1) PRIMARY KEY,
    TenGoi NVARCHAR(150) NOT NULL,
    LoaiGoi NVARCHAR(50) CHECK (LoaiGoi IN (N'Theo thời gian (phút)', N'Theo số lượt chơi')) NOT NULL,
    MoTa NVARCHAR(MAX) NULL,
    Gia DECIMAL(18,2) NOT NULL CHECK (Gia >= 0),
    ThoiGian INT NULL,
    SoLuotChoi INT NULL,
    TrangThai NVARCHAR(20) CHECK (TrangThai IN (N'Đang bán', N'Hết hạn', N'Tạm ngưng')) DEFAULT N'Đang bán'
);

-- Bảng Ca
CREATE TABLE Ca (
    MaCa INT IDENTITY(1,1) PRIMARY KEY,
    TenCa NVARCHAR(50) NOT NULL,
    GioBatDau TIME NOT NULL,
    GioKetThuc TIME NOT NULL,
    SoLuongToiDa INT NULL CHECK (SoLuongToiDa > 0),
    TrangThai NVARCHAR(20) CHECK (TrangThai IN (N'Hoạt động', N'Tạm ngưng')) DEFAULT N'Hoạt động'
);

-- Bảng Dịch vụ thêm
CREATE TABLE DichVuThem (
    MaDV INT IDENTITY(1,1) PRIMARY KEY,
    TenDV NVARCHAR(150) NOT NULL,
    LoaiDV NVARCHAR(50) CHECK (LoaiDV IN (N'Đồ ăn', N'Đồ uống', N'Phụ kiện', N'Dịch vụ khác')) NOT NULL,
    MoTa NVARCHAR(MAX) NULL,
    Gia DECIMAL(18,2) NOT NULL CHECK (Gia >= 0),
    TrangThai NVARCHAR(20) CHECK (TrangThai IN (N'Còn hàng', N'Hết hàng', N'Ngưng bán')) DEFAULT N'Còn hàng'
);

-- Bảng Hóa đơn
CREATE TABLE HoaDon (
    MaHD INT IDENTITY(1,1) PRIMARY KEY,
    MaKH INT NOT NULL FOREIGN KEY REFERENCES KhachHang(MaKH) ON DELETE NO ACTION,
    NgayTao DATETIME DEFAULT GETDATE(),
    TongTien DECIMAL(18,2) NOT NULL CHECK (TongTien >= 0),
    HinhThucTT NVARCHAR(50) CHECK (HinhThucTT IN (N'Tiền mặt', N'Chuyển khoản', N'QR Code', N'Thẻ')) NOT NULL,
    TrangThaiTT NVARCHAR(20) CHECK (TrangThaiTT IN (N'Chờ thanh toán', N'Đã thanh toán', N'Đã hủy')) DEFAULT N'Chờ thanh toán',
    MaGiaoDich NVARCHAR(100) NULL
);

-- Bảng Vé
CREATE TABLE Ve (
    MaVe INT IDENTITY(1,1) PRIMARY KEY,
    MaVeCode NVARCHAR(50) UNIQUE NOT NULL,
    MaKH INT NOT NULL FOREIGN KEY REFERENCES KhachHang(MaKH) ON DELETE NO ACTION,
    MaGoi INT NOT NULL FOREIGN KEY REFERENCES GoiDichVu(MaGoi) ON DELETE NO ACTION,
    MaCa INT NULL FOREIGN KEY REFERENCES Ca(MaCa) ON DELETE NO ACTION,
    MaHD INT NULL FOREIGN KEY REFERENCES HoaDon(MaHD) ON DELETE NO ACTION,
    NgayDat DATETIME DEFAULT GETDATE(),
    NgaySuDung DATE NOT NULL,
    SoNguoi INT NOT NULL CHECK (SoNguoi > 0),
    TongTien DECIMAL(18,2) NOT NULL CHECK (TongTien >= 0),
    TrangThai NVARCHAR(20) CHECK (TrangThai IN (N'Đã đặt', N'Đã check-in', N'Đã sử dụng', N'Đã hủy')) DEFAULT N'Đã đặt',
    NgayCheckIn DATETIME NULL,
    GhiChu NVARCHAR(MAX) NULL
);

-- Bảng trung gian
CREATE TABLE KhuVuiChoi_TroChoi (
    MaKhu INT NOT NULL,
    MaTroChoi INT NOT NULL,
    PRIMARY KEY (MaKhu, MaTroChoi),
    FOREIGN KEY (MaKhu) REFERENCES KhuVuiChoi(MaKhu) ON DELETE CASCADE,
    FOREIGN KEY (MaTroChoi) REFERENCES TroChoi(MaTroChoi) ON DELETE CASCADE
);

CREATE TABLE GoiDichVu_TroChoi (
    MaGoi INT NOT NULL,
    MaTroChoi INT NOT NULL,
    PRIMARY KEY (MaGoi, MaTroChoi),
    FOREIGN KEY (MaGoi) REFERENCES GoiDichVu(MaGoi) ON DELETE CASCADE,
    FOREIGN KEY (MaTroChoi) REFERENCES TroChoi(MaTroChoi) ON DELETE CASCADE
);

CREATE TABLE Ve_DichVuThem (
    MaVe INT NOT NULL,
    MaDV INT NOT NULL,
    SoLuong INT NOT NULL DEFAULT 1 CHECK (SoLuong > 0),
    PRIMARY KEY (MaVe, MaDV),
    FOREIGN KEY (MaVe) REFERENCES Ve(MaVe) ON DELETE CASCADE,
    FOREIGN KEY (MaDV) REFERENCES DichVuThem(MaDV) ON DELETE CASCADE
);

-- Bảng NhanVien_Ca với attendance tracking
CREATE TABLE NhanVien_Ca (
    MaNV INT NOT NULL,
    MaCa INT NOT NULL,
    NgayLamViec DATE NOT NULL,
    ThoiGianCheckIn DATETIME NULL,
    ThoiGianCheckOut DATETIME NULL,
    TrangThaiDiemDanh NVARCHAR(50) CHECK (TrangThaiDiemDanh IN (N'Chưa điểm danh', N'Đang trực', N'Đã hoàn thành', N'Đi muộn', N'Về sớm')) DEFAULT N'Chưa điểm danh',
    DiMuonCoPhep BIT DEFAULT 0,
    GhiChu NVARCHAR(500) NULL,
    PRIMARY KEY (MaNV, MaCa, NgayLamViec),
    FOREIGN KEY (MaNV) REFERENCES NhanVien(MaNV) ON DELETE CASCADE,
    FOREIGN KEY (MaCa) REFERENCES Ca(MaCa) ON DELETE CASCADE
);

-- Bảng Đăng ký lịch trực
CREATE TABLE DangKyLichTruc (
    MaDangKy INT IDENTITY(1,1) PRIMARY KEY,
    MaNV INT NOT NULL,
    MaCa INT NOT NULL,
    NgayDangKy DATE NOT NULL,
    NgayTruc DATE NOT NULL,
    ThoiGianDangKy DATETIME DEFAULT GETDATE(),
    TrangThai NVARCHAR(50) CHECK (TrangThai IN (N'Chờ duyệt', N'Đã duyệt', N'Từ chối')) DEFAULT N'Chờ duyệt',
    GhiChu NVARCHAR(500) NULL,
    FOREIGN KEY (MaNV) REFERENCES NhanVien(MaNV) ON DELETE CASCADE,
    FOREIGN KEY (MaCa) REFERENCES Ca(MaCa) ON DELETE CASCADE
);

CREATE TABLE KhuVuiChoi_Anh (
    MaKhu INT NOT NULL,
    DuongDanAnh NVARCHAR(500) NOT NULL,
    ThuTu INT NULL,
    PRIMARY KEY (MaKhu, DuongDanAnh),
    FOREIGN KEY (MaKhu) REFERENCES KhuVuiChoi(MaKhu) ON DELETE CASCADE
);

CREATE TABLE TroChoi_Anh (
    MaTroChoi INT NOT NULL,
    DuongDanAnh NVARCHAR(500) NOT NULL,
    ThuTu INT NULL,
    PRIMARY KEY (MaTroChoi, DuongDanAnh),
    FOREIGN KEY (MaTroChoi) REFERENCES TroChoi(MaTroChoi) ON DELETE CASCADE
);

GO
PRINT N'✓ Đã tạo xong cấu trúc bảng';
PRINT N'';

-- =====================================================
-- BƯỚC 3: INSERT DỮ LIỆU MẪU
-- =====================================================

PRINT N'Đang thêm dữ liệu mẫu...';

-- Insert Users (Password: 123456789 - SHA256)
SET IDENTITY_INSERT [User] ON;
INSERT INTO [User] (UserId, Username, PasswordHash, FullName, Email, Phone, Role) VALUES
(1, 'admin', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', N'Administrator', 'admin@jumparena.vn', '0901234567', N'Admin'),
(2, 'staff1', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', N'Nguyễn Văn A', 'staff1@jumparena.vn', '0901234568', N'Staff'),
(3, 'staff2', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', N'Trần Thảo B', 'staff2@jumparena.vn', '0901234569', N'Staff'),
(4, 'customer1', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', N'Võ Văn Khách', 'customer1@gmail.com', '0987654321', N'Customer'),
(5, 'customer2', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', N'Nguyễn Thị Lan', 'customer2@gmail.com', '0987654322', N'Customer'),
(6, 'customer3', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', N'Trần Văn Minh', 'customer3@gmail.com', '0987654323', N'Customer');
SET IDENTITY_INSERT [User] OFF;

-- Insert Khách hàng
SET IDENTITY_INSERT KhachHang ON;
INSERT INTO KhachHang (MaKH, TenKH, GioiTinh, SDT, Email, DiaChi, CCCD, NgaySinh, UserId) VALUES
(1, N'Võ Văn Khách', N'Nam', '0987654321', 'customer1@gmail.com', N'123 Lê Lợi, Q.1, TP.HCM', '079012345678', '1990-05-15', 4),
(2, N'Nguyễn Thị Lan', N'Nữ', '0987654322', 'customer2@gmail.com', N'456 Nguyễn Huệ, Q.1, TP.HCM', '079087654321', '1985-08-20', 5),
(3, N'Trần Văn Minh', N'Nam', '0987654323', 'customer3@gmail.com', N'789 Hai Bà Trưng, Q.3, TP.HCM', '079123456789', '1992-12-10', 6),
(4, N'Phạm Thị Dung', N'Nữ', '0934567890', 'phamthid@gmail.com', N'321 Võ Văn Tần, Q3, TP.HCM', NULL, '1997-12-25', NULL),
(5, N'Hoàng Văn Em', N'Nam', '0945678901', 'hoangvane@gmail.com', N'654 Điện Biên Phủ, Q10, TP.HCM', NULL, '1990-07-18', NULL),
(6, N'Võ Thị Phượng', N'Nữ', '0956789012', 'vothiphuong@gmail.com', N'987 Cách Mạng Tháng 8, Q3, TP.HCM', NULL, '1996-02-14', NULL),
(7, N'Đặng Văn Giang', N'Nam', '0967890123', 'dangvang@gmail.com', N'147 Lý Thường Kiệt, Q10, TP.HCM', NULL, '1993-09-05', NULL),
(8, N'Bùi Thị Hà', N'Nữ', '0978901234', 'buithiha@gmail.com', N'258 Hai Bà Trưng, Q1, TP.HCM', NULL, '1999-11-30', NULL),
(9, N'Ngô Văn Inh', N'Nam', '0989012345', 'ngovani@gmail.com', N'369 Phan Đình Phùng, Q5, TP.HCM', NULL, '1991-04-22', NULL),
(10, N'Mai Thị Kim', N'Nữ', '0990123456', 'maithikim@gmail.com', N'741 Nguyễn Thị Minh Khai, Q3, TP.HCM', NULL, '1994-06-08', NULL);
SET IDENTITY_INSERT KhachHang OFF;

-- Insert Nhân viên
SET IDENTITY_INSERT NhanVien ON;
INSERT INTO NhanVien (MaNV, TenNV, HoTen, Email, GioiTinh, NgaySinh, SDT, DiaChi, VaiTro, NgayVaoLam, UserId) VALUES
(1, N'NV001', N'Nguyễn Văn A', 'staff1@jumparena.vn', N'Nam', '1995-03-15', '0901234568', N'12 Võ Văn Tần, Q.3, TP.HCM', N'Nhân viên hỗ trợ', '2024-01-15', 2),
(2, N'NV002', N'Trần Thảo B', 'staff2@jumparena.vn', N'Nữ', '1998-07-22', '0901234569', N'45 Lý Tự Trọng, Q.1, TP.HCM', N'Nhân viên an toàn', '2024-02-01', 3);
SET IDENTITY_INSERT NhanVien OFF;

-- Insert Khu vui chơi
SET IDENTITY_INSERT KhuVuiChoi ON;
INSERT INTO KhuVuiChoi (MaKhu, TenKhu, MoTa, DiaChi, GioMoCua, GioDongCua, TrangThai) VALUES
(1, N'Adventure Zone', N'Khu vực thử thách mạo hiểm dành cho người lớn', N'Tầng 3, TTTM Vincom Center, Q.1, TP.HCM', '08:00:00', '22:00:00', N'Hoạt động'),
(2, N'Kids Zone', N'Khu vui chơi an toàn cho trẻ em', N'Tầng 2, TTTM Vincom Center, Q.1, TP.HCM', '08:00:00', '22:00:00', N'Hoạt động'),
(3, N'Extreme Zone', N'Khu vực thử thách cực hạn', N'Tầng 4, TTTM Vincom Center, Q.1, TP.HCM', '09:00:00', '21:00:00', N'Hoạt động');
SET IDENTITY_INSERT KhuVuiChoi OFF;

-- Insert Trò chơi
SET IDENTITY_INSERT TroChoi ON;
INSERT INTO TroChoi (MaTroChoi, TenTroChoi, DanhMuc, MoTa, DoKho, DoTuoiToiThieu, DoTuoiToiDa, YeuCauAnToan, TrangThai) VALUES
(1, N'Trampoline cơ bản', N'Trampoline', N'Bạt nhún cơ bản cho người mới', 2.0, 5, NULL, N'Đi tất chống trượt', N'Hoạt động'),
(2, N'Ninja Warrior', N'Thử thách', N'Vượt chướng ngại vật như ninja', 4.5, 12, NULL, N'Có hướng dẫn viên', N'Hoạt động'),
(3, N'Ball Pool', N'Thể thao', N'Bể bóng khổng lồ', 1.5, 3, 12, N'Không nhảy từ trên cao', N'Hoạt động'),
(4, N'Slam Dunk', N'Thể thao', N'Bóng rổ với bạt nhún', 3.0, 8, NULL, N'Đi tất chống trượt', N'Hoạt động'),
(5, N'Foam Pit', N'Thể thao', N'Hố xốp luyện nhào lộn', 3.5, 10, NULL, N'Có huấn luyện viên', N'Hoạt động'),
(6, N'Dodgeball Arena', N'Thể thao', N'Đấu bóng né', 2.5, 8, NULL, N'Không ném vào đầu', N'Hoạt động'),
(7, N'Sky Walk', N'Thử thách', N'Đi trên dây ở độ cao 5m', 4.0, 14, NULL, N'Đeo dây an toàn', N'Hoạt động'),
(8, N'Climbing Wall', N'Thử thách', N'Tường leo núi', 3.5, 10, NULL, N'Đeo dây an toàn', N'Hoạt động');
SET IDENTITY_INSERT TroChoi OFF;

-- Insert Gói dịch vụ (12 gói với tiếng Việt chuẩn)
SET IDENTITY_INSERT GoiDichVu ON;
INSERT INTO GoiDichVu (MaGoi, TenGoi, LoaiGoi, MoTa, Gia, ThoiGian, SoLuotChoi, TrangThai) VALUES
(1, N'Gói Cơ Bản 1 Giờ', N'Theo thời gian (phút)', N'Chơi tự do trong 1 giờ, phù hợp cho người mới', 100000, 60, NULL, N'Đang bán'),
(2, N'Gói Tiêu Chuẩn 2 Giờ', N'Theo thời gian (phút)', N'Trải nghiệm đầy đủ trong 2 giờ', 180000, 120, NULL, N'Đang bán'),
(3, N'Gói VIP 3 Giờ', N'Theo thời gian (phút)', N'Tận hưởng trọn vẹn 3 giờ với nhiều ưu đãi', 250000, 180, NULL, N'Đang bán'),
(4, N'Gói Premium Cả Ngày', N'Theo thời gian (phút)', N'Vui chơi cả ngày không giới hạn', 350000, 480, NULL, N'Đang bán'),
(5, N'Gói Gia Đình 4 Người', N'Theo thời gian (phút)', N'Dành cho gia đình 4 người, 2 giờ chơi', 600000, 120, NULL, N'Đang bán'),
(6, N'Gói Nhóm 10 Người', N'Theo thời gian (phút)', N'Team building, nhóm bạn 10 người', 1500000, 180, NULL, N'Đang bán'),
(7, N'Gói Sinh Nhật Trẻ Em', N'Theo thời gian (phút)', N'Tổ chức sinh nhật hoàn hảo cho bé', 2000000, 240, NULL, N'Đang bán'),
(8, N'Gói 5 Lượt Chơi', N'Theo số lượt chơi', N'5 lượt chơi các trò yêu thích', 120000, NULL, 5, N'Đang bán'),
(9, N'Gói 10 Lượt Chơi', N'Theo số lượt chơi', N'10 lượt chơi tiết kiệm hơn', 200000, NULL, 10, N'Đang bán'),
(10, N'Gói 20 Lượt Chơi VIP', N'Theo số lượt chơi', N'20 lượt chơi cao cấp với ưu đãi lớn', 350000, NULL, 20, N'Đang bán'),
(11, N'Gói Học Sinh Sinh Viên', N'Theo thời gian (phút)', N'Ưu đãi đặc biệt cho học sinh sinh viên', 80000, 90, NULL, N'Đang bán'),
(12, N'Gói Cuối Tuần Đặc Biệt', N'Theo thời gian (phút)', N'Chương trình cuối tuần với nhiều quà tặng', 220000, 150, NULL, N'Đang bán');
SET IDENTITY_INSERT GoiDichVu OFF;

-- Insert Ca
SET IDENTITY_INSERT Ca ON;
INSERT INTO Ca (MaCa, TenCa, GioBatDau, GioKetThuc, SoLuongToiDa, TrangThai) VALUES
(1, N'Ca 1 - Sáng sớm', '08:00:00', '10:00:00', 50, N'Hoạt động'),
(2, N'Ca 2 - Sáng', '10:00:00', '12:00:00', 50, N'Hoạt động'),
(3, N'Ca 3 - Trưa', '12:00:00', '14:00:00', 50, N'Hoạt động'),
(4, N'Ca 4 - Chiều', '14:00:00', '16:00:00', 60, N'Hoạt động'),
(5, N'Ca 5 - Chiều muộn', '16:00:00', '18:00:00', 60, N'Hoạt động'),
(6, N'Ca 6 - Tối', '18:00:00', '20:00:00', 70, N'Hoạt động'),
(7, N'Ca 7 - Tối muộn', '20:00:00', '22:00:00', 70, N'Hoạt động');
SET IDENTITY_INSERT Ca OFF;

-- Insert Dịch vụ thêm
SET IDENTITY_INSERT DichVuThem ON;
INSERT INTO DichVuThem (MaDV, TenDV, LoaiDV, MoTa, Gia, TrangThai) VALUES
(1, N'Nước suối', N'Đồ uống', N'Nước suối Aquafina 500ml', 10000, N'Còn hàng'),
(2, N'Nước ngọt', N'Đồ uống', N'Coca/Pepsi 330ml', 15000, N'Còn hàng'),
(3, N'Tất chống trượt', N'Phụ kiện', N'Tất chống trượt bắt buộc', 20000, N'Còn hàng'),
(4, N'Băng bảo vệ', N'Phụ kiện', N'Bộ băng bảo vệ đầy đủ', 50000, N'Còn hàng'),
(5, N'Bánh snack', N'Đồ ăn', N'Snack các loại', 15000, N'Còn hàng'),
(6, N'Combo đồ ăn nhẹ', N'Đồ ăn', N'Combo snack + nước', 25000, N'Còn hàng'),
(7, N'Thuê tủ khóa', N'Dịch vụ khác', N'Tủ khóa cá nhân', 30000, N'Còn hàng'),
(8, N'Chụp ảnh lưu niệm', N'Dịch vụ khác', N'Chụp ảnh chuyên nghiệp', 200000, N'Còn hàng');
SET IDENTITY_INSERT DichVuThem OFF;

-- Insert junction tables
INSERT INTO KhuVuiChoi_TroChoi (MaKhu, MaTroChoi) VALUES
(1, 1), (1, 2), (1, 4), (1, 6),
(2, 1), (2, 3), (2, 4), (2, 6),
(3, 2), (3, 5), (3, 7), (3, 8);

INSERT INTO GoiDichVu_TroChoi (MaGoi, MaTroChoi) VALUES
(1, 1), (1, 3), (1, 4),
(2, 1), (2, 2), (2, 3), (2, 4), (2, 6),
(3, 1), (3, 2), (3, 4), (3, 5), (3, 6), (3, 7), (3, 8);

-- Insert Hóa đơn mẫu
SET IDENTITY_INSERT HoaDon ON;
INSERT INTO HoaDon (MaHD, MaKH, NgayTao, TongTien, HinhThucTT, TrangThaiTT, MaGiaoDich) VALUES
(1, 1, '2025-11-10 10:30:00', 250000, N'QR Code', N'Đã thanh toán', 'JPA000100100001'),
(2, 2, '2025-11-12 14:20:00', 800000, N'Chuyển khoản', N'Đã thanh toán', 'JPA000200600002'),
(3, 3, '2025-11-15 09:15:00', 3500000, N'Chuyển khoản', N'Đã thanh toán', 'JPA000300300003'),
(4, 4, '2025-11-01', 150000, N'Tiền mặt', N'Đã thanh toán', 'JPA000400100004'),
(5, 5, '2025-11-05', 450000, N'Thẻ', N'Đã thanh toán', 'JPA000500200005'),
(6, 6, '2025-10-20', 300000, N'Tiền mặt', N'Đã thanh toán', 'JPA000600800006'),
(7, 7, '2025-10-15', 200000, N'Chuyển khoản', N'Đã thanh toán', 'JPA000700900007'),
(8, 8, '2025-09-25', 120000, N'QR Code', N'Đã thanh toán', 'JPA000801100008'),
(9, 9, '2025-09-10', 180000, N'Tiền mặt', N'Đã thanh toán', 'JPA000900200009'),
(10, 10, '2025-08-28', 250000, N'Thẻ', N'Đã thanh toán', 'JPA001000300010'),
(11, 1, '2025-08-15', 600000, N'Chuyển khoản', N'Đã thanh toán', 'JPA000100500011'),
(12, 2, '2025-07-20', 1500000, N'Thẻ', N'Đã thanh toán', 'JPA000200600012'),
(13, 3, '2025-07-05', 100000, N'Tiền mặt', N'Đã thanh toán', 'JPA000300100013'),
(14, 4, '2025-06-25', 220000, N'QR Code', N'Đã thanh toán', 'JPA000401200014'),
(15, 5, '2025-06-10', 350000, N'Chuyển khoản', N'Đã thanh toán', 'JPA000501000015');
SET IDENTITY_INSERT HoaDon OFF;

-- Insert Vé mẫu (39 vé)
SET IDENTITY_INSERT Ve ON;
INSERT INTO Ve (MaVe, MaVeCode, MaKH, MaGoi, MaCa, MaHD, NgayDat, NgaySuDung, SoNguoi, TongTien, TrangThai) VALUES
-- Tháng 11/2025
(1, 'JPA0001001ABC001', 1, 1, 1, 1, '2025-11-10', '2025-11-20', 1, 100000, N'Đã đặt'),
(2, 'JPA0002002ABC002', 2, 2, 2, 2, '2025-11-12', '2025-11-22', 1, 180000, N'Đã đặt'),
(3, 'JPA0003003ABC003', 3, 3, 3, 3, '2025-11-15', '2025-11-25', 1, 250000, N'Đã đặt'),
(4, 'JPA0004001ABC004', 4, 1, 4, 4, '2025-11-01', '2025-11-21', 1, 100000, N'Đã sử dụng'),
(5, 'JPA0005002ABC005', 5, 2, 5, 5, '2025-11-05', '2025-11-15', 1, 180000, N'Đã sử dụng'),
(6, 'JPA0006008ABC006', 6, 8, 1, 6, '2025-11-08', '2025-11-18', 1, 120000, N'Đã đặt'),
(7, 'JPA0007009ABC007', 7, 9, 2, 7, '2025-11-10', '2025-11-20', 1, 200000, N'Đã đặt'),
(8, 'JPA0008011ABC008', 8, 11, 3, 8, '2025-11-12', '2025-11-22', 1, 80000, N'Đã đặt'),
(9, 'JPA0009002ABC009', 9, 2, 4, 9, '2025-11-13', '2025-11-23', 1, 180000, N'Đã đặt'),
(10, 'JPA0010003ABC010', 10, 3, 5, 10, '2025-11-14', '2025-11-24', 1, 250000, N'Đã đặt'),
-- Tháng 10/2025
(11, 'JPA0001005ABC011', 1, 5, 1, 11, '2025-10-20', '2025-10-25', 4, 600000, N'Đã sử dụng'),
(12, 'JPA0002006ABC012', 2, 6, 2, 12, '2025-10-15', '2025-10-20', 10, 1500000, N'Đã sử dụng'),
(13, 'JPA0003001ABC013', 3, 1, 3, 13, '2025-10-10', '2025-10-15', 1, 100000, N'Đã sử dụng'),
(14, 'JPA0004012ABC014', 4, 12, 4, 14, '2025-10-05', '2025-10-10', 1, 220000, N'Đã sử dụng'),
(15, 'JPA0005010ABC015', 5, 10, 5, 15, '2025-10-01', '2025-10-05', 1, 350000, N'Đã sử dụng'),
-- Tháng 9/2025
(16, 'JPA0006002ABC016', 6, 2, 1, 1, '2025-09-25', '2025-09-28', 1, 180000, N'Đã sử dụng'),
(17, 'JPA0007003ABC017', 7, 3, 2, 2, '2025-09-20', '2025-09-23', 1, 250000, N'Đã sử dụng'),
(18, 'JPA0008001ABC018', 8, 1, 3, 3, '2025-09-15', '2025-09-18', 1, 100000, N'Đã sử dụng'),
(19, 'JPA0009011ABC019', 9, 11, 4, 4, '2025-09-10', '2025-09-13', 1, 80000, N'Đã sử dụng'),
(20, 'JPA0010002ABC020', 10, 2, 5, 5, '2025-09-05', '2025-09-08', 1, 180000, N'Đã sử dụng'),
-- Tháng 8/2025
(21, 'JPA0001004ABC021', 1, 4, 1, 6, '2025-08-28', '2025-08-30', 1, 350000, N'Đã sử dụng'),
(22, 'JPA0002001ABC022', 2, 1, 2, 7, '2025-08-25', '2025-08-27', 1, 100000, N'Đã sử dụng'),
(23, 'JPA0003002ABC023', 3, 2, 3, 8, '2025-08-20', '2025-08-22', 1, 180000, N'Đã sử dụng'),
(24, 'JPA0004003ABC024', 4, 3, 4, 9, '2025-08-15', '2025-08-17', 1, 250000, N'Đã sử dụng'),
(25, 'JPA0005005ABC025', 5, 5, 5, 10, '2025-08-10', '2025-08-12', 4, 600000, N'Đã sử dụng'),
-- Tháng 7/2025
(26, 'JPA0006001ABC026', 6, 1, 1, 11, '2025-07-25', '2025-07-27', 1, 100000, N'Đã sử dụng'),
(27, 'JPA0007002ABC027', 7, 2, 2, 12, '2025-07-20', '2025-07-22', 1, 180000, N'Đã sử dụng'),
(28, 'JPA0008003ABC028', 8, 3, 3, 13, '2025-07-15', '2025-07-17', 1, 250000, N'Đã sử dụng'),
(29, 'JPA0009001ABC029', 9, 1, 4, 14, '2025-07-10', '2025-07-12', 1, 100000, N'Đã sử dụng'),
(30, 'JPA0010002ABC030', 10, 2, 5, 15, '2025-07-05', '2025-07-07', 1, 180000, N'Đã sử dụng'),
-- Tháng 6/2025
(31, 'JPA0001001ABC031', 1, 1, 1, 1, '2025-06-25', '2025-06-27', 1, 100000, N'Đã sử dụng'),
(32, 'JPA0002002ABC032', 2, 2, 2, 2, '2025-06-20', '2025-06-22', 1, 180000, N'Đã sử dụng'),
(33, 'JPA0003003ABC033', 3, 3, 3, 3, '2025-06-15', '2025-06-17', 1, 250000, N'Đã sử dụng'),
(34, 'JPA0004001ABC034', 4, 1, 4, 4, '2025-06-10', '2025-06-12', 1, 100000, N'Đã sử dụng'),
(35, 'JPA0005002ABC035', 5, 2, 5, 5, '2025-06-05', '2025-06-07', 1, 180000, N'Đã sử dụng'),
(36, 'JPA0006012ABC036', 6, 12, 1, 6, '2025-06-03', '2025-06-05', 1, 220000, N'Đã sử dụng'),
(37, 'JPA0007011ABC037', 7, 11, 2, 7, '2025-06-02', '2025-06-04', 1, 80000, N'Đã sử dụng'),
(38, 'JPA0008010ABC038', 8, 10, 3, 8, '2025-06-01', '2025-06-03', 1, 350000, N'Đã sử dụng'),
(39, 'JPA0009009ABC039', 9, 9, 4, 9, '2025-06-01', '2025-06-02', 1, 200000, N'Đã sử dụng');
SET IDENTITY_INSERT Ve OFF;

-- Insert Ve_DichVuThem
INSERT INTO Ve_DichVuThem (MaVe, MaDV, SoLuong) VALUES
(1, 3, 1), (1, 1, 1),
(2, 3, 1), (2, 6, 1),
(3, 3, 1), (3, 2, 2);

-- Insert NhanVien_Ca (lịch làm việc 30 ngày gần đây)
DECLARE @StartDate DATE = DATEADD(DAY, -30, CAST(GETDATE() AS DATE));
DECLARE @EndDate DATE = CAST(GETDATE() AS DATE);
DECLARE @CurrentDate DATE = @StartDate;

WHILE @CurrentDate <= @EndDate
BEGIN
    -- NV1 - Ca 1 (Sáng)
    INSERT INTO NhanVien_Ca (MaNV, MaCa, NgayLamViec, TrangThaiDiemDanh)
    VALUES (1, 1, @CurrentDate, CASE WHEN @CurrentDate < CAST(GETDATE() AS DATE) THEN N'Đã hoàn thành' ELSE N'Chưa điểm danh' END);
    
    -- NV2 - Ca 2 (Sáng)
    INSERT INTO NhanVien_Ca (MaNV, MaCa, NgayLamViec, TrangThaiDiemDanh)
    VALUES (2, 2, @CurrentDate, CASE WHEN @CurrentDate < CAST(GETDATE() AS DATE) THEN N'Đã hoàn thành' ELSE N'Chưa điểm danh' END);
    
    SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);
END

GO
PRINT N'✓ Đã thêm xong dữ liệu mẫu';
PRINT N'  - 6 Users (1 Admin, 2 Staff, 3 Customer)';
PRINT N'  - 10 Khách hàng';
PRINT N'  - 2 Nhân viên';
PRINT N'  - 3 Khu vui chơi';
PRINT N'  - 8 Trò chơi';
PRINT N'  - 12 Gói dịch vụ';
PRINT N'  - 7 Ca làm việc';
PRINT N'  - 8 Dịch vụ thêm';
PRINT N'  - 15 Hóa đơn';
PRINT N'  - 39 Vé';
PRINT N'  - 60 Ca làm việc (30 ngày × 2 nhân viên)';
PRINT N'';

-- =====================================================
-- BƯỚC 4: TẠO STORED PROCEDURES
-- =====================================================

PRINT N'Đang tạo stored procedures...';

GO
CREATE OR ALTER PROCEDURE sp_DatVeJumparena
    @MaKH INT,
    @MaGoi INT,
    @MaCa INT,
    @NgaySuDung DATE,
    @SoNguoi INT,
    @DanhSachDV NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TongTien DECIMAL(18,2) = 0;
    DECLARE @GiaGoi DECIMAL(18,2);
    DECLARE @MaHD INT;
    DECLARE @MaVe INT;
    DECLARE @MaVeCode NVARCHAR(50);
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        SELECT @GiaGoi = Gia FROM GoiDichVu WHERE MaGoi = @MaGoi;
        SET @TongTien = @GiaGoi * @SoNguoi;
        
        INSERT INTO HoaDon (MaKH, TongTien, HinhThucTT, TrangThaiTT)
        VALUES (@MaKH, @TongTien, N'QR Code', N'Chờ thanh toán');
        
        SET @MaHD = SCOPE_IDENTITY();
        SET @MaVeCode = 'JPA' + RIGHT('0000' + CAST(@MaKH AS VARCHAR), 4) + 
                        RIGHT('000' + CAST(@MaGoi AS VARCHAR), 3) + 
                        SUBSTRING(CONVERT(VARCHAR(36), NEWID()), 1, 8);
        
        INSERT INTO Ve (MaVeCode, MaKH, MaGoi, MaCa, MaHD, NgaySuDung, SoNguoi, TongTien)
        VALUES (@MaVeCode, @MaKH, @MaGoi, @MaCa, @MaHD, @NgaySuDung, @SoNguoi, @TongTien);
        
        SET @MaVe = SCOPE_IDENTITY();
        
        IF @DanhSachDV IS NOT NULL AND LEN(@DanhSachDV) > 0
        BEGIN
            DECLARE @Item NVARCHAR(100), @Pos INT, @MaDV INT, @SoLuong INT, @GiaDV DECIMAL(18,2), @ColonPos INT;
            
            WHILE LEN(@DanhSachDV) > 0
            BEGIN
                SET @Pos = CHARINDEX(',', @DanhSachDV);
                IF @Pos > 0
                BEGIN
                    SET @Item = SUBSTRING(@DanhSachDV, 1, @Pos - 1);
                    SET @DanhSachDV = SUBSTRING(@DanhSachDV, @Pos + 1, LEN(@DanhSachDV));
                END
                ELSE
                BEGIN
                    SET @Item = @DanhSachDV;
                    SET @DanhSachDV = '';
                END
                
                SET @ColonPos = CHARINDEX(':', @Item);
                SET @MaDV = CAST(SUBSTRING(@Item, 1, @ColonPos - 1) AS INT);
                SET @SoLuong = CAST(SUBSTRING(@Item, @ColonPos + 1, LEN(@Item)) AS INT);
                
                INSERT INTO Ve_DichVuThem (MaVe, MaDV, SoLuong)
                VALUES (@MaVe, @MaDV, @SoLuong);
                
                SELECT @GiaDV = Gia FROM DichVuThem WHERE MaDV = @MaDV;
                SET @TongTien = @TongTien + (@GiaDV * @SoLuong);
            END
            
            UPDATE HoaDon SET TongTien = @TongTien WHERE MaHD = @MaHD;
            UPDATE Ve SET TongTien = @TongTien WHERE MaVe = @MaVe;
        END
        
        COMMIT TRANSACTION;
        SELECT @MaVe AS MaVe, @MaVeCode AS MaVeCode, @MaHD AS MaHD, @TongTien AS TongTien;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_CheckInVe
    @MaVeCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        IF NOT EXISTS (SELECT 1 FROM Ve WHERE MaVeCode = @MaVeCode)
            THROW 50001, N'Mã vé không tồn tại', 1;
        
        DECLARE @TrangThai NVARCHAR(20);
        SELECT @TrangThai = TrangThai FROM Ve WHERE MaVeCode = @MaVeCode;
        
        IF @TrangThai = N'Đã check-in' THROW 50002, N'Vé đã được check-in', 1;
        IF @TrangThai = N'Đã sử dụng' THROW 50003, N'Vé đã được sử dụng', 1;
        IF @TrangThai = N'Đã hủy' THROW 50004, N'Vé đã bị hủy', 1;
        
        UPDATE Ve SET TrangThai = N'Đã check-in', NgayCheckIn = GETDATE()
        WHERE MaVeCode = @MaVeCode;
        
        COMMIT TRANSACTION;
        SELECT N'Check-in thành công' AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_ThongKeTheoThang
    @Thang INT,
    @Nam INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        SUM(TongTien) AS TongDoanhThu,
        COUNT(DISTINCT MaHD) AS SoHoaDon,
        COUNT(DISTINCT MaVe) AS SoVe,
        COUNT(DISTINCT MaKH) AS SoKhachHang
    FROM Ve
    WHERE MONTH(NgayDat) = @Thang AND YEAR(NgayDat) = @Nam;
    
    SELECT 
        g.TenGoi, g.LoaiGoi,
        COUNT(v.MaVe) AS SoVe,
        SUM(v.TongTien) AS DoanhThu
    FROM Ve v
    JOIN GoiDichVu g ON v.MaGoi = g.MaGoi
    WHERE MONTH(v.NgayDat) = @Thang AND YEAR(v.NgayDat) = @Nam
    GROUP BY g.TenGoi, g.LoaiGoi
    ORDER BY DoanhThu DESC;
    
    SELECT TOP 10
        c.TenCa, c.GioBatDau, c.GioKetThuc,
        COUNT(v.MaVe) AS SoLuotDat
    FROM Ve v
    JOIN Ca c ON v.MaCa = c.MaCa
    WHERE MONTH(v.NgayDat) = @Thang AND YEAR(v.NgayDat) = @Nam
    GROUP BY c.TenCa, c.GioBatDau, c.GioKetThuc
    ORDER BY SoLuotDat DESC;
END
GO

PRINT N'✓ Đã tạo xong stored procedures';
PRINT N'';

-- =====================================================
-- HOÀN TẤT CÀI ĐẶT
-- =====================================================

PRINT N'=====================================================';
PRINT N'HOÀN TẤT CÀI ĐẶT DATABASE QLJumparena2';
PRINT N'=====================================================';
PRINT N'';
PRINT N'📋 THÔNG TIN ĐĂNG NHẬP:';
PRINT N'';
PRINT N'👤 Admin:';
PRINT N'   Username: admin';
PRINT N'   Password: 123456789';
PRINT N'';
PRINT N'👷 Staff 1:';
PRINT N'   Username: staff1';
PRINT N'   Password: 123456789';
PRINT N'   Tên: Nguyễn Văn A';
PRINT N'';
PRINT N'👷 Staff 2:';
PRINT N'   Username: staff2';
PRINT N'   Password: 123456789';
PRINT N'   Tên: Trần Thảo B';
PRINT N'';
PRINT N'👨 Customer 1:';
PRINT N'   Username: customer1';
PRINT N'   Password: 123456789';
PRINT N'';
PRINT N'📊 THỐNG KÊ DỮ LIỆU:';
SELECT 
    N'Users' AS [Loại dữ liệu], 
    COUNT(*) AS [Số lượng]
FROM [User]
UNION ALL
SELECT N'Khách hàng', COUNT(*) FROM KhachHang
UNION ALL
SELECT N'Nhân viên', COUNT(*) FROM NhanVien
UNION ALL
SELECT N'Gói dịch vụ', COUNT(*) FROM GoiDichVu
UNION ALL
SELECT N'Vé', COUNT(*) FROM Ve
UNION ALL
SELECT N'Hóa đơn', COUNT(*) FROM HoaDon
UNION ALL
SELECT N'Ca làm việc', COUNT(*) FROM NhanVien_Ca;

PRINT N'';
PRINT N'✅ Database đã sẵn sàng sử dụng!';
PRINT N'🎯 Chỉ cần Execute file này 1 lần duy nhất!';
PRINT N'';

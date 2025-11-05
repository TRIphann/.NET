-- =====================================================
-- HỆ THỐNG QUẢN LÝ DU LỊCH - QLDuLich
-- Version: 2.0 - Complete Edition
-- Tương thích: SQL Server 2019+
-- Ngày cập nhật: 2025
-- Mô tả: Database hoàn chỉnh với 10 tour đầy đủ thông tin
-- =====================================================

-- Set encoding UTF-8
SET NOCOUNT ON;
GO

-- Xóa database cũ nếu tồn tại
IF DB_ID('QLDuLich') IS NOT NULL
BEGIN
    ALTER DATABASE QLDuLich SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QLDuLich;
END
GO

CREATE DATABASE QLDuLich
COLLATE Vietnamese_100_CI_AS;
GO
USE QLDuLich;
GO

PRINT N'=====================================================';
PRINT N'BẮT ĐẦU TẠO DATABASE QLDuLich';
PRINT N'=====================================================';

-- =====================================================
-- PHẦN 1: TẠO CẤU TRÚC BẢNG
-- =====================================================

-- Bảng Users (Phân quyền)
CREATE TABLE [User] (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(128) NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NULL,
    Phone NVARCHAR(20) NULL,
    Role NVARCHAR(50) CHECK (Role IN (N'Admin', N'Guide', N'Customer')) NOT NULL
);

-- Bảng Khách hàng
CREATE TABLE KhachHang (
    MaKH INT IDENTITY(1,1) PRIMARY KEY,
    TenKH NVARCHAR(100) NOT NULL,
    GioiTinh NVARCHAR(10) CHECK (GioiTinh IN (N'Nam', N'Nữ')) NOT NULL,
    SDT VARCHAR(15) NOT NULL,
    DiaChi NVARCHAR(255),
    CCCD NVARCHAR(20) NULL,
    AnhDaiDien NVARCHAR(500) NULL,
    UserId INT NULL FOREIGN KEY REFERENCES [User](UserId) ON DELETE SET NULL
);

-- Bảng Nhân viên (Hướng dẫn viên)
CREATE TABLE NhanVien (
    MaNV INT PRIMARY KEY,
    TenNV NVARCHAR(100) NOT NULL,
    GioiTinh NVARCHAR(10) CHECK (GioiTinh IN (N'Nam', N'Nữ')) NOT NULL,
    SDT VARCHAR(15),
    DiaChi NVARCHAR(255),
	VaiTro NVARCHAR(50),
    AnhDaiDien NVARCHAR(500) NULL,
    UserId INT NULL FOREIGN KEY REFERENCES [User](UserId) ON DELETE SET NULL
);

-- Bảng Tour
CREATE TABLE Tour (
    MaTour INT PRIMARY KEY,
    TenTour NVARCHAR(100) NOT NULL,
    NgayBatDau DATE NOT NULL,
    NgayKetThuc DATE NOT NULL,
    Gia DECIMAL(18,2) CHECK (Gia >= 0) NOT NULL,
    AnhDaiDien NVARCHAR(500) NULL,
    MoTa NVARCHAR(MAX) NULL,
    CHECK (NgayKetThuc > NgayBatDau)
);

-- Bảng Ảnh Tour (Nhiều ảnh cho 1 tour)
CREATE TABLE Tour_Anh (
    MaAnh INT IDENTITY(1,1) PRIMARY KEY,
    MaTour INT NOT NULL,
    DuongDan NVARCHAR(255) NOT NULL,
    FOREIGN KEY (MaTour) REFERENCES Tour(MaTour) ON DELETE CASCADE
);

-- Bảng Phương tiện
CREATE TABLE PhuongTien (
    MaPT INT PRIMARY KEY,
    TenPT NVARCHAR(100),
    LoaiPT NVARCHAR(50),
    SoCho INT CHECK (SoCho > 0)
);

-- Bảng Địa điểm
CREATE TABLE DiaDiem (
    MaDD INT PRIMARY KEY,
    TenDD NVARCHAR(100),
    Tinh NVARCHAR(100)
);

-- Bảng Dịch vụ đi kèm
CREATE TABLE DichVuDiKem (
    MaDV INT PRIMARY KEY,
    TenDV NVARCHAR(100),
    Gia DECIMAL(18,2) CHECK (Gia >= 0),
    LoaiDV NVARCHAR(255)
);

-- Bảng Lịch trình chi tiết
CREATE TABLE LichTrinhCT (
    MaLT INT PRIMARY KEY,
    NgayThu INT CHECK (NgayThu > 0),
    NoiDung NVARCHAR(MAX)
);

-- Bảng Hóa đơn
CREATE TABLE HoaDon (
    MaHD INT IDENTITY(1,1) PRIMARY KEY,
    MaKH INT NOT NULL,
    MaTour INT NOT NULL,
    NgayTT DATE DEFAULT GETDATE(),
    HinhThucTT NVARCHAR(50) CHECK (HinhThucTT IN (N'Tiền mặt', N'Chuyển khoản', N'QR Code')) NOT NULL,
    TongTien DECIMAL(18,2) CHECK (TongTien >= 0),
    TrangThai NVARCHAR(50) DEFAULT N'Đã thanh toán',
    FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH),
    FOREIGN KEY (MaTour) REFERENCES Tour(MaTour)
);

-- ========================================
-- BẢNG TRUNG GIAN (Many-to-Many)
-- ========================================

-- Nhân viên - Tour (1 tour có 1 hướng dẫn viên)
CREATE TABLE NhanVien_Tour (
    MaNV INT,
    MaTour INT,
    PRIMARY KEY (MaNV, MaTour),
    FOREIGN KEY (MaNV) REFERENCES NhanVien(MaNV) ON DELETE CASCADE,
    FOREIGN KEY (MaTour) REFERENCES Tour(MaTour) ON DELETE CASCADE
);

-- Tour - Phương tiện
CREATE TABLE Tour_PhuongTien (
    MaTour INT,
    MaPT INT,
    PRIMARY KEY (MaTour, MaPT),
    FOREIGN KEY (MaTour) REFERENCES Tour(MaTour) ON DELETE CASCADE,
    FOREIGN KEY (MaPT) REFERENCES PhuongTien(MaPT) ON DELETE CASCADE
);

-- Tour - Địa điểm
CREATE TABLE Tour_DiaDiem (
    MaTour INT,
    MaDD INT,
    PRIMARY KEY (MaTour, MaDD),
    FOREIGN KEY (MaTour) REFERENCES Tour(MaTour) ON DELETE CASCADE,
    FOREIGN KEY (MaDD) REFERENCES DiaDiem(MaDD) ON DELETE CASCADE
);

-- Tour - Lịch trình
CREATE TABLE Tour_LichTrinhCT (
    MaTour INT,
    MaLT INT,
    PRIMARY KEY (MaTour, MaLT),
    FOREIGN KEY (MaTour) REFERENCES Tour(MaTour) ON DELETE CASCADE,
    FOREIGN KEY (MaLT) REFERENCES LichTrinhCT(MaLT) ON DELETE CASCADE
);

-- Khách hàng - Tour
CREATE TABLE KhachHang_Tour (
    MaKH INT,
    MaTour INT,
    SoLuongVe INT CHECK (SoLuongVe > 0),
    PRIMARY KEY (MaKH, MaTour),
    FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH) ON DELETE CASCADE,
    FOREIGN KEY (MaTour) REFERENCES Tour(MaTour) ON DELETE CASCADE
);

-- Dịch vụ đã đăng ký
CREATE TABLE DichVuDaDangKy (
    MaKH INT,
    MaTour INT,
    MaDV INT,
    PRIMARY KEY (MaKH, MaTour, MaDV),
    FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH) ON DELETE CASCADE,
    FOREIGN KEY (MaTour) REFERENCES Tour(MaTour) ON DELETE CASCADE,
    FOREIGN KEY (MaDV) REFERENCES DichVuDiKem(MaDV) ON DELETE CASCADE
);

GO
PRINT N'Đã tạo xong cấu trúc bảng !';

-- =====================================================
-- PHẦN 2: INSERT DỮ LIỆU
-- =====================================================

-- Insert Users
SET IDENTITY_INSERT [User] ON;
INSERT INTO [User] (UserId, Username, PasswordHash, FullName, Email, Phone, Role) VALUES
(1, 'admin', 'A665A45920422F9D417E4867EFDC4FB8A04A1F3FFF1FA07E998E86F7F7A27AE3', N'Quản trị viên', 'admin@qldulich.com', '0900000000', 'Admin'),
(2, 'guide1', 'A665A45920422F9D417E4867EFDC4FB8A04A1F3FFF1FA07E998E86F7F7A27AE3', N'Nguyễn Văn An', 'guide1@qldulich.com', '0901234567', 'Guide'),
(3, 'guide2', 'A665A45920422F9D417E4867EFDC4FB8A04A1F3FFF1FA07E998E86F7F7A27AE3', N'Trần Thị Bình', 'guide2@qldulich.com', '0912345678', 'Guide'),
(4, 'guide3', 'A665A45920422F9D417E4867EFDC4FB8A04A1F3FFF1FA07E998E86F7F7A27AE3', N'Lê Văn Cường', 'guide3@qldulich.com', '0923456789', 'Guide'),
(5, 'guide4', 'A665A45920422F9D417E4867EFDC4FB8A04A1F3FFF1FA07E998E86F7F7A27AE3', N'Phạm Thị Dung', 'guide4@qldulich.com', '0934567890', 'Guide'),
(6, 'guide5', 'A665A45920422F9D417E4867EFDC4FB8A04A1F3FFF1FA07E998E86F7F7A27AE3', N'Hoàng Văn Em', 'guide5@qldulich.com', '0945678901', 'Guide'),
(7, 'user1', 'A665A45920422F9D417E4867EFDC4FB8A04A1F3FFF1FA07E998E86F7F7A27AE3', N'Võ Văn Khách', 'user1@gmail.com', '0956789012', 'Customer'),
(8, 'user2', 'A665A45920422F9D417E4867EFDC4FB8A04A1F3FFF1FA07E998E86F7F7A27AE3', N'Nguyễn Thị Lan', 'user2@gmail.com', '0967890123', 'Customer'),
(9, 'user3', 'A665A45920422F9D417E4867EFDC4FB8A04A1F3FFF1FA07E998E86F7F7A27AE3', N'Trần Văn Minh', 'user3@gmail.com', '0978901234', 'Customer');
SET IDENTITY_INSERT [User] OFF;

-- Insert Nhân viên (Hướng dẫn viên)
INSERT INTO NhanVien (MaNV, TenNV, GioiTinh, SDT, DiaChi, VaiTro, AnhDaiDien, UserId) VALUES
(1, N'Nguyễn Văn An', N'Nam', '0901234567', N'123 Lê Lợi, Q1, TP.HCM', N'Hướng dẫn viên', N'guide1.jpg', 2),
(2, N'Trần Thị Bình', N'Nữ', '0912345678', N'456 Nguyễn Huệ, Q1, TP.HCM', N'Hướng dẫn viên', N'guide2.jpg', 3),
(3, N'Lê Văn Cường', N'Nam', '0923456789', N'789 Võ Văn Tần, Q3, TP.HCM', N'Hướng dẫn viên', N'guide3.jpg', 4),
(4, N'Phạm Thị Dung', N'Nữ', '0934567890', N'321 Hai Bà Trưng, Q1, Hà Nội', N'Hướng dẫn viên', N'guide4.jpg', 5),
(5, N'Hoàng Văn Em', N'Nam', '0945678901', N'654 Trần Phú, Đà Nẵng', N'Hướng dẫn viên', N'guide5.jpg', 6);

-- Insert Khách hàng
SET IDENTITY_INSERT KhachHang ON;
INSERT INTO KhachHang (MaKH, TenKH, GioiTinh, SDT, DiaChi, CCCD, AnhDaiDien, UserId) VALUES
(1, N'Võ Văn Khách', N'Nam', '0956789012', N'123 Lê Lợi, Q1, TP.HCM', N'123456789012', N'avatar1.jpg', 7),
(2, N'Nguyễn Thị Lan', N'Nữ', '0967890123', N'456 Trần Hưng Đạo, Q5, TP.HCM', N'123456789013', N'avatar2.jpg', 8),
(3, N'Trần Văn Minh', N'Nam', '0978901234', N'789 Nguyễn Huệ, Q1, TP.HCM', N'123456789014', N'avatar3.jpg', 9);
SET IDENTITY_INSERT KhachHang OFF;

-- Insert 10 Tour với ngày sau tháng 12/2025 - Giá đơn vị 1000đ
INSERT INTO Tour (MaTour, TenTour, NgayBatDau, NgayKetThuc, Gia) VALUES
(1, N'Tour Đà Lạt - Thành phố ngàn hoa', '2026-01-15', '2026-01-17', 3500),
(2, N'Tour Nha Trang - Biển xanh cát trắng', '2026-01-20', '2026-01-23', 4800),
(3, N'Tour Phú Quốc - Đảo Ngọc thiên đường', '2026-02-01', '2026-02-04', 6500),
(4, N'Tour Hội An - Phố cổ đèn lồng', '2026-02-10', '2026-02-13', 5200),
(5, N'Tour Hạ Long - Kỳ quan thế giới', '2026-02-15', '2026-02-17', 5800),
(6, N'Tour Sapa - Miền Bắc huyền ảo', '2026-03-01', '2026-03-04', 4500),
(7, N'Tour Đà Nẵng - Thành phố đáng sống', '2026-03-10', '2026-03-13', 4900),
(8, N'Tour Vũng Tàu - Nghỉ dưỡng cuối tuần', '2026-03-20', '2026-03-21', 2500),
(9, N'Tour Mũi Né - Thiên đường nghỉ dưỡng', '2026-04-01', '2026-04-03', 3800),
(10, N'Tour Huế - Cố đô ngàn năm', '2026-04-15', '2026-04-17', 4700);

-- Insert 50 Địa điểm (5 điểm cho mỗi tour)
INSERT INTO DiaDiem (MaDD, TenDD, Tinh) VALUES
-- Đà Lạt (1-5)
(1, N'Hồ Xuân Hương', N'Lâm Đồng'),
(2, N'Thác Datanla', N'Lâm Đồng'),
(3, N'Đồi Chè Cầu Đất', N'Lâm Đồng'),
(4, N'Thiền viện Trúc Lâm', N'Lâm Đồng'),
(5, N'Ga Đà Lạt', N'Lâm Đồng'),
-- Nha Trang (6-10)
(6, N'Vinpearl Land', N'Khánh Hòa'),
(7, N'Biển Nha Trang', N'Khánh Hòa'),
(8, N'Tháp Bà Ponagar', N'Khánh Hòa'),
(9, N'Hòn Mun', N'Khánh Hòa'),
(10, N'Viện Hải Dương Học', N'Khánh Hòa'),
-- Phú Quốc (11-15)
(11, N'Bãi Sao', N'Kiên Giang'),
(12, N'Dinh Cậu', N'Kiên Giang'),
(13, N'Vinpearl Safari', N'Kiên Giang'),
(14, N'Sunset Sanato Beach Club', N'Kiên Giang'),
(15, N'Grand World Phú Quốc', N'Kiên Giang'),
-- Hội An (16-20)
(16, N'Phố Cổ Hội An', N'Quảng Nam'),
(17, N'Chùa Cầu', N'Quảng Nam'),
(18, N'Rừng Dừa Bảy Mẫu', N'Quảng Nam'),
(19, N'Làng Gốm Thanh Hà', N'Quảng Nam'),
(20, N'Cù Lao Chàm', N'Quảng Nam'),
-- Hạ Long (21-25)
(21, N'Vịnh Hạ Long', N'Quảng Ninh'),
(22, N'Hang Sửng Sốt', N'Quảng Ninh'),
(23, N'Đảo Titop', N'Quảng Ninh'),
(24, N'Làng Chài Cửa Vạn', N'Quảng Ninh'),
(25, N'Hang Đầu Gỗ', N'Quảng Ninh'),
-- Sapa (26-30)
(26, N'Bản Cát Cát', N'Lào Cai'),
(27, N'Thác Bạc', N'Lào Cai'),
(28, N'Đỉnh Fansipan', N'Lào Cai'),
(29, N'Thị Trấn Sapa', N'Lào Cai'),
(30, N'Ruộng Bậc Thang', N'Lào Cai'),
-- Đà Nẵng (31-35)
(31, N'Bà Nà Hills', N'Đà Nẵng'),
(32, N'Cầu Vàng', N'Đà Nẵng'),
(33, N'Bãi Biển Mỹ Khê', N'Đà Nẵng'),
(34, N'Chùa Linh Ứng', N'Đà Nẵng'),
(35, N'Ngũ Hành Sơn', N'Đà Nẵng'),
-- Vũng Tàu (36-40)
(36, N'Tượng Chúa Kitô', N'Bà Rịa - Vũng Tàu'),
(37, N'Bãi Trước', N'Bà Rịa - Vũng Tàu'),
(38, N'Bãi Sau', N'Bà Rịa - Vũng Tàu'),
(39, N'Hải Đăng Vũng Tàu', N'Bà Rịa - Vũng Tàu'),
(40, N'Bạch Dinh', N'Bà Rịa - Vũng Tàu'),
-- Mũi Né (41-45)
(41, N'Đồi Cát Bay', N'Bình Thuận'),
(42, N'Suối Tiên', N'Bình Thuận'),
(43, N'Làng Chài Mũi Né', N'Bình Thuận'),
(44, N'Bãi Biển Mũi Né', N'Bình Thuận'),
(45, N'Đồi Cát Trắng', N'Bình Thuận'),
-- Huế (46-50)
(46, N'Đại Nội Huế', N'Thừa Thiên Huế'),
(47, N'Lăng Khải Định', N'Thừa Thiên Huế'),
(48, N'Chùa Thiên Mụ', N'Thừa Thiên Huế'),
(49, N'Đồng Văn Cổ Trấn', N'Thừa Thiên Huế'),
(50, N'Sông Hương', N'Thừa Thiên Huế');

-- Insert 10 Phương tiện
INSERT INTO PhuongTien (MaPT, TenPT, LoaiPT, SoCho) VALUES
(1, N'Xe 16 chỗ Ford Transit', N'Xe khách', 16),
(2, N'Xe 29 chỗ Hyundai County', N'Xe khách', 29),
(3, N'Xe 45 chỗ Thaco', N'Xe khách', 45),
(4, N'Máy bay Vietnam Airlines', N'Máy bay', 180),
(5, N'Máy bay VietJet Air', N'Máy bay', 180),
(6, N'Tàu cao tốc Superdong', N'Tàu thủy', 200),
(7, N'Cano 12 chỗ', N'Tàu thủy', 12),
(8, N'Du thuyền 5 sao', N'Du thuyền', 50),
(9, N'Xe limousine 9 chỗ', N'Limousine', 9),
(10, N'Tàu hỏa SE1', N'Tàu hỏa', 64);

-- Insert 10 Dịch vụ đi kèm
INSERT INTO DichVuDiKem (MaDV, TenDV, Gia, LoaiDV) VALUES
(1, N'Bảo hiểm du lịch', 100000, N'Bảo hiểm'),
(2, N'Ăn sáng buffet', 150000, N'Ăn uống'),
(3, N'Massage trị liệu', 300000, N'Spa'),
(4, N'Tour tham quan thêm', 500000, N'Tham quan'),
(5, N'Thuê xe máy', 200000, N'Di chuyển'),
(6, N'Lặn biển ngắm san hô', 600000, N'Thể thao'),
(7, N'Dù lượn', 800000, N'Thể thao'),
(8, N'Karaoke tối', 400000, N'Giải trí'),
(9, N'Phòng đơn (phụ thu)', 500000, N'Phòng nghỉ'),
(10, N'Nâng hạng phòng VIP', 1000000, N'Phòng nghỉ');

-- Insert 34 Lịch trình chi tiết
INSERT INTO LichTrinhCT (MaLT, NgayThu, NoiDung) VALUES
-- Tour Đà Lạt (1-3)
(1, 1, N'06:00 - Khởi hành từ TP.HCM, dừng chân ăn sáng tại Dầu Giây. 12:00 - Đến Đà Lạt, check-in khách sạn. 14:00 - Tham quan Hồ Xuân Hương, chụp ảnh. 18:00 - Ăn tối, tự do khám phá chợ đêm Đà Lạt.'),
(2, 2, N'08:00 - Ăn sáng tại khách sạn. 09:00 - Tham quan Thác Datanla, trải nghiệm xe trượt ống. 12:00 - Ăn trưa. 14:00 - Khám phá Đồi Chè Cầu Đất, check-in. 18:00 - Ăn tối, nghỉ ngơi.'),
(3, 3, N'07:00 - Ăn sáng. 08:00 - Tham quan Thiền viện Trúc Lâm, cáp treo ngắm cảnh. 11:00 - Tham quan Ga Đà Lạt. 12:00 - Ăn trưa. 14:00 - Khởi hành về TP.HCM. 20:00 - Về đến TP.HCM, kết thúc chuyến đi.'),
-- Tour Nha Trang (4-7)
(4, 1, N'05:00 - Khởi hành từ TP.HCM. 12:00 - Đến Nha Trang, check-in khách sạn. 14:00 - Tắm biển Nha Trang. 18:00 - Ăn tối hải sản, dạo phố biển.'),
(5, 2, N'08:00 - Ăn sáng. 09:00 - Tham quan Vinpearl Land cả ngày. 12:00 - Ăn trưa tại công viên. 18:00 - Về khách sạn, ăn tối tự do.'),
(6, 3, N'07:00 - Ăn sáng. 08:00 - Tour 4 đảo: Hòn Mun lặn ngắm san hô, thăm Viện Hải Dương Học. 17:00 - Về khách sạn. 19:00 - Ăn tối BBQ hải sản.'),
(7, 4, N'08:00 - Ăn sáng. 09:00 - Tham quan Tháp Bà Ponagar. 11:00 - Mua sắm đặc sản. 12:00 - Ăn trưa. 14:00 - Khởi hành về TP.HCM. 20:00 - Kết thúc tour.'),
-- Tour Phú Quốc (8-11)
(8, 1, N'06:00 - Bay từ TP.HCM đến Phú Quốc. 09:00 - Đến sân bay Phú Quốc, xe đón về khách sạn. 10:00 - Check-in, nghỉ ngơi. 14:00 - Tham quan Dinh Cậu. 18:00 - Ăn tối tại Sunset Sanato, ngắm hoàng hôn.'),
(9, 2, N'08:00 - Ăn sáng. 09:00 - Tour Nam đảo: Bãi Sao tắm biển. 12:00 - Ăn trưa hải sản. 14:00 - Khám phá Grand World Phú Quốc. 19:00 - Ăn tối, xem show nghệ thuật.'),
(10, 3, N'08:00 - Ăn sáng. 09:00 - Tham quan Vinpearl Safari cả ngày. 12:00 - Ăn trưa. 17:00 - Về khách sạn nghỉ ngơi. 19:00 - Ăn tối BBQ.'),
(11, 4, N'08:00 - Ăn sáng. 10:00 - Tự do tắm biển, mua sắm. 12:00 - Ăn trưa. 14:00 - Ra sân bay về TP.HCM. 17:00 - Kết thúc tour.'),
-- Tour Hội An (12-15)
(12, 1, N'06:00 - Bay từ TP.HCM đến Đà Nẵng. 09:00 - Đến Đà Nẵng, xe đón về Hội An. 11:00 - Check-in khách sạn. 14:00 - Tham quan Phố Cổ Hội An, Chùa Cầu. 18:00 - Thả đèn lồng sông Hoài.'),
(13, 2, N'08:00 - Ăn sáng. 09:00 - Tour Rừng Dừa Bảy Mẫu, chèo thuyền thúng. 12:00 - Ăn trưa. 14:00 - Tham quan Làng Gốm Thanh Hà. 18:00 - Ăn tối cao lầu, cơm gà.'),
(14, 3, N'07:00 - Ăn sáng. 08:00 - Tour Cù Lao Chàm, lặn biển. 12:00 - Ăn trưa hải sản. 15:00 - Về Hội An. 18:00 - Tự do mua sắm.'),
(15, 4, N'08:00 - Ăn sáng. 09:00 - Tự do nghỉ ngơi. 11:00 - Ra sân bay Đà Nẵng. 13:00 - Bay về TP.HCM. 15:00 - Kết thúc tour.'),
-- Tour Hạ Long (16-18)
(16, 1, N'05:00 - Khởi hành từ Hà Nội. 08:00 - Dừng nghỉ tại Hải Dương. 12:00 - Đến Hạ Long, lên du thuyền 5 sao. 14:00 - Tham quan Hang Sửng Sột. 18:00 - Ăn tối buffet hải sản, karaoke.'),
(17, 2, N'07:00 - Tập Thái Cực trên boong tàu. 08:00 - Ăn sáng. 09:00 - Tham quan Đảo Titop, leo núi ngắm toàn cảnh. 12:00 - Ăn trưa. 14:00 - Tham quan Làng Chài Cửa Vạn. 18:00 - Ăn tối.'),
(18, 3, N'07:00 - Ăn sáng. 08:00 - Tham quan Hang Đầu Gỗ. 10:00 - Xuống thuyền, khởi hành về Hà Nội. 14:00 - Về đến Hà Nội, kết thúc tour.'),
-- Tour Sapa (19-22)
(19, 1, N'21:00 - Khởi hành từ Hà Nội bằng tàu hỏa. Ngủ đêm trên tàu.'),
(20, 2, N'06:00 - Đến Lào Cai, xe đón lên Sapa. 08:00 - Ăn sáng. 09:00 - Tham quan Bản Cát Cát, check-in. 12:00 - Ăn trưa. 14:00 - Thác Bạc. 18:00 - Ăn tối, nghỉ ngơi.'),
(21, 3, N'05:00 - Chinh phục Fansipan bằng cáp treo. 12:00 - Ăn trưa. 14:00 - Tham quan thị trấn Sapa. 18:00 - Ăn tối, chợ tình Sapa.'),
(22, 4, N'08:00 - Ăn sáng. 09:00 - Tham quan Ruộng Bậc Thang. 11:00 - Khởi hành về Lào Cai. 14:00 - Tàu về Hà Nội. 21:00 - Về Hà Nội, kết thúc.'),
-- Tour Đà Nẵng (23-26)
(23, 1, N'06:00 - Bay từ TP.HCM đến Đà Nẵng. 09:00 - Đến Đà Nẵng, check-in. 14:00 - Tắm biển Mỹ Khê. 18:00 - Ăn tối hải sản.'),
(24, 2, N'08:00 - Ăn sáng. 09:00 - Tham quan Bà Nà Hills cả ngày, Cầu Vàng. 12:00 - Ăn trưa. 18:00 - Về khách sạn.'),
(25, 3, N'08:00 - Ăn sáng. 09:00 - Tham quan Ngũ Hành Sơn, Chùa Linh Ứng. 12:00 - Ăn trưa. 14:00 - Tự do mua sắm. 18:00 - Ăn tối.'),
(26, 4, N'08:00 - Ăn sáng. 10:00 - Ra sân bay về TP.HCM. 12:00 - Kết thúc tour.'),
-- Tour Vũng Tàu (27-28)
(27, 1, N'06:00 - Khởi hành từ TP.HCM. 08:30 - Đến Vũng Tàu. 09:00 - Tham quan Tượng Chúa Kitô, Bạch Dinh. 12:00 - Ăn trưa hải sản. 14:00 - Tắm biển Bãi Trước. 18:00 - Ăn tối BBQ.'),
(28, 2, N'08:00 - Ăn sáng. 09:00 - Tham quan Hải Đăng, Bãi Sau. 12:00 - Ăn trưa. 14:00 - Khởi hành về TP.HCM. 16:30 - Kết thúc tour.'),
-- Tour Mũi Né (29-31)
(29, 1, N'06:00 - Khởi hành từ TP.HCM. 10:00 - Đến Mũi Né, check-in resort. 14:00 - Tắm biển. 18:00 - BBQ hải sản.'),
(30, 2, N'05:00 - Ngắm bình minh Đồi Cát Bay. 08:00 - Ăn sáng. 09:00 - Tham quan Suối Tiên, Làng Chài. 12:00 - Ăn trưa. 14:00 - Nghỉ ngơi tại resort. 18:00 - Ăn tối.'),
(31, 3, N'08:00 - Ăn sáng. 09:00 - Tham quan Đồi Cát Trắng. 11:00 - Khởi hành về TP.HCM. 15:00 - Kết thúc tour.'),
-- Tour Huế (32-34)
(32, 1, N'06:00 - Bay từ TP.HCM đến Huế. 09:00 - Đến Huế, check-in. 14:00 - Tham quan Đại Nội. 18:00 - Ăn tối cơm niêu, đi thuyền rồng sông Hương.'),
(33, 2, N'08:00 - Ăn sáng. 09:00 - Tham quan Lăng Khải Định, Chùa Thiên Mụ. 12:00 - Ăn trưa bún bò Huế. 14:00 - Đồng Văn Cổ Trấn. 18:00 - Ăn tối.'),
(34, 3, N'08:00 - Ăn sáng. 09:00 - Tự do mua sắm. 11:00 - Ra sân bay về TP.HCM. 13:00 - Kết thúc tour.');

-- Insert Nhân viên - Tour (Phân công hướng dẫn viên)
INSERT INTO NhanVien_Tour (MaNV, MaTour) VALUES
(1, 1), (2, 2), (3, 3), (4, 4), (5, 5),
(1, 6), (2, 7), (3, 8), (4, 9), (5, 10);

-- Insert Tour - Ảnh (40 ảnh - mỗi tour 4 ảnh)
SET IDENTITY_INSERT Tour_Anh ON;
INSERT INTO Tour_Anh (MaAnh, MaTour, DuongDan) VALUES
-- Tour 1: Đà Lạt
(1, 1, 'album/1_1.jpg'), (2, 1, 'album/1_2.jpg'), (3, 1, 'album/1_3.jpg'), (4, 1, 'album/1_4.jpg'),
-- Tour 2: Nha Trang
(5, 2, 'album/2_1.jpg'), (6, 2, 'album/2_2.jpg'), (7, 2, 'album/2_3.jpg'), (8, 2, 'album/2_4.jpg'),
-- Tour 3: Phú Quốc
(9, 3, 'album/3_1.jpg'), (10, 3, 'album/3_2.jpg'), (11, 3, 'album/3_3.jpg'), (12, 3, 'album/3_4.jpg'),
-- Tour 4: Hội An
(13, 4, 'album/4_1.jpg'), (14, 4, 'album/4_2.jpg'), (15, 4, 'album/4_3.jpg'), (16, 4, 'album/4_4.jpg'),
-- Tour 5: Hạ Long
(17, 5, 'album/5_1.jpg'), (18, 5, 'album/5_2.jpg'), (19, 5, 'album/5_3.jpg'), (20, 5, 'album/5_4.jpg'),
-- Tour 6: Sapa
(21, 6, 'album/6_1.jpg'), (22, 6, 'album/6_2.jpg'), (23, 6, 'album/6_3.jpg'), (24, 6, 'album/6_4.jpg'),
-- Tour 7: Đà Nẵng
(25, 7, 'album/7_1.jpg'), (26, 7, 'album/7_2.jpg'), (27, 7, 'album/7_3.jpg'), (28, 7, 'album/7_4.jpg'),
-- Tour 8: Vũng Tàu
(29, 8, 'album/8_1.jpg'), (30, 8, 'album/8_2.jpg'), (31, 8, 'album/8_3.jpg'), (32, 8, 'album/8_4.jpg'),
-- Tour 9: Mũi Né
(33, 9, 'album/9_1.jpg'), (34, 9, 'album/9_2.jpg'), (35, 9, 'album/9_3.jpg'), (36, 9, 'album/9_4.jpg'),
-- Tour 10: Huế
(37, 10, 'album/10_1.jpg'), (38, 10, 'album/10_2.jpg'), (39, 10, 'album/10_3.jpg'), (40, 10, 'album/10_4.jpg');
SET IDENTITY_INSERT Tour_Anh OFF;

-- Insert Tour - Phương tiện
INSERT INTO Tour_PhuongTien (MaTour, MaPT) VALUES
(1, 2), (2, 3), (2, 7), (3, 4), (3, 1),
(4, 5), (4, 2), (5, 8), (5, 2), (6, 10),
(6, 2), (7, 4), (7, 2), (8, 9), (9, 3),
(10, 5), (10, 1);

-- Insert Tour - Địa điểm (50 liên kết)
INSERT INTO Tour_DiaDiem (MaTour, MaDD) VALUES
-- Tour 1-10, mỗi tour 5 địa điểm
(1,1), (1,2), (1,3), (1,4), (1,5),
(2,6), (2,7), (2,8), (2,9), (2,10),
(3,11), (3,12), (3,13), (3,14), (3,15),
(4,16), (4,17), (4,18), (4,19), (4,20),
(5,21), (5,22), (5,23), (5,24), (5,25),
(6,26), (6,27), (6,28), (6,29), (6,30),
(7,31), (7,32), (7,33), (7,34), (7,35),
(8,36), (8,37), (8,38), (8,39), (8,40),
(9,41), (9,42), (9,43), (9,44), (9,45),
(10,46), (10,47), (10,48), (10,49), (10,50);

-- Insert Tour - Lịch trình
INSERT INTO Tour_LichTrinhCT (MaTour, MaLT) VALUES
(1,1), (1,2), (1,3),
(2,4), (2,5), (2,6), (2,7),
(3,8), (3,9), (3,10), (3,11),
(4,12), (4,13), (4,14), (4,15),
(5,16), (5,17), (5,18),
(6,19), (6,20), (6,21), (6,22),
(7,23), (7,24), (7,25), (7,26),
(8,27), (8,28),
(9,29), (9,30), (9,31),
(10,32), (10,33), (10,34);

-- Insert Khách hàng - Tour (Khách hàng đăng ký tour)
INSERT INTO KhachHang_Tour (MaKH, MaTour, SoLuongVe) VALUES
(1, 1, 2),  -- Võ Văn Khách đăng ký Tour Đà Lạt, 2 vé
(1, 3, 1),  -- Võ Văn Khách đăng ký Tour Phú Quốc, 1 vé
(2, 2, 3),  -- Nguyễn Thị Lan đăng ký Tour Nha Trang, 3 vé
(2, 5, 2),  -- Nguyễn Thị Lan đăng ký Tour Hạ Long, 2 vé
(3, 4, 2),  -- Trần Văn Minh đăng ký Tour Hội An, 2 vé
(3, 7, 4),  -- Trần Văn Minh đăng ký Tour Đà Nẵng, 4 vé
(1, 8, 2),  -- Võ Văn Khách đăng ký Tour Vũng Tàu, 2 vé
(2, 10, 1), -- Nguyễn Thị Lan đăng ký Tour Huế, 1 vé
(3, 6, 2),  -- Trần Văn Minh đăng ký Tour Sapa, 2 vé
(1, 9, 3);  -- Võ Văn Khách đăng ký Tour Mũi Né, 3 vé

-- Insert Dịch vụ đã đăng ký
INSERT INTO DichVuDaDangKy (MaKH, MaTour, MaDV) VALUES
-- Khách hàng 1 - Tour 1 (Đà Lạt)
(1, 1, 1),  -- Bảo hiểm du lịch
(1, 1, 2),  -- Ăn sáng buffet
-- Khách hàng 1 - Tour 3 (Phú Quốc)
(1, 3, 1),  -- Bảo hiểm du lịch
(1, 3, 3),  -- Massage trị liệu
(1, 3, 6),  -- Lặn biển ngắm san hô
-- Khách hàng 2 - Tour 2 (Nha Trang)
(2, 2, 1),  -- Bảo hiểm du lịch
(2, 2, 6),  -- Lặn biển ngắm san hô
(2, 2, 8),  -- Karaoke tối
-- Khách hàng 2 - Tour 5 (Hạ Long)
(2, 5, 1),  -- Bảo hiểm du lịch
(2, 5, 2),  -- Ăn sáng buffet
(2, 5, 10), -- Nâng hạng phòng VIP
-- Khách hàng 3 - Tour 4 (Hội An)
(3, 4, 1),  -- Bảo hiểm du lịch
(3, 4, 5),  -- Thuê xe máy
-- Khách hàng 3 - Tour 7 (Đà Nẵng)
(3, 7, 1),  -- Bảo hiểm du lịch
(3, 7, 4),  -- Tour tham quan thêm
(3, 7, 7),  -- Dù lượn
-- Khách hàng 1 - Tour 8 (Vũng Tàu)
(1, 8, 1),  -- Bảo hiểm du lịch
-- Khách hàng 2 - Tour 10 (Huế)
(2, 10, 1), -- Bảo hiểm du lịch
(2, 10, 9), -- Phòng đơn
-- Khách hàng 3 - Tour 6 (Sapa)
(3, 6, 1),  -- Bảo hiểm du lịch
(3, 6, 2),  -- Ăn sáng buffet
-- Khách hàng 1 - Tour 9 (Mũi Né)
(1, 9, 1),  -- Bảo hiểm du lịch
(1, 9, 3),  -- Massage trị liệu
(1, 9, 5);  -- Thuê xe máy

-- Insert Hóa đơn
SET IDENTITY_INSERT HoaDon ON;
INSERT INTO HoaDon (MaHD, MaKH, MaTour, NgayTT, HinhThucTT, TongTien, TrangThai) VALUES
-- Hóa đơn 1: Khách 1 - Tour Đà Lạt (3,500,000 x 2 + 100,000 + 150,000)
(1, 1, 1, '2025-12-01', N'Chuyển khoản', 7250000, N'Đã thanh toán'),
-- Hóa đơn 2: Khách 1 - Tour Phú Quốc (6,500,000 x 1 + 100,000 + 300,000 + 600,000)
(2, 1, 3, '2025-12-15', N'Chuyển khoản', 7500000, N'Đã thanh toán'),
-- Hóa đơn 3: Khách 2 - Tour Nha Trang (4,800,000 x 3 + 100,000 + 600,000 + 400,000)
(3, 2, 2, '2025-12-05', N'QR Code', 15500000, N'Đã thanh toán'),
-- Hóa đơn 4: Khách 2 - Tour Hạ Long (5,800,000 x 2 + 100,000 + 150,000 + 1,000,000)
(4, 2, 5, '2025-12-20', N'Chuyển khoản', 12850000, N'Đã thanh toán'),
-- Hóa đơn 5: Khách 3 - Tour Hội An (5,200,000 x 2 + 100,000 + 200,000)
(5, 3, 4, '2025-12-10', N'Tiền mặt', 10700000, N'Đã thanh toán'),
-- Hóa đơn 6: Khách 3 - Tour Đà Nẵng (4,900,000 x 4 + 100,000 + 500,000 + 800,000)
(6, 3, 7, '2025-12-25', N'Chuyển khoản', 21000000, N'Đã thanh toán'),
-- Hóa đơn 7: Khách 1 - Tour Vũng Tàu (2,500,000 x 2 + 100,000)
(7, 1, 8, '2026-01-05', N'Tiền mặt', 5100000, N'Đã thanh toán'),
-- Hóa đơn 8: Khách 2 - Tour Huế (4,700,000 x 1 + 100,000 + 500,000)
(8, 2, 10, '2026-01-10', N'QR Code', 5300000, N'Đã thanh toán'),
-- Hóa đơn 9: Khách 3 - Tour Sapa (4,500,000 x 2 + 100,000 + 150,000)
(9, 3, 6, '2026-01-15', N'Chuyển khoản', 9250000, N'Đã thanh toán'),
-- Hóa đơn 10: Khách 1 - Tour Mũi Né (3,800,000 x 3 + 100,000 + 300,000 + 200,000)
(10, 1, 9, '2026-01-20', N'Chuyển khoản', 12000000, N'Đã thanh toán');
SET IDENTITY_INSERT HoaDon OFF;

GO
PRINT N'Đã insert xong tất cả dữ liệu';
PRINT N'✓ Đã thêm 10 bản ghi KhachHang_Tour';
PRINT N'✓ Đã thêm 23 bản ghi DichVuDaDangKy';
PRINT N'✓ Đã thêm 10 bản ghi HoaDon';
PRINT N'✓ Đã thêm 15 bản ghi DichVuDiKem';

-- =====================================================
-- PHẦN 3: STORED PROCEDURE - Đăng ký tour
-- =====================================================
GO
CREATE OR ALTER PROCEDURE sp_DangKyTour
    @MaKH INT,
    @MaTour INT,
    @SoLuongVe INT,
    @DanhSachDV NVARCHAR(MAX) = NULL -- Danh sách MaDV ngăn cách bởi dấu phẩy, VD: '1,3,5'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TongTien DECIMAL(18,2) = 0;
    DECLARE @GiaTour DECIMAL(18,2);
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Lấy giá tour
        SELECT @GiaTour = Gia FROM Tour WHERE MaTour = @MaTour;
        
        -- Tính tổng tiền tour
        SET @TongTien = @GiaTour * @SoLuongVe;
        
        -- Thêm vào bảng KhachHang_Tour
        IF NOT EXISTS (SELECT 1 FROM KhachHang_Tour WHERE MaKH = @MaKH AND MaTour = @MaTour)
        BEGIN
            INSERT INTO KhachHang_Tour (MaKH, MaTour, SoLuongVe)
            VALUES (@MaKH, @MaTour, @SoLuongVe);
        END
        ELSE
        BEGIN
            UPDATE KhachHang_Tour 
            SET SoLuongVe = @SoLuongVe
            WHERE MaKH = @MaKH AND MaTour = @MaTour;
        END
        
        -- Xử lý dịch vụ đi kèm
        IF @DanhSachDV IS NOT NULL AND LEN(@DanhSachDV) > 0
        BEGIN
            -- Xóa các dịch vụ cũ
            DELETE FROM DichVuDaDangKy WHERE MaKH = @MaKH AND MaTour = @MaTour;
            
            -- Thêm dịch vụ mới
            DECLARE @MaDV INT;
            DECLARE @Pos INT;
            DECLARE @GiaDV DECIMAL(18,2);
            
            -- Parse danh sách dịch vụ
            WHILE LEN(@DanhSachDV) > 0
            BEGIN
                SET @Pos = CHARINDEX(',', @DanhSachDV);
                
                IF @Pos > 0
                BEGIN
                    SET @MaDV = CAST(SUBSTRING(@DanhSachDV, 1, @Pos - 1) AS INT);
                    SET @DanhSachDV = SUBSTRING(@DanhSachDV, @Pos + 1, LEN(@DanhSachDV));
                END
                ELSE
                BEGIN
                    SET @MaDV = CAST(@DanhSachDV AS INT);
                    SET @DanhSachDV = '';
                END
                
                -- Thêm dịch vụ
                INSERT INTO DichVuDaDangKy (MaKH, MaTour, MaDV)
                VALUES (@MaKH, @MaTour, @MaDV);
                
                -- Cộng giá dịch vụ vào tổng tiền
                SELECT @GiaDV = Gia FROM DichVuDiKem WHERE MaDV = @MaDV;
                SET @TongTien = @TongTien + @GiaDV;
            END
        END
        
        -- Tạo hóa đơn
        INSERT INTO HoaDon (MaKH, MaTour, NgayTT, HinhThucTT, TongTien, TrangThai)
        VALUES (@MaKH, @MaTour, GETDATE(), N'Chưa thanh toán', @TongTien, N'Chưa thanh toán');
        
        COMMIT TRANSACTION;
        
        SELECT @TongTien AS TongTien, N'Đăng ký tour thành công!' AS Message;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO

PRINT N'=====================================================';
PRINT N'✓ HOÀN TẤT TẠO DATABASE QLDuLich';
PRINT N'=====================================================';
PRINT N'';
PRINT N'Thống kê dữ liệu:';
PRINT N'- Users: ' + CAST((SELECT COUNT(*) FROM [User]) AS NVARCHAR(10));
PRINT N'- Nhân viên: ' + CAST((SELECT COUNT(*) FROM NhanVien) AS NVARCHAR(10));
PRINT N'- Khách hàng: ' + CAST((SELECT COUNT(*) FROM KhachHang) AS NVARCHAR(10));
PRINT N'- Tours: ' + CAST((SELECT COUNT(*) FROM Tour) AS NVARCHAR(10));
PRINT N'- Địa điểm: ' + CAST((SELECT COUNT(*) FROM DiaDiem) AS NVARCHAR(10));
PRINT N'- Phương tiện: ' + CAST((SELECT COUNT(*) FROM PhuongTien) AS NVARCHAR(10));
PRINT N'- Dịch vụ đi kèm: ' + CAST((SELECT COUNT(*) FROM DichVuDiKem) AS NVARCHAR(10));
PRINT N'- Lịch trình: ' + CAST((SELECT COUNT(*) FROM LichTrinhCT) AS NVARCHAR(10));
PRINT N'- Hóa đơn: ' + CAST((SELECT COUNT(*) FROM HoaDon) AS NVARCHAR(10));
GO

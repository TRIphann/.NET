-- =====================================================
-- XÓA VÀ TẠO LẠI DỮ LIỆU MẪU VỚI UTF-8 ĐÚNG
-- =====================================================

USE QLJumparena2;
GO

PRINT N'Xóa dữ liệu cũ...';

-- Xóa dữ liệu (giữ cấu trúc bảng)
DELETE FROM NhanVien_Ca;
DELETE FROM Ve;
DELETE FROM HoaDon;
DELETE FROM KhachHang;
DELETE FROM NhanVien;
DELETE FROM GoiDichVu;
DELETE FROM Ca;

PRINT N'✓ Đã xóa dữ liệu cũ';

-- Reset identity
DBCC CHECKIDENT ('GoiDichVu', RESEED, 0);
DBCC CHECKIDENT ('NhanVien', RESEED, 0);
DBCC CHECKIDENT ('KhachHang', RESEED, 0);
DBCC CHECKIDENT ('Ca', RESEED, 0);

PRINT N'Tạo dữ liệu mới với UTF-8...';

-- Tạo Gói Dịch Vụ
INSERT INTO GoiDichVu (TenGoi, LoaiGoi, MoTa, Gia, ThoiGian, SoLuotChoi, TrangThai) VALUES
(N'Gói Cơ Bản 1 Giờ', N'Theo thời gian (phút)', N'Gói cơ bản cho 1 người chơi trong 1 giờ', 100000, 60, NULL, N'Đang bán'),
(N'Gói Tiêu Chuẩn 2 Giờ', N'Theo thời gian (phút)', N'Gói tiêu chuẩn cho 1 người chơi trong 2 giờ', 180000, 120, NULL, N'Đang bán'),
(N'Gói VIP 3 Giờ', N'Theo thời gian (phút)', N'Gói VIP cho 1 người chơi trong 3 giờ', 250000, 180, NULL, N'Đang bán'),
(N'Gói Premium Cả Ngày', N'Theo thời gian (phút)', N'Gói premium không giới hạn thời gian trong ngày', 350000, 480, NULL, N'Đang bán'),
(N'Gói Gia Đình 4 Người', N'Theo thời gian (phút)', N'Gói gia đình cho 4 người chơi trong 2 giờ', 600000, 120, NULL, N'Đang bán'),
(N'Gói Học Sinh Sinh Viên', N'Theo thời gian (phút)', N'Gói ưu đãi cho học sinh sinh viên', 80000, 60, NULL, N'Đang bán'),
(N'Gói Cuối Tuần Ảo Diệu', N'Theo thời gian (phút)', N'Gói đặc biệt cuối tuần', 220000, 150, NULL, N'Đang bán'),
(N'Gói 10 Lượt Chơi', N'Theo số lượt chơi', N'Gói 10 lượt chơi không giới hạn thời gian', 200000, NULL, 10, N'Đang bán'),
(N'Gói 20 Lượt Chơi', N'Theo số lượt chơi', N'Gói 20 lượt chơi không giới hạn thời gian', 350000, NULL, 20, N'Đang bán'),
(N'Gói Nhóm 10 Người', N'Theo thời gian (phút)', N'Gói nhóm lớn cho 10 người', 1500000, 180, NULL, N'Đang bán');

PRINT N'✓ Đã tạo gói dịch vụ';

-- Tạo Ca
INSERT INTO Ca (TenCa, GioBatDau, GioKetThuc, SoLuongToiDa, TrangThai) VALUES
(N'Ca Sáng', '07:00:00', '12:00:00', 10, N'Hoạt động'),
(N'Ca Chiều', '12:00:00', '17:00:00', 10, N'Hoạt động'),
(N'Ca Tối', '17:00:00', '22:00:00', 10, N'Hoạt động');

PRINT N'✓ Đã tạo ca làm việc';

-- Tạo Nhân Viên
INSERT INTO NhanVien (TenNV, HoTen, GioiTinh, NgaySinh, SDT, DiaChi, Email) VALUES
(N'Nguyễn Văn A', N'Nguyễn Văn A', N'Nam', '1995-01-15', '0901234568', N'123 Lê Lợi, Q1, TP.HCM', 'nguyenvana@jumparena.vn'),
(N'Trần Thảo B', N'Trần Thảo B', N'Nữ', '1998-03-20', '0901234569', N'456 Nguyễn Huệ, Q1, TP.HCM', 'tranthaob@jumparena.vn'),
(N'Lê Thị Cẩm', N'Lê Thị Cẩm', N'Nữ', '1998-05-15', '0901234570', N'123 Lê Lợi, Q1, TP.HCM', 'lecam@jumparena.vn'),
(N'Phạm Văn Dũng', N'Phạm Văn Dũng', N'Nam', '1995-08-22', '0901234571', N'456 Nguyễn Huệ, Q1, TP.HCM', 'phamdung@jumparena.vn'),
(N'Hoàng Thị Lan', N'Hoàng Thị Lan', N'Nữ', '1997-11-30', '0901234572', N'789 Trần Hưng Đạo, Q5, TP.HCM', 'hoanglan@jumparena.vn'),
(N'Đặng Văn Hùng', N'Đặng Văn Hùng', N'Nam', '1996-03-12', '0901234573', N'321 Võ Văn Tần, Q3, TP.HCM', 'danghung@jumparena.vn'),
(N'Vũ Thị Mai', N'Vũ Thị Mai', N'Nữ', '1999-07-18', '0901234574', N'654 Hai Bà Trưng, Q3, TP.HCM', 'vumai@jumparena.vn'),
(N'Bùi Văn Tài', N'Bùi Văn Tài', N'Nam', '1994-12-25', '0901234575', N'987 Lý Thường Kiệt, Q10, TP.HCM', 'buitai@jumparena.vn'),
(N'Trương Thị Hoa', N'Trương Thị Hoa', N'Nữ', '1998-09-08', '0901234576', N'147 Cách Mạng Tháng 8, Q10, TP.HCM', 'truonghoa@jumparena.vn'),
(N'Ngô Văn Khoa', N'Ngô Văn Khoa', N'Nam', '1997-04-14', '0901234577', N'258 Điện Biên Phủ, Q3, TP.HCM', 'ngokhoa@jumparena.vn');

PRINT N'✓ Đã tạo nhân viên';

-- Tạo lịch làm việc ngẫu nhiên
DECLARE @Date DATE = DATEADD(DAY, -30, GETDATE());
DECLARE @EndDate DATE = GETDATE();

WHILE @Date <= @EndDate
BEGIN
    DECLARE @MaNV INT = 1;
    WHILE @MaNV <= 10
    BEGIN
        IF (ABS(CHECKSUM(NEWID())) % 100) < 55
        BEGIN
            DECLARE @CaId INT = (ABS(CHECKSUM(NEWID())) % 3) + 1;
            IF NOT EXISTS (SELECT 1 FROM NhanVien_Ca WHERE MaNV = @MaNV AND MaCa = @CaId AND NgayLamViec = @Date)
            BEGIN
                INSERT INTO NhanVien_Ca (MaNV, MaCa, NgayLamViec, TrangThaiDiemDanh)
                VALUES (@MaNV, @CaId, @Date, N'Đã hoàn thành');
            END
        END
        SET @MaNV = @MaNV + 1;
    END
    SET @Date = DATEADD(DAY, 1, @Date);
END

PRINT N'✓ Đã tạo lịch làm việc';

PRINT N'';
PRINT N'=== HOÀN TẤT ===';
SELECT COUNT(*) AS [Gói Dịch Vụ] FROM GoiDichVu;
SELECT COUNT(*) AS [Nhân Viên] FROM NhanVien;
SELECT COUNT(*) AS [Ca Làm Việc] FROM Ca;
SELECT COUNT(*) AS [Lịch Trực] FROM NhanVien_Ca;

GO

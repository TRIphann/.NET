-- =====================================================
-- CẬP NHẬT CA LÀM VIỆC VÀ LỊCH TRỰC NHÂN VIÊN
-- =====================================================

USE QLJumparena2;
GO

PRINT N'Bắt đầu cập nhật ca làm việc...';

-- Xóa dữ liệu cũ
DELETE FROM NhanVien_Ca;
PRINT N'✓ Đã xóa dữ liệu NhanVien_Ca cũ';

-- Cập nhật ca hiện có
UPDATE Ca SET TenCa = N'Ca Sáng', GioBatDau = '07:00:00', GioKetThuc = '12:00:00' WHERE MaCa = 1;
UPDATE Ca SET TenCa = N'Ca Chiều', GioBatDau = '12:00:00', GioKetThuc = '17:00:00' WHERE MaCa = 2;
UPDATE Ca SET TenCa = N'Ca Tối', GioBatDau = '17:00:00', GioKetThuc = '22:00:00' WHERE MaCa = 3;

-- Nếu chưa có ca, tạo mới
IF NOT EXISTS (SELECT 1 FROM Ca WHERE MaCa = 1)
    INSERT INTO Ca (TenCa, GioBatDau, GioKetThuc, SoLuongToiDa, TrangThai) 
    VALUES (N'Ca Sáng', '07:00:00', '12:00:00', 10, N'Hoạt động');

IF NOT EXISTS (SELECT 1 FROM Ca WHERE MaCa = 2)
    INSERT INTO Ca (TenCa, GioBatDau, GioKetThuc, SoLuongToiDa, TrangThai) 
    VALUES (N'Ca Chiều', '12:00:00', '17:00:00', 10, N'Hoạt động');

IF NOT EXISTS (SELECT 1 FROM Ca WHERE MaCa = 3)
    INSERT INTO Ca (TenCa, GioBatDau, GioKetThuc, SoLuongToiDa, TrangThai) 
    VALUES (N'Ca Tối', '17:00:00', '22:00:00', 10, N'Hoạt động');

PRINT N'✓ Đã cập nhật 3 ca làm việc';

-- Thêm dữ liệu mẫu cho 30 ngày gần đây
DECLARE @Date DATE = DATEADD(DAY, -30, GETDATE());
DECLARE @EndDate DATE = GETDATE();

WHILE @Date <= @EndDate
BEGIN
    -- Nhân viên 1 - Ca sáng và chiều
    IF NOT EXISTS (SELECT 1 FROM NhanVien_Ca WHERE MaNV = 1 AND MaCa = 1 AND NgayLamViec = @Date)
        INSERT INTO NhanVien_Ca (MaNV, MaCa, NgayLamViec, TrangThaiDiemDanh)
        VALUES (1, 1, @Date, N'Đã hoàn thành');
    
    IF NOT EXISTS (SELECT 1 FROM NhanVien_Ca WHERE MaNV = 1 AND MaCa = 2 AND NgayLamViec = @Date)
        INSERT INTO NhanVien_Ca (MaNV, MaCa, NgayLamViec, TrangThaiDiemDanh)
        VALUES (1, 2, @Date, N'Đã hoàn thành');
    
    -- Nhân viên 2 - Ca chiều và tối
    IF NOT EXISTS (SELECT 1 FROM NhanVien_Ca WHERE MaNV = 2 AND MaCa = 2 AND NgayLamViec = @Date)
        INSERT INTO NhanVien_Ca (MaNV, MaCa, NgayLamViec, TrangThaiDiemDanh)
        VALUES (2, 2, @Date, N'Đã hoàn thành');
    
    IF NOT EXISTS (SELECT 1 FROM NhanVien_Ca WHERE MaNV = 2 AND MaCa = 3 AND NgayLamViec = @Date)
        INSERT INTO NhanVien_Ca (MaNV, MaCa, NgayLamViec, TrangThaiDiemDanh)
        VALUES (2, 3, @Date, N'Đã hoàn thành');
    
    SET @Date = DATEADD(DAY, 1, @Date);
END

PRINT N'✓ Đã tạo lịch trực cho nhân viên';

-- Thống kê
DECLARE @TotalShifts INT = (SELECT COUNT(*) FROM NhanVien_Ca);
DECLARE @CaSang INT = (SELECT COUNT(*) FROM NhanVien_Ca WHERE MaCa = 1);
DECLARE @CaChieu INT = (SELECT COUNT(*) FROM NhanVien_Ca WHERE MaCa = 2);
DECLARE @CaToi INT = (SELECT COUNT(*) FROM NhanVien_Ca WHERE MaCa = 3);

PRINT N'';
PRINT N'=== THỐNG KÊ ===';
PRINT N'Tổng số ca đã xếp: ' + CAST(@TotalShifts AS NVARCHAR(10));
PRINT N'Ca Sáng: ' + CAST(@CaSang AS NVARCHAR(10));
PRINT N'Ca Chiều: ' + CAST(@CaChieu AS NVARCHAR(10));
PRINT N'Ca Tối: ' + CAST(@CaToi AS NVARCHAR(10));
PRINT N'';
PRINT N'✓ Hoàn tất cập nhật!';

-- Hiển thị mẫu dữ liệu
PRINT N'';
PRINT N'=== MẪU DỮ LIỆU (10 bản ghi gần nhất) ===';
SELECT TOP 10
    nv.HoTen AS [Nhân Viên],
    c.TenCa AS [Ca],
    CONVERT(VARCHAR(10), nc.NgayLamViec, 103) AS [Ngày],
    CONVERT(VARCHAR(5), c.GioBatDau, 108) AS [Giờ Bắt Đầu],
    CONVERT(VARCHAR(5), c.GioKetThuc, 108) AS [Giờ Kết Thúc],
    nc.TrangThaiDiemDanh AS [Trạng Thái]
FROM NhanVien_Ca nc
JOIN NhanVien nv ON nc.MaNV = nv.MaNV
JOIN Ca c ON nc.MaCa = c.MaCa
ORDER BY nc.NgayLamViec DESC, c.GioBatDau;

GO

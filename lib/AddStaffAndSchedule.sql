-- =====================================================
-- THÊM NHÂN VIÊN VÀ TẠO LỊCH LÀM VIỆC NGẪU NHIÊN
-- =====================================================

USE QLJumparena2;
GO

PRINT N'Bắt đầu thêm nhân viên và tạo lịch...';

-- Xóa lịch cũ
DELETE FROM NhanVien_Ca;
PRINT N'✓ Đã xóa lịch trực cũ';

-- Thêm 8 nhân viên mới (đã có 2 nhân viên)
-- Kiểm tra và thêm nhân viên
IF NOT EXISTS (SELECT 1 FROM NhanVien WHERE MaNV = 3)
BEGIN
    INSERT INTO NhanVien (HoTen, GioiTinh, NgaySinh, SDT, DiaChi, NgayVaoLam, TrangThai)
    VALUES (N'Lê Thị Cẩm', N'Nữ', '1998-05-15', '0901234570', N'123 Lê Lợi, Q1, TP.HCM', '2023-01-10', N'Đang làm việc');
END

IF NOT EXISTS (SELECT 1 FROM NhanVien WHERE MaNV = 4)
BEGIN
    INSERT INTO NhanVien (HoTen, GioiTinh, NgaySinh, SDT, DiaChi, NgayVaoLam, TrangThai)
    VALUES (N'Phạm Văn Dũng', N'Nam', '1995-08-22', '0901234571', N'456 Nguyễn Huệ, Q1, TP.HCM', '2023-02-15', N'Đang làm việc');
END

IF NOT EXISTS (SELECT 1 FROM NhanVien WHERE MaNV = 5)
BEGIN
    INSERT INTO NhanVien (HoTen, GioiTinh, NgaySinh, SDT, DiaChi, NgayVaoLam, TrangThai)
    VALUES (N'Hoàng Thị Lan', N'Nữ', '1997-11-30', '0901234572', N'789 Trần Hưng Đạo, Q5, TP.HCM', '2023-03-20', N'Đang làm việc');
END

IF NOT EXISTS (SELECT 1 FROM NhanVien WHERE MaNV = 6)
BEGIN
    INSERT INTO NhanVien (HoTen, GioiTinh, NgaySinh, SDT, DiaChi, NgayVaoLam, TrangThai)
    VALUES (N'Đặng Văn Hùng', N'Nam', '1996-03-12', '0901234573', N'321 Võ Văn Tần, Q3, TP.HCM', '2023-04-05', N'Đang làm việc');
END

IF NOT EXISTS (SELECT 1 FROM NhanVien WHERE MaNV = 7)
BEGIN
    INSERT INTO NhanVien (HoTen, GioiTinh, NgaySinh, SDT, DiaChi, NgayVaoLam, TrangThai)
    VALUES (N'Vũ Thị Mai', N'Nữ', '1999-07-18', '0901234574', N'654 Hai Bà Trưng, Q3, TP.HCM', '2023-05-12', N'Đang làm việc');
END

IF NOT EXISTS (SELECT 1 FROM NhanVien WHERE MaNV = 8)
BEGIN
    INSERT INTO NhanVien (HoTen, GioiTinh, NgaySinh, SDT, DiaChi, NgayVaoLam, TrangThai)
    VALUES (N'Bùi Văn Tài', N'Nam', '1994-12-25', '0901234575', N'987 Lý Thường Kiệt, Q10, TP.HCM', '2023-06-18', N'Đang làm việc');
END

IF NOT EXISTS (SELECT 1 FROM NhanVien WHERE MaNV = 9)
BEGIN
    INSERT INTO NhanVien (HoTen, GioiTinh, NgaySinh, SDT, DiaChi, NgayVaoLam, TrangThai)
    VALUES (N'Trương Thị Hoa', N'Nữ', '1998-09-08', '0901234576', N'147 Cách Mạng Tháng 8, Q10, TP.HCM', '2023-07-22', N'Đang làm việc');
END

IF NOT EXISTS (SELECT 1 FROM NhanVien WHERE MaNV = 10)
BEGIN
    INSERT INTO NhanVien (HoTen, GioiTinh, NgaySinh, SDT, DiaChi, NgayVaoLam, TrangThai)
    VALUES (N'Ngô Văn Khoa', N'Nam', '1997-04-14', '0901234577', N'258 Điện Biên Phủ, Q3, TP.HCM', '2023-08-30', N'Đang làm việc');
END

PRINT N'✓ Đã thêm nhân viên';

-- Tạo lịch làm việc ngẫu nhiên cho 30 ngày gần đây
DECLARE @StartDate DATE = DATEADD(DAY, -30, GETDATE());
DECLARE @EndDate DATE = GETDATE();
DECLARE @CurrentDate DATE = @StartDate;

PRINT N'Tạo lịch làm việc từ ' + CONVERT(NVARCHAR(10), @StartDate, 120) + ' đến ' + CONVERT(NVARCHAR(10), @EndDate, 120);

WHILE @CurrentDate <= @EndDate
BEGIN
    DECLARE @MaNV INT = 1;
    
    -- Duyệt qua 10 nhân viên
    WHILE @MaNV <= 10
    BEGIN
        -- Xác suất làm việc trong ngày (khoảng 50-70%)
        DECLARE @WorkToday INT = ABS(CHECKSUM(NEWID())) % 100;
        
        IF @WorkToday < 60  -- 60% khả năng làm việc
        BEGIN
            -- Random 1 ca trong 3 ca
            DECLARE @CaId INT = (ABS(CHECKSUM(NEWID())) % 3) + 1;
            
            -- Insert ca làm việc
            IF NOT EXISTS (SELECT 1 FROM NhanVien_Ca WHERE MaNV = @MaNV AND MaCa = @CaId AND NgayLamViec = @CurrentDate)
            BEGIN
                INSERT INTO NhanVien_Ca (MaNV, MaCa, NgayLamViec, TrangThaiDiemDanh)
                VALUES (@MaNV, @CaId, @CurrentDate, N'Đã hoàn thành');
            END
        END
        
        SET @MaNV = @MaNV + 1;
    END
    
    SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);
END

PRINT N'✓ Đã tạo lịch làm việc ngẫu nhiên';

-- Thống kê
DECLARE @TotalShifts INT = (SELECT COUNT(*) FROM NhanVien_Ca);
DECLARE @CaSang INT = (SELECT COUNT(*) FROM NhanVien_Ca WHERE MaCa = 1);
DECLARE @CaChieu INT = (SELECT COUNT(*) FROM NhanVien_Ca WHERE MaCa = 2);
DECLARE @CaToi INT = (SELECT COUNT(*) FROM NhanVien_Ca WHERE MaCa = 3);

PRINT N'';
PRINT N'=== THỐNG KÊ ===';
PRINT N'Tổng số nhân viên: 10';
PRINT N'Tổng số ca đã xếp: ' + CAST(@TotalShifts AS NVARCHAR(10));
PRINT N'Ca Sáng: ' + CAST(@CaSang AS NVARCHAR(10));
PRINT N'Ca Chiều: ' + CAST(@CaChieu AS NVARCHAR(10));
PRINT N'Ca Tối: ' + CAST(@CaToi AS NVARCHAR(10));

-- Thống kê theo nhân viên
PRINT N'';
PRINT N'=== THỐNG KÊ THEO NHÂN VIÊN ===';
SELECT 
    nv.MaNV,
    nv.HoTen AS [Nhân Viên],
    COUNT(*) AS [Tổng Ca],
    SUM(CASE WHEN nc.MaCa = 1 THEN 1 ELSE 0 END) AS [Ca Sáng],
    SUM(CASE WHEN nc.MaCa = 2 THEN 1 ELSE 0 END) AS [Ca Chiều],
    SUM(CASE WHEN nc.MaCa = 3 THEN 1 ELSE 0 END) AS [Ca Tối]
FROM NhanVien nv
LEFT JOIN NhanVien_Ca nc ON nv.MaNV = nc.MaNV
WHERE nv.MaNV <= 10
GROUP BY nv.MaNV, nv.HoTen
ORDER BY nv.MaNV;

PRINT N'';
PRINT N'✓ Hoàn tất!';

-- Hiển thị mẫu dữ liệu hôm nay
PRINT N'';
PRINT N'=== LỊCH LÀM VIỆC HÔM NAY ===';
SELECT 
    nv.HoTen AS [Nhân Viên],
    c.TenCa AS [Ca],
    CONVERT(VARCHAR(5), c.GioBatDau, 108) AS [Giờ Bắt Đầu],
    CONVERT(VARCHAR(5), c.GioKetThuc, 108) AS [Giờ Kết Thúc],
    nc.TrangThaiDiemDanh AS [Trạng Thái]
FROM NhanVien_Ca nc
JOIN NhanVien nv ON nc.MaNV = nv.MaNV
JOIN Ca c ON nc.MaCa = c.MaCa
WHERE nc.NgayLamViec = CAST(GETDATE() AS DATE)
ORDER BY c.GioBatDau, nv.HoTen;

GO

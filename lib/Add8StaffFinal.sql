-- =====================================================
-- THÊM 8 NHÂN VIÊN MỚI (FIX TenNV)
-- =====================================================

USE QLJumparena2;
GO

PRINT N'Thêm 8 nhân viên mới...';

-- Nhân viên 3
IF NOT EXISTS (SELECT 1 FROM NhanVien WHERE MaNV = 3)
BEGIN
    SET IDENTITY_INSERT NhanVien ON;
    INSERT INTO NhanVien (MaNV, TenNV, HoTen, GioiTinh, NgaySinh, SDT, DiaChi, Email)
    VALUES (3, N'Lê Thị Cẩm', N'Lê Thị Cẩm', N'Nữ', '1998-05-15', '0901234570', N'123 Lê Lợi, Q1, TP.HCM', 'lecam@jumparena.vn');
    SET IDENTITY_INSERT NhanVien OFF;
END

-- Nhân viên 4
IF NOT EXISTS (SELECT 1 FROM NhanVien WHERE MaNV = 4)
BEGIN
    SET IDENTITY_INSERT NhanVien ON;
    INSERT INTO NhanVien (MaNV, TenNV, HoTen, GioiTinh, NgaySinh, SDT, DiaChi, Email)
    VALUES (4, N'Phạm Văn Dũng', N'Phạm Văn Dũng', N'Nam', '1995-08-22', '0901234571', N'456 Nguyễn Huệ, Q1, TP.HCM', 'phamdung@jumparena.vn');
    SET IDENTITY_INSERT NhanVien OFF;
END

-- Nhân viên 5
IF NOT EXISTS (SELECT 1 FROM NhanVien WHERE MaNV = 5)
BEGIN
    SET IDENTITY_INSERT NhanVien ON;
    INSERT INTO NhanVien (MaNV, TenNV, HoTen, GioiTinh, NgaySinh, SDT, DiaChi, Email)
    VALUES (5, N'Hoàng Thị Lan', N'Hoàng Thị Lan', N'Nữ', '1997-11-30', '0901234572', N'789 Trần Hưng Đạo, Q5, TP.HCM', 'hoanglan@jumparena.vn');
    SET IDENTITY_INSERT NhanVien OFF;
END

-- Nhân viên 6
IF NOT EXISTS (SELECT 1 FROM NhanVien WHERE MaNV = 6)
BEGIN
    SET IDENTITY_INSERT NhanVien ON;
    INSERT INTO NhanVien (MaNV, TenNV, HoTen, GioiTinh, NgaySinh, SDT, DiaChi, Email)
    VALUES (6, N'Đặng Văn Hùng', N'Đặng Văn Hùng', N'Nam', '1996-03-12', '0901234573', N'321 Võ Văn Tần, Q3, TP.HCM', 'danghung@jumparena.vn');
    SET IDENTITY_INSERT NhanVien OFF;
END

-- Nhân viên 7
IF NOT EXISTS (SELECT 1 FROM NhanVien WHERE MaNV = 7)
BEGIN
    SET IDENTITY_INSERT NhanVien ON;
    INSERT INTO NhanVien (MaNV, TenNV, HoTen, GioiTinh, NgaySinh, SDT, DiaChi, Email)
    VALUES (7, N'Vũ Thị Mai', N'Vũ Thị Mai', N'Nữ', '1999-07-18', '0901234574', N'654 Hai Bà Trưng, Q3, TP.HCM', 'vumai@jumparena.vn');
    SET IDENTITY_INSERT NhanVien OFF;
END

-- Nhân viên 8
IF NOT EXISTS (SELECT 1 FROM NhanVien WHERE MaNV = 8)
BEGIN
    SET IDENTITY_INSERT NhanVien ON;
    INSERT INTO NhanVien (MaNV, TenNV, HoTen, GioiTinh, NgaySinh, SDT, DiaChi, Email)
    VALUES (8, N'Bùi Văn Tài', N'Bùi Văn Tài', N'Nam', '1994-12-25', '0901234575', N'987 Lý Thường Kiệt, Q10, TP.HCM', 'buitai@jumparena.vn');
    SET IDENTITY_INSERT NhanVien OFF;
END

-- Nhân viên 9
IF NOT EXISTS (SELECT 1 FROM NhanVien WHERE MaNV = 9)
BEGIN
    SET IDENTITY_INSERT NhanVien ON;
    INSERT INTO NhanVien (MaNV, TenNV, HoTen, GioiTinh, NgaySinh, SDT, DiaChi, Email)
    VALUES (9, N'Trương Thị Hoa', N'Trương Thị Hoa', N'Nữ', '1998-09-08', '0901234576', N'147 Cách Mạng Tháng 8, Q10, TP.HCM', 'truonghoa@jumparena.vn');
    SET IDENTITY_INSERT NhanVien OFF;
END

-- Nhân viên 10
IF NOT EXISTS (SELECT 1 FROM NhanVien WHERE MaNV = 10)
BEGIN
    SET IDENTITY_INSERT NhanVien ON;
    INSERT INTO NhanVien (MaNV, TenNV, HoTen, GioiTinh, NgaySinh, SDT, DiaChi, Email)
    VALUES (10, N'Ngô Văn Khoa', N'Ngô Văn Khoa', N'Nam', '1997-04-14', '0901234577', N'258 Điện Biên Phủ, Q3, TP.HCM', 'ngokhoa@jumparena.vn');
    SET IDENTITY_INSERT NhanVien OFF;
END

PRINT N'✓ Đã thêm 8 nhân viên';

-- Tạo lịch ngẫu nhiên
DECLARE @Date DATE = DATEADD(DAY, -30, GETDATE());
DECLARE @EndDate DATE = GETDATE();

WHILE @Date <= @EndDate
BEGIN
    DECLARE @MaNV INT = 3;
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

-- Thống kê
SELECT 
    nv.MaNV,
    nv.HoTen,
    nv.GioiTinh,
    COUNT(nc.MaCa) AS [Tổng Ca]
FROM NhanVien nv
LEFT JOIN NhanVien_Ca nc ON nv.MaNV = nc.MaNV
WHERE nv.MaNV <= 10
GROUP BY nv.MaNV, nv.HoTen, nv.GioiTinh
ORDER BY nv.MaNV;

GO

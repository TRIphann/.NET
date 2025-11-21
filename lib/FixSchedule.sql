-- =====================================================
-- TẠO LỊCH LÀM VIỆC NGẪU NHIÊN CHO 2 NHÂN VIÊN
-- =====================================================

USE QLJumparena2;
GO

PRINT N'Tạo lịch làm việc ngẫu nhiên...';

-- Xóa lịch cũ
DELETE FROM NhanVien_Ca;

-- Tạo lịch cho 30 ngày gần đây
DECLARE @Date DATE = DATEADD(DAY, -30, GETDATE());
DECLARE @EndDate DATE = GETDATE();

WHILE @Date <= @EndDate
BEGIN
    -- Nhân viên 1: 60% khả năng làm việc
    IF (ABS(CHECKSUM(NEWID())) % 100) < 60
    BEGIN
        DECLARE @Ca1 INT = (ABS(CHECKSUM(NEWID())) % 3) + 1;
        INSERT INTO NhanVien_Ca (MaNV, MaCa, NgayLamViec, TrangThaiDiemDanh)
        VALUES (1, @Ca1, @Date, N'Đã hoàn thành');
    END
    
    -- Nhân viên 2: 60% khả năng làm việc
    IF (ABS(CHECKSUM(NEWID())) % 100) < 60
    BEGIN
        DECLARE @Ca2 INT = (ABS(CHECKSUM(NEWID())) % 3) + 1;
        INSERT INTO NhanVien_Ca (MaNV, MaCa, NgayLamViec, TrangThaiDiemDanh)
        VALUES (2, @Ca2, @Date, N'Đã hoàn thành');
    END
    
    SET @Date = DATEADD(DAY, 1, @Date);
END

PRINT N'✓ Hoàn tất!';

-- Thống kê
SELECT 
    nv.HoTen AS [Nhân Viên],
    COUNT(*) AS [Tổng Ca],
    SUM(CASE WHEN nc.MaCa = 1 THEN 1 ELSE 0 END) AS [Ca Sáng],
    SUM(CASE WHEN nc.MaCa = 2 THEN 1 ELSE 0 END) AS [Ca Chiều],
    SUM(CASE WHEN nc.MaCa = 3 THEN 1 ELSE 0 END) AS [Ca Tối]
FROM NhanVien nv
LEFT JOIN NhanVien_Ca nc ON nv.MaNV = nc.MaNV
WHERE nv.MaNV IN (1, 2)
GROUP BY nv.HoTen;

GO

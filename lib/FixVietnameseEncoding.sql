-- =====================================================
-- FIX VIETNAMESE ENCODING - CẬP NHẬT LẠI DỮ LIỆU
-- =====================================================

USE QLJumparena2;
GO

PRINT N'Bắt đầu fix encoding tiếng Việt...';

-- Cập nhật lại tên gói dịch vụ
UPDATE GoiDichVu SET TenGoi = N'Gói Cơ Bản 1 Giờ' WHERE MaGoi = 1;
UPDATE GoiDichVu SET TenGoi = N'Gói Tiêu Chuẩn 2 Giờ' WHERE MaGoi = 2;
UPDATE GoiDichVu SET TenGoi = N'Gói VIP 3 Giờ' WHERE MaGoi = 3;
UPDATE GoiDichVu SET TenGoi = N'Gói Premium Cả Ngày' WHERE MaGoi = 4;
UPDATE GoiDichVu SET TenGoi = N'Gói Gia Đình 4 Người' WHERE MaGoi = 5;
UPDATE GoiDichVu SET TenGoi = N'Gói Học Sinh Sinh Viên' WHERE MaGoi = 6;
UPDATE GoiDichVu SET TenGoi = N'Gói Cuối Tuần Ảo Diệu' WHERE MaGoi = 7;
UPDATE GoiDichVu SET TenGoi = N'Gói 10 Lượt Chơi' WHERE MaGoi = 8;
UPDATE GoiDichVu SET TenGoi = N'Gói 20 Lượt Chơi' WHERE MaGoi = 9;
UPDATE GoiDichVu SET TenGoi = N'Gói Nhóm 10 Người' WHERE MaGoi = 10;

PRINT N'✓ Đã cập nhật tên gói dịch vụ';

-- Cập nhật tên nhân viên
UPDATE NhanVien SET HoTen = N'Nguyễn Văn A', TenNV = N'Nguyễn Văn A' WHERE MaNV = 1;
UPDATE NhanVien SET HoTen = N'Trần Thảo B', TenNV = N'Trần Thảo B' WHERE MaNV = 2;
UPDATE NhanVien SET HoTen = N'Lê Thị Cẩm', TenNV = N'Lê Thị Cẩm' WHERE MaNV = 3;
UPDATE NhanVien SET HoTen = N'Phạm Văn Dũng', TenNV = N'Phạm Văn Dũng' WHERE MaNV = 4;
UPDATE NhanVien SET HoTen = N'Hoàng Thị Lan', TenNV = N'Hoàng Thị Lan' WHERE MaNV = 5;
UPDATE NhanVien SET HoTen = N'Đặng Văn Hùng', TenNV = N'Đặng Văn Hùng' WHERE MaNV = 6;
UPDATE NhanVien SET HoTen = N'Vũ Thị Mai', TenNV = N'Vũ Thị Mai' WHERE MaNV = 7;
UPDATE NhanVien SET HoTen = N'Bùi Văn Tài', TenNV = N'Bùi Văn Tài' WHERE MaNV = 8;
UPDATE NhanVien SET HoTen = N'Trương Thị Hoa', TenNV = N'Trương Thị Hoa' WHERE MaNV = 9;
UPDATE NhanVien SET HoTen = N'Ngô Văn Khoa', TenNV = N'Ngô Văn Khoa' WHERE MaNV = 10;

PRINT N'✓ Đã cập nhật tên nhân viên';

-- Cập nhật tên ca
UPDATE Ca SET TenCa = N'Ca Sáng' WHERE MaCa = 1;
UPDATE Ca SET TenCa = N'Ca Chiều' WHERE MaCa = 2;
UPDATE Ca SET TenCa = N'Ca Tối' WHERE MaCa = 3;

PRINT N'✓ Đã cập nhật tên ca';

-- Cập nhật tên khách hàng (nếu có)
UPDATE KhachHang SET TenKH = N'Nguyễn Văn A' WHERE MaKH = 1 AND TenKH LIKE '%Nguy%n%';
UPDATE KhachHang SET TenKH = N'Trần Thị B' WHERE MaKH = 2 AND TenKH LIKE '%Tr%n%';

PRINT N'✓ Đã cập nhật tên khách hàng';

PRINT N'';
PRINT N'=== HOÀN TẤT ===';
PRINT N'Đã fix encoding tiếng Việt cho tất cả dữ liệu!';

-- Kiểm tra kết quả
PRINT N'';
PRINT N'=== KIỂM TRA KẾT QUẢ ===';
SELECT TOP 5 MaGoi, TenGoi, Gia FROM GoiDichVu;
SELECT TOP 5 MaNV, HoTen FROM NhanVien;
SELECT MaCa, TenCa FROM Ca;

GO

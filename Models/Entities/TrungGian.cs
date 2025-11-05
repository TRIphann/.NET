using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    // ============================
    // Bảng trung gian KhachHang_Tour
    // ============================
    public class KhachHang_Tour
    {
        [Key, Column(Order = 0)]
        public int MaKH { get; set; }

        [Key, Column(Order = 1)]
        public int MaTour { get; set; }

        public int SoLuongVe { get; set; }

        [ForeignKey("MaKH")]
        public KhachHang? KhachHang { get; set; }

        [ForeignKey("MaTour")]
        public Tour? Tour { get; set; }
    }

    // ============================
    // Bảng trung gian NhanVien_Tour
    // ============================
    public class NhanVien_Tour
    {
        // ✅ SỬA: Khóa chính phức hợp (MaNV, MaTour)
        [Key, Column(Order = 0)]
        public int MaNV { get; set; }

        [Key, Column(Order = 1)]
        public int MaTour { get; set; }

        // Khóa ngoại liên kết với bảng NhanVien
        [ForeignKey("MaNV")]
        public NhanVien? NhanVien { get; set; }

        // Khóa ngoại liên kết với bảng Tour
        [ForeignKey("MaTour")]
        public Tour? Tour { get; set; }
    }

    // ============================
    // Bảng trung gian DichVuDaDangKy
    // ============================
    public class DichVuDaDangKy
    {
        [Key, Column(Order = 0)]
        public int MaKH { get; set; }

        [Key, Column(Order = 1)]
        public int MaTour { get; set; }

        [Key, Column(Order = 2)]
        public int MaDV { get; set; }

        [ForeignKey("MaKH")]
        public KhachHang? KhachHang { get; set; }

        [ForeignKey("MaTour")]
        public Tour? Tour { get; set; }

        [ForeignKey("MaDV")]
        public DichVuDiKem? DichVuDiKem { get; set; }
    }

    // ============================
    // Bảng Tour_Anh
    // ============================
    public class Tour_Anh
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaAnh { get; set; }

        public int MaTour { get; set; }

        [Required, StringLength(255)]
        public string DuongDan { get; set; } = string.Empty;

        [ForeignKey("MaTour")]
        public Tour? Tour { get; set; }
    }

    // ============================
    // Bảng Tour_PhuongTien
    // ============================
    public class Tour_PhuongTien
    {
        [Key, Column(Order = 0)]
        public int MaTour { get; set; }

        [Key, Column(Order = 1)]
        public int MaPT { get; set; }

        [ForeignKey("MaTour")]
        public Tour? Tour { get; set; }

        [ForeignKey("MaPT")]
        public PhuongTien? PhuongTien { get; set; }
    }

    // ============================
    // Bảng Tour_DiaDiem
    // ============================
    public class Tour_DiaDiem
    {
        [Key, Column(Order = 0)]
        public int MaTour { get; set; }

        [Key, Column(Order = 1)]
        public int MaDD { get; set; }

        [ForeignKey("MaTour")]
        public Tour? Tour { get; set; }

        [ForeignKey("MaDD")]
        public DiaDiem? DiaDiem { get; set; }
    }

    // ============================
    // Bảng Tour_LichTrinhCT
    // ============================
    public class Tour_LichTrinhCT
    {
        [Key, Column(Order = 0)]
        public int MaTour { get; set; }

        [Key, Column(Order = 1)]
        public int MaLT { get; set; }

        [ForeignKey("MaTour")]
        public Tour? Tour { get; set; }

        [ForeignKey("MaLT")]
        public LichTrinhCT? LichTrinhCT { get; set; }
    }
}

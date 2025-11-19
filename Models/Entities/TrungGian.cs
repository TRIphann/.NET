using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    // ====================================
    // KHU VUI CHƠI - TRÒ CHƠI
    // ====================================
    [Table("KhuVuiChoi_TroChoi")]
    public class KhuVuiChoi_TroChoi
    {
        [Key, Column(Order = 0)]
        public int MaKhu { get; set; }

        [Key, Column(Order = 1)]
        public int MaTroChoi { get; set; }

        [StringLength(500)]
        public string? ViTri { get; set; }

        [StringLength(500)]
        public string? GhiChu { get; set; }

        [ForeignKey("MaKhu")]
        public KhuVuiChoi? KhuVuiChoi { get; set; }

        [ForeignKey("MaTroChoi")]
        public TroChoi? TroChoi { get; set; }
    }

    // ====================================
    // GÓI DỊCH VỤ - TRÒ CHƠI
    // ====================================
    [Table("GoiDichVu_TroChoi")]
    public class GoiDichVu_TroChoi
    {
        [Key, Column(Order = 0)]
        public int MaGoi { get; set; }

        [Key, Column(Order = 1)]
        public int MaTroChoi { get; set; }

        [StringLength(500)]
        public string? GhiChu { get; set; }

        [ForeignKey("MaGoi")]
        public GoiDichVu? GoiDichVu { get; set; }

        [ForeignKey("MaTroChoi")]
        public TroChoi? TroChoi { get; set; }
    }

    // ====================================
    // VÉ - DỊCH VỤ THÊM
    // ====================================
    [Table("Ve_DichVuThem")]
    public class Ve_DichVuThem
    {
        [Key, Column(Order = 0)]
        public int MaVe { get; set; }

        [Key, Column(Order = 1)]
        public int MaDVThem { get; set; }

        public int SoLuong { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ThanhTien { get; set; }

        [ForeignKey("MaVe")]
        public Ve? Ve { get; set; }

        [ForeignKey("MaDVThem")]
        public DichVuThem? DichVuThem { get; set; }
    }

    // ====================================
    // NHÂN VIÊN - CA
    // ====================================
    [Table("NhanVien_Ca")]
    public class NhanVien_Ca
    {
        [Key, Column(Order = 0)]
        public int MaNV { get; set; }

        [Key, Column(Order = 1)]
        public int MaCa { get; set; }

        [Key, Column(Order = 2)]
        [Required]
        public DateTime NgayLamViec { get; set; }

        [Column("ThoiGianCheckIn")]
        public DateTime? ThoiGianCheckIn { get; set; }

        [Column("ThoiGianCheckOut")]
        public DateTime? ThoiGianCheckOut { get; set; }

        [StringLength(50)]
        [Column("TrangThaiDiemDanh")]
        public string TrangThaiDiemDanh { get; set; } = "Chưa điểm danh";

        [Column("DiMuonCoPhep")]
        public bool DiMuonCoPhep { get; set; } = false;

        [StringLength(500)]
        [Column("GhiChu")]
        public string? GhiChu { get; set; }

        [ForeignKey("MaNV")]
        public NhanVien? NhanVien { get; set; }

        [ForeignKey("MaCa")]
        public Ca? Ca { get; set; }
    }

    // ====================================
    // TRÒ CHƠI - ẢNH
    // ====================================
    [Table("TroChoi_Anh")]
    public class TroChoi_Anh
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaAnh { get; set; }

        [Required]
        public int MaTroChoi { get; set; }

        [Required]
        [StringLength(500)]
        public string DuongDan { get; set; } = string.Empty;

        [StringLength(200)]
        public string? MoTa { get; set; }

        [ForeignKey("MaTroChoi")]
        public TroChoi? TroChoi { get; set; }
    }

    // ====================================
    // KHU VUI CHƠI - ẢNH
    // ====================================
    [Table("KhuVuiChoi_Anh")]
    public class KhuVuiChoi_Anh
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaAnh { get; set; }

        [Required]
        public int MaKhu { get; set; }

        [Required]
        [StringLength(500)]
        public string DuongDan { get; set; } = string.Empty;

        [StringLength(200)]
        public string? MoTa { get; set; }

        [ForeignKey("MaKhu")]
        public KhuVuiChoi? KhuVuiChoi { get; set; }
    }
}

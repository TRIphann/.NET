#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    [Table("TroChoi")]
    public class TroChoi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaTroChoi { get; set; }

        [Required]
        [StringLength(200)]
        public string TenTroChoi { get; set; } = string.Empty;

        [StringLength(100)]
        public string? DanhMuc { get; set; }  // Trampoline, Thử thách, Kids Zone, etc.

        [StringLength(1000)]
        public string? MoTa { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DoKho { get; set; }  // Độ khó từ 1-5

        public int? DoTuoiToiThieu { get; set; }

        public int? DoTuoiToiDa { get; set; }

        public int? SoNguoiToiDa { get; set; }

        [StringLength(500)]
        public string? YeuCauAnToan { get; set; }

        [StringLength(50)]
        public string? TrangThai { get; set; }  // Hoạt động, Bảo trì

        public DateTime? NgayCapNhat { get; set; }

        // Navigation properties
        public ICollection<KhuVuiChoi_TroChoi>? KhuVuiChoi_TroChoi { get; set; }
        public ICollection<GoiDichVu_TroChoi>? GoiDichVu_TroChoi { get; set; }
        public ICollection<TroChoi_Anh>? TroChoi_Anh { get; set; }
    }
}

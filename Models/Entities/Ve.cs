#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    [Table("Ve")]
    public class Ve
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaVe { get; set; }

        [StringLength(50)]
        public string? MaVeCode { get; set; }

        [Required]
        public int MaKH { get; set; }

        [Required]
        public int MaGoi { get; set; }

        public int? MaCa { get; set; }

        public int? MaHD { get; set; }

        [Required]
        public DateTime NgayDat { get; set; } = DateTime.Now;

        [Required]
        public DateTime NgaySuDung { get; set; }

        public int SoNguoi { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTien { get; set; }

        [StringLength(50)]
        public string? TrangThai { get; set; }  // Đã đặt, Đã check-in, Đã sử dụng, Đã hủy

        public DateTime? NgayCheckIn { get; set; }

        public string? GhiChu { get; set; }

        // Foreign Keys
        [ForeignKey("MaKH")]
        public KhachHang? KhachHang { get; set; }

        [ForeignKey("MaGoi")]
        public GoiDichVu? GoiDichVu { get; set; }

        [ForeignKey("MaCa")]
        public Ca? Ca { get; set; }

        [ForeignKey("MaHD")]
        public HoaDon? HoaDon { get; set; }

        // Navigation properties
        public ICollection<Ve_DichVuThem>? Ve_DichVuThem { get; set; }
    }
}

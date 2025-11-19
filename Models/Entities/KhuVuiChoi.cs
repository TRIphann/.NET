using System;
using System.Collections.Generic;
#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    [Table("KhuVuiChoi")]
    public class KhuVuiChoi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaKhu { get; set; }

        [Required]
        [StringLength(200)]
        public string TenKhu { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? MoTa { get; set; }

        [StringLength(500)]
        public string? DiaChi { get; set; }

        [StringLength(20)]
        public string? SDT { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        public TimeOnly? GioMoCua { get; set; }

        public TimeOnly? GioDongCua { get; set; }

        [StringLength(50)]
        public string? TrangThai { get; set; }  // Đang hoạt động, Bảo trì, Đóng cửa

        // Navigation properties
        public ICollection<KhuVuiChoi_TroChoi>? KhuVuiChoi_TroChoi { get; set; }
        public ICollection<KhuVuiChoi_Anh>? KhuVuiChoi_Anh { get; set; }
    }
}

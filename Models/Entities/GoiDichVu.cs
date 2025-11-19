using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    [Table("GoiDichVu")]
    public class GoiDichVu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaGoi { get; set; }

        [Required]
        [StringLength(200)]
        public string TenGoi { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? MoTa { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Gia { get; set; }

        public int? ThoiGian { get; set; }  // Thời gian sử dụng (phút)

        public int? SoLuotChoi { get; set; }

        [StringLength(50)]
        public string? TrangThai { get; set; }  // Đang bán, Ngừng bán

        // Navigation properties
        public ICollection<GoiDichVu_TroChoi>? GoiDichVu_TroChoi { get; set; }
        public ICollection<Ve>? Ve { get; set; }
    }
}

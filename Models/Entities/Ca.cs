using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    [Table("Ca")]
    public class Ca
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaCa { get; set; }

        [Required]
        [StringLength(100)]
        public string TenCa { get; set; } = string.Empty;

        [Required]
        [Column("ThoiGianBatDau")]
        public TimeOnly GioBatDau { get; set; }

        [Required]
        [Column("ThoiGianKetThuc")]
        public TimeOnly GioKetThuc { get; set; }

        [StringLength(1000)]
        public string? MoTa { get; set; }

        // Computed properties for backward compatibility
        [NotMapped]
        public int? SoLuongToiDa { get; set; }
        
        [NotMapped]
        public string? TrangThai { get; set; }

        // Navigation properties
        public ICollection<NhanVien_Ca>? NhanVien_Ca { get; set; }
    }
}

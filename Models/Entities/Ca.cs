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
        public TimeOnly GioBatDau { get; set; }

        [Required]
        public TimeOnly GioKetThuc { get; set; }

        // Computed properties for backward compatibility
        [NotMapped]
        public int? SoLuongToiDa { get; set; }
        
        [NotMapped]
        public string? TrangThai { get; set; }

        // Navigation properties
        public ICollection<NhanVien_Ca>? NhanVien_Ca { get; set; }
        public ICollection<Ve>? Ve { get; set; }
    }
}

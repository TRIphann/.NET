using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    [Table("DichVuThem")]
    public class DichVuThem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaDVThem { get; set; }

        [Required]
        [StringLength(200)]
        public string TenDV { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? MoTa { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DonGia { get; set; }

        [StringLength(50)]
        public string? LoaiDV { get; set; }  // Đồ ăn, Đồ uống, Phụ kiện, Khác

        [StringLength(50)]
        public string? TrangThai { get; set; }  // Còn hàng, Hết hàng

        // Navigation properties
        public ICollection<Ve_DichVuThem>? Ve_DichVuThem { get; set; }
    }
}

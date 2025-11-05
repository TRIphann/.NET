using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    public class DichVuDiKem
    {
        [Key] 
        public int MaDV { get; set; }

        [Required, StringLength(100)] 
        public string TenDV { get; set; } = string.Empty;

        [Column("Gia")]
        public decimal DonGia { get; set; }

        [StringLength(255)]
        [Column("LoaiDV")]
        public string? MoTa { get; set; } = string.Empty;
    }
}

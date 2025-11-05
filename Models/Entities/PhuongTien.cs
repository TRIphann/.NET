using System.ComponentModel.DataAnnotations;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    public class PhuongTien
    {
        [Key] public int MaPT { get; set; }

        [Required, StringLength(100)] public string TenPT { get; set; } = string.Empty;

        [StringLength(50)] public string? LoaiPT { get; set; } = string.Empty;

        public int SoCho { get; set; }
    }
}

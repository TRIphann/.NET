using System.ComponentModel.DataAnnotations;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    public class DiaDiem
    {
        [Key] public int MaDD { get; set; }

        [Required, StringLength(100)] public string TenDD { get; set; } = string.Empty;

        [StringLength(100)] public string? Tinh { get; set; } = string.Empty;
    }
}

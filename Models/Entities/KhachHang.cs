using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    public class KhachHang
    {
        [Key] public int MaKH { get; set; }

        [Required, StringLength(100)] public string TenKH { get; set; } = string.Empty;

        [Required, StringLength(10)] public string GioiTinh { get; set; } = string.Empty;

        [Required, StringLength(15)] public string SDT { get; set; } = string.Empty;

        [StringLength(255)] public string? DiaChi { get; set; } = string.Empty;

        [StringLength(20)] public string? CCCD { get; set; } = string.Empty;

        [StringLength(255)] public string? AnhDaiDien { get; set; }

        public int? UserId { get; set; }
        [ForeignKey("UserId")] public User? User { get; set; }
    }
}

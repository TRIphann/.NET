using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    public class NhanVien
    {
        [Key] public int MaNV { get; set; }

        [Required, StringLength(100)] public string TenNV { get; set; } = string.Empty;

        [Required, StringLength(10)] public string GioiTinh { get; set; } = string.Empty;

        [StringLength(255)] public string? DiaChi { get; set; } = string.Empty;

        [StringLength(15)] public string? SDT { get; set; } = string.Empty;

        [StringLength(50)] public string? VaiTro { get; set; } = string.Empty;

        [StringLength(255)] public string? AnhDaiDien { get; set; }

        public int? UserId { get; set; }
        [ForeignKey("UserId")] public User? User { get; set; }

        public ICollection<NhanVien_Tour> NhanVien_Tour { get; set; }
    }
}

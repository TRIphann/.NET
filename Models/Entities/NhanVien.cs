using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    [Table("NhanVien")]
    public class NhanVien
    {
        [Key] 
        public int MaNV { get; set; }

        [Required, StringLength(100)] 
        public string HoTen { get; set; } = string.Empty;

        [StringLength(10)]
        public string? GioiTinh { get; set; }

        [StringLength(15)] 
        public string SDT { get; set; } = string.Empty;

        [StringLength(100)] 
        public string? Email { get; set; }

        public DateTime? NgaySinh { get; set; }

        [StringLength(255)] 
        public string? DiaChi { get; set; }

        [StringLength(500)]
        public string? AnhDaiDien { get; set; }

        public int? UserId { get; set; }
        
        [ForeignKey("UserId")] 
        public User? User { get; set; }

        // Navigation properties
        public ICollection<NhanVien_Ca>? NhanVien_Ca { get; set; }
    }
}

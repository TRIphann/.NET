using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    [Table("DangKyLichTruc")]
    public class DangKyLichTruc
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaDangKy { get; set; }

        [Required]
        public int MaNV { get; set; }

        [Required]
        public int MaCa { get; set; }

        [Required]
        public DateTime NgayDangKy { get; set; }

        [Required]
        public DateTime NgayTruc { get; set; }

        [Required]
        public DateTime ThoiGianDangKy { get; set; }

        [StringLength(50)]
        [Required]
        public string TrangThai { get; set; } = "Chờ duyệt";

        [StringLength(500)]
        public string? GhiChu { get; set; }

        [ForeignKey("MaNV")]
        public NhanVien? NhanVien { get; set; }

        [ForeignKey("MaCa")]
        public Ca? Ca { get; set; }
    }
}

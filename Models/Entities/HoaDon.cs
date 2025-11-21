using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    [Table("HoaDon")]
    public class HoaDon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaHD { get; set; }

        [Required]
        public int MaKH { get; set; }

        [Required]
        [Column("NgayTao")]
        public DateTime NgayTT { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTien { get; set; }

        [Required]
        [StringLength(50)]
        public string HinhThucTT { get; set; } = "Tiền mặt";  // Tiền mặt, Chuyển khoản, QR Code, Thẻ

        [StringLength(20)]
        [Column("TrangThaiTT")]
        public string? TrangThai { get; set; } = "Đã thanh toán";  // Chờ thanh toán, Đã thanh toán, Đã hủy

        [StringLength(100)]
        public string? MaGiaoDich { get; set; }

        // Foreign Keys
        [ForeignKey("MaKH")]
        public KhachHang? KhachHang { get; set; }
    }
}

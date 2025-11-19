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
        public int MaVe { get; set; }

        [Required]
        [Column("NgayLap")]
        public DateTime NgayTT { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTien { get; set; }

        [Required]
        [StringLength(50)]
        [Column("PhuongThucThanhToan")]
        public string HinhThucTT { get; set; } = "Tiền mặt";  // Tiền mặt, Chuyển khoản, Thẻ, Ví điện tử

        [StringLength(50)]
        public string? TrangThai { get; set; } = "Đã thanh toán";  // Đã thanh toán, Chưa thanh toán, Đã hoàn tiền

        // Foreign Keys
        [ForeignKey("MaVe")]
        public Ve? Ve { get; set; }
        
        // Computed property for backward compatibility
        [NotMapped]
        public int MaKH => Ve?.MaKH ?? 0;
        
        [NotMapped]
        public KhachHang? KhachHang => Ve?.KhachHang;
    }
}

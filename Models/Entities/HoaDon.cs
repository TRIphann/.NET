using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    public class HoaDon
    {
        [Key] public int MaHD { get; set; }

        public int MaKH { get; set; }
        [ForeignKey("MaKH")] public KhachHang? KhachHang { get; set; }

        public int MaTour { get; set; }
        [ForeignKey("MaTour")] public Tour? Tour { get; set; }

        [StringLength(50)] public string HinhThucTT { get; set; } = "Tiền mặt";

        public DateTime NgayTT { get; set; } = DateTime.Now;

        public decimal TongTien { get; set; }
    }
}

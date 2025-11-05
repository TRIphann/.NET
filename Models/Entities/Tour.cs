using System.ComponentModel.DataAnnotations;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    public class Tour
    {
        [Key] public int MaTour { get; set; }

        [Required, StringLength(100)] public string TenTour { get; set; } = string.Empty;

        [Required] public DateTime NgayBatDau { get; set; }

        [Required] public DateTime NgayKetThuc { get; set; }

        [Required] public decimal Gia { get; set; }

        public string? AnhDaiDien { get; set; }

        public string? MoTa { get; set; }

        public ICollection<NhanVien_Tour> NhanVien_Tour { get; set; }
        public ICollection<Tour_Anh>? Tour_Anh { get; set; }
        public ICollection<Tour_PhuongTien> Tour_PhuongTien { get; set; }
        public ICollection<Tour_DiaDiem> Tour_DiaDiem { get; set; }
        public ICollection<Tour_LichTrinhCT> Tour_LichTrinhCT { get; set; }
    }
}

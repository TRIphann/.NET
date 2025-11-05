using System.ComponentModel.DataAnnotations;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
	public class LichTrinhCT
	{
		[Key]
		public int MaLT { get; set; }

		[Required, StringLength(255)]
		public string NoiDung { get; set; } = string.Empty;

		[Required]
		public int NgayThu { get; set; }

		// Quan hệ ngược: 1 Lịch trình có thể thuộc nhiều Tour_LichTrinhCT
		public ICollection<Tour_LichTrinhCT>? Tour_LichTrinhCTs { get; set; }
	}
}

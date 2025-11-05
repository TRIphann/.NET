using System.ComponentModel.DataAnnotations;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    public class User
    {
        [Key] public int UserId { get; set; }

        [Required, StringLength(100)] public string Username { get; set; } = string.Empty;

        [Required, StringLength(128)] public string PasswordHash { get; set; } = string.Empty;

        [Required, StringLength(150)] public string FullName { get; set; } = string.Empty;

        [StringLength(150)] public string? Email { get; set; } = string.Empty;

        [StringLength(20)] public string? Phone { get; set; } = string.Empty;

        [Required, StringLength(50)] public string Role { get; set; } = string.Empty;
        
        public NhanVien? NhanVien { get; set; }
    }
}

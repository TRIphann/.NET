using Microsoft.EntityFrameworkCore;
using QLDuLichRBAC_Upgrade.Models.Entities;

namespace QLDuLichRBAC_Upgrade.Models
{
    public class QLDuLichContext : DbContext
    {
        public QLDuLichContext(DbContextOptions<QLDuLichContext> options)
            : base(options)
        {
        }

        // ========================
        // Các bảng chính
        // ========================
        public DbSet<User> Users { get; set; }
        public DbSet<KhachHang> KhachHang { get; set; }
        public DbSet<NhanVien> NhanVien { get; set; }
        public DbSet<Tour> Tour { get; set; }
        public DbSet<PhuongTien> PhuongTien { get; set; }
        public DbSet<DichVuDiKem> DichVuDiKem { get; set; }
        public DbSet<HoaDon> HoaDon { get; set; }
        public DbSet<DiaDiem> DiaDiem { get; set; }
        public DbSet<DichVuDaDangKy> DichVuDaDangKy { get; set; }
        public DbSet<LichTrinhCT> LichTrinhCT { get; set; }

        // ========================
        // Các bảng trung gian
        // ========================
        public DbSet<KhachHang_Tour> KhachHang_Tour { get; set; }
        public DbSet<NhanVien_Tour> NhanVien_Tour { get; set; }
        public DbSet<Tour_Anh> Tour_Anh { get; set; }
        public DbSet<Tour_PhuongTien> Tour_PhuongTien { get; set; }
        public DbSet<Tour_DiaDiem> Tour_DiaDiem { get; set; }
        public DbSet<Tour_LichTrinhCT> Tour_LichTrinhCT { get; set; }

        // ========================
        // Cấu hình quan hệ
        // ========================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ====== Cấu hình tên bảng ======
            modelBuilder.Entity<User>().ToTable("User");

            // ====== Khóa chính phức hợp ======
            modelBuilder.Entity<KhachHang_Tour>().HasKey(k => new { k.MaKH, k.MaTour });
            modelBuilder.Entity<NhanVien_Tour>().HasKey(n => new { n.MaNV, n.MaTour });
            modelBuilder.Entity<DichVuDaDangKy>().HasKey(x => new { x.MaKH, x.MaTour, x.MaDV });
            modelBuilder.Entity<Tour_Anh>().HasKey(t => t.MaAnh);
            modelBuilder.Entity<Tour_PhuongTien>().HasKey(x => new { x.MaTour, x.MaPT });
            modelBuilder.Entity<Tour_DiaDiem>().HasKey(x => new { x.MaTour, x.MaDD });

            // ====== Cấu hình quan hệ ======

            // Nhân viên - User (1-1)
            modelBuilder.Entity<NhanVien>()
                .HasOne(nv => nv.User)
                .WithOne(u => u.NhanVien)
                .HasForeignKey<NhanVien>(nv => nv.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // NhanVien - Tour (n-n) ✅ SỬA: Cập nhật tên property navigation
            modelBuilder.Entity<NhanVien_Tour>()
                .HasOne(nt => nt.Tour)
                .WithMany(t => t.NhanVien_Tour)
                .HasForeignKey(nt => nt.MaTour)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NhanVien_Tour>()
                .HasOne(nt => nt.NhanVien)
                .WithMany(nv => nv.NhanVien_Tour)
                .HasForeignKey(nt => nt.MaNV)
                .OnDelete(DeleteBehavior.Cascade);

            // Tour - Ảnh
            modelBuilder.Entity<Tour_Anh>()
                .HasOne(t => t.Tour)
                .WithMany(t => t.Tour_Anh)
                .HasForeignKey(t => t.MaTour)
                .OnDelete(DeleteBehavior.Cascade);

            // Tour - Phương tiện
            modelBuilder.Entity<Tour_PhuongTien>()
                .HasOne(tp => tp.Tour)
                .WithMany(t => t.Tour_PhuongTien)
                .HasForeignKey(tp => tp.MaTour)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Tour_PhuongTien>()
                .HasOne(tp => tp.PhuongTien)
                .WithMany()
                .HasForeignKey(tp => tp.MaPT)
                .OnDelete(DeleteBehavior.Cascade);

            // Tour - Địa điểm
            modelBuilder.Entity<Tour_DiaDiem>()
                .HasOne(td => td.Tour)
                .WithMany(t => t.Tour_DiaDiem)
                .HasForeignKey(td => td.MaTour)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Tour_DiaDiem>()
                .HasOne(td => td.DiaDiem)
                .WithMany()
                .HasForeignKey(td => td.MaDD)
                .OnDelete(DeleteBehavior.Cascade);

            // Tour - Lịch trình
            modelBuilder.Entity<Tour_LichTrinhCT>()
                .HasKey(lt => new { lt.MaTour, lt.MaLT });

            modelBuilder.Entity<Tour_LichTrinhCT>()
                .HasOne(lt => lt.Tour)
                .WithMany(t => t.Tour_LichTrinhCT)
                .HasForeignKey(lt => lt.MaTour)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Tour_LichTrinhCT>()
                .HasOne(lt => lt.LichTrinhCT)
                .WithMany(l => l.Tour_LichTrinhCTs)
                .HasForeignKey(lt => lt.MaLT)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

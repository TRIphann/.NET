using Microsoft.EntityFrameworkCore;
using QLDuLichRBAC_Upgrade.Models.Entities;

namespace QLDuLichRBAC_Upgrade.Models
{
    public class QLJumaparenaContext : DbContext
    {
        public QLJumaparenaContext(DbContextOptions<QLJumaparenaContext> options)
            : base(options)
        {
        }

        // ========================
        // Bảng chính
        // ========================
        public DbSet<User> User { get; set; }
        public DbSet<KhachHang> KhachHang { get; set; }
        public DbSet<NhanVien> NhanVien { get; set; }
        public DbSet<KhuVuiChoi> KhuVuiChoi { get; set; }
        public DbSet<TroChoi> TroChoi { get; set; }
        public DbSet<GoiDichVu> GoiDichVu { get; set; }
        public DbSet<Ca> Ca { get; set; }
        public DbSet<Ve> Ve { get; set; }
        public DbSet<HoaDon> HoaDon { get; set; }
        public DbSet<DichVuThem> DichVuThem { get; set; }

        // ========================
        // Bảng trung gian
        // ========================
        public DbSet<KhuVuiChoi_TroChoi> KhuVuiChoi_TroChoi { get; set; }
        public DbSet<GoiDichVu_TroChoi> GoiDichVu_TroChoi { get; set; }
        public DbSet<Ve_DichVuThem> Ve_DichVuThem { get; set; }
        public DbSet<NhanVien_Ca> NhanVien_Ca { get; set; }
        public DbSet<DangKyLichTruc> DangKyLichTruc { get; set; }
        public DbSet<TroChoi_Anh> TroChoi_Anh { get; set; }
        public DbSet<KhuVuiChoi_Anh> KhuVuiChoi_Anh { get; set; }

        // ========================
        // Cấu hình quan hệ
        // ========================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình tên bảng rõ ràng
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<KhachHang>().ToTable("KhachHang");
            modelBuilder.Entity<NhanVien>().ToTable("NhanVien");
            modelBuilder.Entity<KhuVuiChoi>().ToTable("KhuVuiChoi");
            modelBuilder.Entity<TroChoi>().ToTable("TroChoi");
            modelBuilder.Entity<GoiDichVu>().ToTable("GoiDichVu");
            modelBuilder.Entity<Ca>().ToTable("Ca");
            modelBuilder.Entity<Ve>().ToTable("Ve");
            modelBuilder.Entity<HoaDon>().ToTable("HoaDon");
            modelBuilder.Entity<DichVuThem>().ToTable("DichVuThem");

            // Khóa chính phức hợp
            modelBuilder.Entity<KhuVuiChoi_TroChoi>().HasKey(kt => new { kt.MaKhu, kt.MaTroChoi });
            modelBuilder.Entity<GoiDichVu_TroChoi>().HasKey(gt => new { gt.MaGoi, gt.MaTroChoi });
            modelBuilder.Entity<Ve_DichVuThem>().HasKey(vd => new { vd.MaVe, vd.MaDVThem });
            modelBuilder.Entity<NhanVien_Ca>().HasKey(nc => new { nc.MaNV, nc.MaCa, nc.NgayLamViec });
            modelBuilder.Entity<TroChoi_Anh>().HasKey(ta => ta.MaAnh);
            modelBuilder.Entity<KhuVuiChoi_Anh>().HasKey(ka => ka.MaAnh);

            // KhachHang - User (1-1)
            modelBuilder.Entity<KhachHang>()
                .HasOne(k => k.User)
                .WithOne(u => u.KhachHang)
                .HasForeignKey<KhachHang>(k => k.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // NhanVien - User (1-1)
            modelBuilder.Entity<NhanVien>()
                .HasOne(nv => nv.User)
                .WithOne(u => u.NhanVien)
                .HasForeignKey<NhanVien>(nv => nv.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // KhuVuiChoi - TroChoi (n-n)
            modelBuilder.Entity<KhuVuiChoi_TroChoi>()
                .HasOne(kt => kt.KhuVuiChoi)
                .WithMany(k => k.KhuVuiChoi_TroChoi)
                .HasForeignKey(kt => kt.MaKhu)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<KhuVuiChoi_TroChoi>()
                .HasOne(kt => kt.TroChoi)
                .WithMany(t => t.KhuVuiChoi_TroChoi)
                .HasForeignKey(kt => kt.MaTroChoi)
                .OnDelete(DeleteBehavior.Cascade);

            // GoiDichVu - TroChoi (n-n)
            modelBuilder.Entity<GoiDichVu_TroChoi>()
                .HasOne(gt => gt.GoiDichVu)
                .WithMany(g => g.GoiDichVu_TroChoi)
                .HasForeignKey(gt => gt.MaGoi)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GoiDichVu_TroChoi>()
                .HasOne(gt => gt.TroChoi)
                .WithMany(t => t.GoiDichVu_TroChoi)
                .HasForeignKey(gt => gt.MaTroChoi)
                .OnDelete(DeleteBehavior.Cascade);

            // Ve - DichVuThem (n-n)
            modelBuilder.Entity<Ve_DichVuThem>()
                .HasOne(vd => vd.Ve)
                .WithMany(v => v.Ve_DichVuThem)
                .HasForeignKey(vd => vd.MaVe)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ve_DichVuThem>()
                .HasOne(vd => vd.DichVuThem)
                .WithMany(d => d.Ve_DichVuThem)
                .HasForeignKey(vd => vd.MaDVThem)
                .OnDelete(DeleteBehavior.Cascade);

            // NhanVien - Ca (n-n)
            modelBuilder.Entity<NhanVien_Ca>()
                .HasOne(nc => nc.NhanVien)
                .WithMany(nv => nv.NhanVien_Ca)
                .HasForeignKey(nc => nc.MaNV)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NhanVien_Ca>()
                .HasOne(nc => nc.Ca)
                .WithMany(c => c.NhanVien_Ca)
                .HasForeignKey(nc => nc.MaCa)
                .OnDelete(DeleteBehavior.Cascade);

            // DangKyLichTruc - NhanVien & Ca
            modelBuilder.Entity<DangKyLichTruc>()
                .HasOne(d => d.NhanVien)
                .WithMany()
                .HasForeignKey(d => d.MaNV)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DangKyLichTruc>()
                .HasOne(d => d.Ca)
                .WithMany()
                .HasForeignKey(d => d.MaCa)
                .OnDelete(DeleteBehavior.Cascade);

            // TroChoi - Anh
            modelBuilder.Entity<TroChoi_Anh>()
                .HasOne(ta => ta.TroChoi)
                .WithMany(t => t.TroChoi_Anh)
                .HasForeignKey(ta => ta.MaTroChoi)
                .OnDelete(DeleteBehavior.Cascade);

            // KhuVuiChoi - Anh
            modelBuilder.Entity<KhuVuiChoi_Anh>()
                .HasOne(ka => ka.KhuVuiChoi)
                .WithMany(k => k.KhuVuiChoi_Anh)
                .HasForeignKey(ka => ka.MaKhu)
                .OnDelete(DeleteBehavior.Cascade);

            // Ve - GoiDichVu
            modelBuilder.Entity<Ve>()
                .HasOne(v => v.GoiDichVu)
                .WithMany(g => g.Ve)
                .HasForeignKey(v => v.MaGoi)
                .OnDelete(DeleteBehavior.Restrict);

            // HoaDon - Ve
            modelBuilder.Entity<HoaDon>()
                .HasOne(h => h.KhachHang)
                .WithMany()
                .HasForeignKey(h => h.MaKH)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

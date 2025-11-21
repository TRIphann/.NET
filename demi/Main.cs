using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BTLLTTQ
{
    public partial class Main : Form
    {
        private int UserVaiTroID;
        private string UserHoTen; // Có thể thêm HoTen để hiển thị lời chào

        // ⭐️ Constructor mới: Nhận VaiTroID từ Form Đăng nhập
        public Main(int vaiTroID)
        {
            InitializeComponent();
            this.UserVaiTroID = vaiTroID;

            // Gọi hàm phân quyền ngay khi Form được tạo
            PhanQuyenChucNang(vaiTroID);
        }

        // Tùy chọn: Nếu bạn muốn nhận cả Họ Tên từ Form Đăng nhập
        // public Main(int vaiTroID, string hoTen)
        // {
        //     InitializeComponent();
        //     this.UserVaiTroID = vaiTroID;
        //     this.UserHoTen = hoTen;
        //     PhanQuyenChucNang(vaiTroID);
        // }


        private void PhanQuyenChucNang(int vaiTroID)
        {
            string tenVaiTro = GetVaiTroName(vaiTroID);
            this.Text = $"Hệ thống Quản lý Jump Arena - Đang đăng nhập: {tenVaiTro}";

            // Ví dụ phân quyền (Bạn sẽ tùy chỉnh sau khi thiết kế Menu):
            // if (vaiTroID == 1) // Nhân viên bán hàng
            // {
            //     menuBaoCao.Visible = false;
            //     menuQuanLyNhanSu.Visible = false;
            // }
            // else if (vaiTroID == 3) // Kế toán trưởng
            // {
            //     menuQuanLyNhanSu.Visible = false;
            // }
        }

        // Hàm hỗ trợ lấy tên vai trò
        private string GetVaiTroName(int id)
        {
            if (id == 1) return "Nhân viên";
            if (id == 2) return "Quản lý";
            if (id == 3) return "Kế toán trưởng";
            return "Khách (Lỗi)";
        }

        // ... Các sự kiện Form khác (ví dụ: khi nhấn nút Logout) ...
    }
}
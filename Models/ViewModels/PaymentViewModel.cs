namespace QLDuLichRBAC_Upgrade.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho thanh toán
    /// </summary>
    public class PaymentViewModel
    {
        public int MaTour { get; set; }
        public string TenTour { get; set; } = string.Empty;
        public int SoLuongVe { get; set; }
        public decimal GiaTour { get; set; }
        public List<DichVuViewModel> DichVuDaChon { get; set; } = new();
        public decimal TongTien { get; set; }
        public string QRCodeBase64 { get; set; } = string.Empty;
        public string MaGiaoDich { get; set; } = string.Empty;
    }

    public class DichVuViewModel
    {
        public int MaDV { get; set; }
        public string TenDV { get; set; } = string.Empty;
        public decimal DonGia { get; set; }
    }

    /// <summary>
    /// Model cho ph?n h?i ki?m tra thanh toán
    /// </summary>
    public class PaymentCheckResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
    }
}

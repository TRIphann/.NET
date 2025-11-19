using Microsoft.EntityFrameworkCore;
using QLDuLichRBAC_Upgrade.Models;
using QLDuLichRBAC_Upgrade.Services;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// ===== CẤU HÌNH DỊCH VỤ =====
// Cấu hình Encoding UTF-8 cho Vietnamese
builder.Services.Configure<Microsoft.Extensions.WebEncoders.WebEncoderOptions>(options =>
{
    options.TextEncoderSettings = new System.Text.Encodings.Web.TextEncoderSettings(UnicodeRanges.All);
});

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(UnicodeRanges.All);
    });
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Kết nối Database
builder.Services.AddDbContext<QLJumaparenaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("QLJumparena")));

// ✅ Đăng ký PaymentService
builder.Services.AddHttpClient<PaymentService>();
builder.Services.AddScoped<PaymentService>();

// ===== CẤU HÌNH APP =====
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// ===== ĐỊNH NGHĨA ROUTE MẶC ĐỊNH =====
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

//dotnet watch run --project "D:\sql\PhanTichThietKEYC\QLDuLichRBAC_Final\QLDuLichRBAC_Final.csproj"
//dotnet build "D:\sql\PhanTichThietKEYC\QLDuLichRBAC_Final\QLDuLichRBAC_Final.csproj"
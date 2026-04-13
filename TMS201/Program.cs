using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using TMS201.Data;
using TMS201.Hubs;
using TMS201.Services;
using TMS201.Services.Implementations;
using TMS201.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add services
builder.Services.AddControllersWithViews();

// 🔥 ADD THIS (IMPORTANT)
builder.Services.AddSignalR();

// ✅ Register Report Service
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddScoped<IExcelImportService, ExcelImportService>();


builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

// ✅ DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));


builder.Services.Configure<FormOptions>(options => {
    options.MultipartBodyLengthLimit = 52428800; // 50 MB
});

builder.WebHost.ConfigureKestrel(options => {
    options.Limits.MaxRequestBodySize = 52428800; // 50 MB
});


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

//app.UseSession();  // 🔥 must

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"
);

app.MapHub<NotificationHub>("/notificationHub"); // 🔥 must

app.Run();

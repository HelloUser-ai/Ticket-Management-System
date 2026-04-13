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

// ✅ SignalR
builder.Services.AddSignalR();

// ✅ Custom Services
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IExcelImportService, ExcelImportService>();

// ✅ Session + HttpContext
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ DbContext (connection string from config)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// ✅ File upload limit (50MB)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52428800;
});

// ✅ Kestrel config (50MB)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 52428800;
});

var app = builder.Build();

// ✅ Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Session MUST be before Authorization
app.UseSession();

app.UseAuthorization();

// ✅ Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"
);

// ✅ SignalR Hub
app.MapHub<NotificationHub>("/notificationHub");

app.Run();

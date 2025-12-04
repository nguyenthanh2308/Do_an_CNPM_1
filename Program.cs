using HotelManagementSystem.Data;
using HotelManagementSystem.Services.Interfaces;
using HotelManagementSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1. Lấy connection string từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Đăng ký DbContext sử dụng MySQL (Pomelo)
builder.Services.AddDbContext<HotelDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// 3. Đăng ký Services cho DI
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IRoomTypeService, RoomTypeService>();
builder.Services.AddScoped<IAmenityService, AmenityService>();
builder.Services.AddScoped<IRatePlanService, RatePlanService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IHousekeepingService, HousekeepingService>();
builder.Services.AddScoped<IReportService, ReportService>();

// 4. Cấu hình Authentication với Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// 5. Đăng ký Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("Staff", policy => policy.RequireRole("Admin", "Manager", "Receptionist"));
    options.AddPolicy("AllRoles", policy => policy.RequireRole("Admin", "Manager", "Receptionist", "Housekeeping"));
});

// 6. Đăng ký MVC (Controllers + Views)
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Pipeline cơ bản
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();   // Bật Authentication
app.UseAuthorization();    // Bật Authorization

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

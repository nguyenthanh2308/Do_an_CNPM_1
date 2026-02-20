using HotelManagement.API.Data;
using HotelManagement.API.Repositories.Interfaces;
using HotelManagement.API.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Repository Pattern - Generic Repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

// Repository Pattern - Specific Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IGuestRepository, GuestRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IHousekeepingRepository, HousekeepingRepository>();
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
builder.Services.AddScoped<IRatePlanRepository, RatePlanRepository>();
builder.Services.AddScoped<IAmenityRepository, AmenityRepository>();
builder.Services.AddScoped<IBookingRoomRepository, BookingRoomRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IRoomTypeAmenityRepository, RoomTypeAmenityRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Business Services
// Core Priority 1 Services
builder.Services.AddScoped<HotelManagement.API.Services.Interfaces.IAuthService, HotelManagement.API.Services.Implementations.AuthService>();
builder.Services.AddScoped<HotelManagement.API.Services.Interfaces.IBookingService, HotelManagement.API.Services.Implementations.BookingService>();
builder.Services.AddScoped<HotelManagement.API.Services.Interfaces.IRoomService, HotelManagement.API.Services.Implementations.RoomService>();
builder.Services.AddScoped<HotelManagement.API.Services.Interfaces.IGuestService, HotelManagement.API.Services.Implementations.GuestService>();
builder.Services.AddScoped<HotelManagement.API.Services.Interfaces.IHotelService, HotelManagement.API.Services.Implementations.HotelService>();
builder.Services.AddScoped<HotelManagement.API.Services.Interfaces.IAmenityService, HotelManagement.API.Services.Implementations.AmenityService>();
builder.Services.AddScoped<HotelManagement.API.Services.Interfaces.IRoomTypeService, HotelManagement.API.Services.Implementations.RoomTypeService>();
builder.Services.AddScoped<HotelManagement.API.Services.Interfaces.IPaymentService, HotelManagement.API.Services.Implementations.PaymentService>();
builder.Services.AddScoped<HotelManagement.API.Services.Interfaces.IInvoiceService, HotelManagement.API.Services.Implementations.InvoiceService>();

// Priority 2+ Services
builder.Services.AddScoped<HotelManagement.API.Services.Interfaces.IHousekeepingService, HotelManagement.API.Services.Implementations.HousekeepingService>();
builder.Services.AddScoped<HotelManagement.API.Services.Interfaces.INotificationService, HotelManagement.API.Services.Implementations.NotificationService>();
builder.Services.AddScoped<HotelManagement.API.Services.Interfaces.IPaymentTransactionService, HotelManagement.API.Services.Implementations.PaymentTransactionService>();
builder.Services.AddScoped<HotelManagement.API.Services.Interfaces.IPromotionService, HotelManagement.API.Services.Implementations.PromotionService>();
builder.Services.AddScoped<HotelManagement.API.Services.Interfaces.IRatePlanService, HotelManagement.API.Services.Implementations.RatePlanService>();
builder.Services.AddScoped<HotelManagement.API.Services.Interfaces.IReportService, HotelManagement.API.Services.Implementations.ReportService>();


// CORS Configuration for Angular Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular default port
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// JWT Authentication Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.

// Middleware: Request Logging (log all requests)
app.UseMiddleware<HotelManagement.API.Middleware.RequestLoggingMiddleware>();

// Middleware: Exception Handling (catch and format errors)
app.UseMiddleware<HotelManagement.API.Middleware.ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program accessible to integration tests
public partial class Program { }

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using QuanLyPhongKhamApi.BackgroundServices;
using QuanLyPhongKhamApi.BLL;
using QuanLyPhongKhamApi.DAL;
using QuanLyPhongKhamApi.Middleware;
using QuanLyPhongKhamApi.Models;
using QuanLyPhongKhamApi.Services;
using Serilog;
using System.Security.Claims;
using System.Text;

// --- 1. CẤU HÌNH SERILOG ---
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    Log.Information("Bắt đầu khởi tạo ứng dụng...");
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // --- 2. ĐĂNG KÝ DỊCH VỤ (SERVICES) ---
    var jwtKey = builder.Configuration["Jwt:Key"]!;

    builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();

    // ✅ BƯỚC 1: THÊM CẤU HÌNH CORS VÀO ĐÂY
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll",
            policyBuilder =>
            {
                policyBuilder.AllowAnyOrigin() // Cho phép mọi địa chỉ gọi đến
                       .AllowAnyMethod()   // Cho phép mọi phương thức (GET, POST, etc.)
                       .AllowAnyHeader();  // Cho phép mọi header
            });
    });


    // Đăng ký dịch vụ email "giả" để mô phỏng
    builder.Services.AddTransient<IEmailService, MockEmailService>();

    // Đăng ký Dịch vụ chạy nền
    builder.Services.AddHostedService<AppointmentReminderService>();



    builder.Services.AddControllers();

    // Đăng ký DAL & BLL
    builder.Services.AddSingleton<AccountDAL>();
    builder.Services.AddTransient<AppointmentDAL>();
    builder.Services.AddTransient<DoctorDAL>();
    builder.Services.AddTransient<DrugDAL>();
    builder.Services.AddTransient<EncounterDAL>();
    builder.Services.AddTransient<InvoiceDAL>();
    builder.Services.AddTransient<PatientDAL>();
    builder.Services.AddTransient<ReportDAL>();
    builder.Services.AddTransient<SpecialtyDAL>();

    builder.Services.AddTransient<AccountBLL>();
    builder.Services.AddTransient<AppointmentBLL>();
    builder.Services.AddTransient<DoctorBLL>();
    builder.Services.AddTransient<DrugBLL>();
    builder.Services.AddTransient<EncounterBLL>();
    builder.Services.AddTransient<InvoiceBLL>();
    builder.Services.AddTransient<PatientBLL>();
    builder.Services.AddTransient<ReportBLL>();
    builder.Services.AddTransient<SpecialtyBLL>();

    // Cấu hình JWT
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.Name
            };
        });

    builder.Services.AddAuthorization();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // --- 3. CẤU HÌNH MIDDLEWARE PIPELINE ---
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();

    // ✅ BƯỚC 2: KÍCH HOẠT CORS MIDDLEWARE Ở ĐÂY
    // Vị trí này rất quan trọng: sau UseRouting và trước UseAuthorization
    app.UseRouting();
    app.UseCors("AllowAll");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Ứng dụng đã dừng đột ngột");
}
finally
{
    Log.CloseAndFlush();
}

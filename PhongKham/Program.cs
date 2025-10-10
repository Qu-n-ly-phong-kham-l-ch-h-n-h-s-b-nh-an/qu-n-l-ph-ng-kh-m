using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OfficeOpenXml;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// 1️⃣ Add services to the container
// ===============================
builder.Services.AddControllers();

// Đăng ký DbContext (đúng tên connection trong appsettings.json)
builder.Services.AddDbContext<PhongKhamDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PhongKhamDb")));

// Đăng ký các Service
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<DoctorService>();
builder.Services.AddScoped<SpecialtyService>();
builder.Services.AddScoped<DrugService>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<EncounterService>();
builder.Services.AddScoped<DiagnosisService>();
builder.Services.AddScoped<DrugStockService>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<PatientService>();
builder.Services.AddScoped<PrescriptionService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TokenService>();

// ===============================
// 2️⃣ Cấu hình Swagger (chuẩn OpenAPI 3.0.1)
// ===============================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PhongKham API",
        Version = "v1",
        Description = "API quản lý phòng khám có phân quyền JWT (Admin, Bác sĩ, Dược sĩ, Lễ tân, Bệnh nhân)"
    });

    // Cho phép nhập token trực tiếp trong Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Nhập token dạng: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ===============================
// 3️⃣ Cấu hình JWT Authentication
// ===============================
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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// ===============================
// 4️⃣ Build the app
// ===============================
var app = builder.Build();

// ===============================
// 5️⃣ Configure the HTTP request pipeline
// ===============================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // Đường dẫn chính xác tới swagger.json
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PhongKham API v1");
        c.RoutePrefix = "swagger"; // Đảm bảo mở Swagger tại /swagger
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

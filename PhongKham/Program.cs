using Microsoft.EntityFrameworkCore;
using PhongKham.DAL.Entities;
using PhongKham.BLL.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Đăng ký DbContext
builder.Services.AddDbContext<PhongKhamDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký Service
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<DoctorService>();
builder.Services.AddScoped<SpecialtyService>();



// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

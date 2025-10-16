// File: Controllers/ReportsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyPhongKhamApi.BLL;

namespace QuanLyPhongKhamApi.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportBLL _bus;
        public ReportsController(ReportBLL bus) { _bus = bus; }

        // GET: api/reports/doctor-revenue?startDate=2025-01-01&endDate=2025-12-31
        [HttpGet("doctor-revenue")]
        public IActionResult GetDoctorRevenue([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            // ✅ DỌN DẸP: Xóa try-catch, Middleware sẽ tự động xử lý lỗi
            var report = _bus.GetDoctorRevenue(startDate, endDate);
            return Ok(report);
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportService _reportService;

        public ReportsController(ReportService reportService)
        {
            _reportService = reportService;
        }

        // 📊 Thống kê theo bác sĩ
        [HttpGet("doctor")]
        public IActionResult GetRevenueByDoctor()
        {
            var data = _reportService.GetRevenueByDoctor();
            return Ok(data);
        }

        // 📈 Thống kê theo chuyên khoa
        [HttpGet("specialty")]
        public IActionResult GetRevenueBySpecialty()
        {
            var data = _reportService.GetRevenueBySpecialty();
            return Ok(data);
        }

        // 📤 Xuất Excel theo bác sĩ
        [HttpGet("doctor/export")]
        public IActionResult ExportRevenueByDoctor()
        {
            var file = _reportService.ExportRevenueByDoctorToExcel();
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DoanhThu_BacSi.xlsx");
        }

        // 📤 Xuất Excel theo chuyên khoa
        [HttpGet("specialty/export")]
        public IActionResult ExportRevenueBySpecialty()
        {
            var file = _reportService.ExportRevenueBySpecialtyToExcel();
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DoanhThu_ChuyenKhoa.xlsx");
        }
    }
}

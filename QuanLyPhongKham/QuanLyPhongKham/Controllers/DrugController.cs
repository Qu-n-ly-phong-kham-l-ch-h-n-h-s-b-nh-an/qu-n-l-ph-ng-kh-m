// File: Controllers/DrugsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyPhongKhamApi.BLL;
using QuanLyPhongKhamApi.Models;
using System.Security.Claims;

namespace QuanLyPhongKhamApi.Controllers
{
    [Authorize(Roles = "Admin,Doctor")] // Chỉ Admin và Dược sĩ có quyền
    [ApiController]
    [Route("api")]
    public class DrugsController : ControllerBase
    {
        private readonly DrugBLL _bus;
        public DrugsController(DrugBLL bus) { _bus = bus; }

        #region Drug Endpoints
        [HttpGet("drugs")]
        public IActionResult GetAllDrugs()
        {
            return Ok(_bus.GetAll());
        }

        [HttpGet("drugs/{id}")]
        public IActionResult GetDrugById(int id)
        {
            var drug = _bus.GetById(id);
            if (drug == null) return NotFound();
            return Ok(drug);
        }

        [HttpPost("drugs")]
        public IActionResult CreateDrug([FromBody] DrugRequest req)
        {
            // ✅ DỌN DẸP: Xóa try-catch, Middleware sẽ tự động xử lý lỗi
            var createdBy = int.Parse(User.FindFirstValue("AccountID")!);
            var newId = _bus.Create(req, createdBy);
            var newDrug = _bus.GetById(newId);
            return CreatedAtAction(nameof(GetDrugById), new { id = newId }, newDrug);
        }

        [HttpPut("drugs/{id}")]
        public IActionResult UpdateDrug(int id, [FromBody] DrugRequest req)
        {
            // ✅ DỌN DẸP: Xóa try-catch
            var ok = _bus.Update(id, req);
            if (!ok) return NotFound();
            return Ok(new { message = "Cập nhật thông tin thuốc thành công." });
        }

        [HttpDelete("drugs/{id}")]
        [Authorize(Roles = "Admin")] // Chỉ Admin được xóa
        public IActionResult DeleteDrug(int id)
        {
            var ok = _bus.Delete(id);
            if (!ok) return NotFound();
            return NoContent();
        }
        #endregion

        #region Stock Endpoints
        [HttpGet("stock")]
        public IActionResult GetStockReport()
        {
            return Ok(_bus.GetStockReport());
        }

        [HttpPost("stock/adjust")]
        public IActionResult AdjustStock([FromBody] StockAdjustRequest req)
        {
            var ok = _bus.AdjustStock(req);
            if (!ok) return BadRequest(new { message = "Điều chỉnh tồn kho thất bại." });
            return Ok(new { message = "Điều chỉnh tồn kho thành công." });
        }
        #endregion
    }
}
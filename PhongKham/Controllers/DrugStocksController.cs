using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;
using PhongKham.API.Models.DTOs;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Bắt buộc đăng nhập
    public class DrugStocksController : ControllerBase
    {
        private readonly DrugStockService _drugStockService;

        public DrugStocksController(DrugStockService drugStockService)
        {
            _drugStockService = drugStockService;
        }

        // ===================== GET ALL =====================
        [Authorize(Roles = "Admin,Pharmacist")]
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _drugStockService.GetAll()
                .Select(ds => new DrugStockResponseDTO
                {
                    StockId = ds.StockId,
                    DrugId = ds.DrugId,
                    DrugName = ds.Drug?.DrugName,
                    QuantityAvailable = ds.QuantityAvailable,
                    LastUpdated = ds.LastUpdated
                });

            return Ok(list);
        }

        // ===================== GET BY ID =====================
        [Authorize(Roles = "Admin,Pharmacist")]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var ds = _drugStockService.GetById(id);
            if (ds == null) return NotFound("Không tìm thấy bản ghi tồn kho.");

            var dto = new DrugStockResponseDTO
            {
                StockId = ds.StockId,
                DrugId = ds.DrugId,
                DrugName = ds.Drug?.DrugName,
                QuantityAvailable = ds.QuantityAvailable,
                LastUpdated = ds.LastUpdated
            };

            return Ok(dto);
        }

        // ===================== CREATE =====================
        [Authorize(Roles = "Admin,Pharmacist")]
        [HttpPost]
        public IActionResult Create([FromBody] DrugStockRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stock = new DrugStock
            {
                DrugId = dto.DrugId,
                QuantityAvailable = dto.QuantityAvailable,
                LastUpdated = DateTime.Now
            };

            _drugStockService.Create(stock);
            return Ok(new { message = "Thêm tồn kho thành công", stock.StockId });
        }

        // ===================== UPDATE =====================
        [Authorize(Roles = "Admin,Pharmacist")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] DrugStockRequestDTO dto)
        {
            var existing = _drugStockService.GetById(id);
            if (existing == null)
                return NotFound("Không tìm thấy bản ghi tồn kho để cập nhật.");

            existing.QuantityAvailable = dto.QuantityAvailable;
            existing.LastUpdated = DateTime.Now;
            existing.DrugId = dto.DrugId;

            _drugStockService.Update(existing);
            return Ok(new { message = "Cập nhật tồn kho thành công" });
        }

        // ===================== DELETE =====================
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _drugStockService.Delete(id);
            return NoContent();
        }
    }
}
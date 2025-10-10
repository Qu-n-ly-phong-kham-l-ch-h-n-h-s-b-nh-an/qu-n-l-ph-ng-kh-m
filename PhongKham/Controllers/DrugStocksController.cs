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

        // ✅ GET ALL
        [Authorize(Roles = "Admin,Pharmacist")]
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _drugStockService.GetAll()
                .Select(ds => new DrugStockDTO
                {
                    StockId = ds.StockId,
                    DrugId = ds.DrugId,
                    DrugName = ds.Drug?.DrugName,
                    QuantityAvailable = ds.QuantityAvailable ?? 0,
                    LastUpdated = ds.LastUpdated
                });

            return Ok(list);
        }

        // ✅ GET BY ID
        [Authorize(Roles = "Admin,Pharmacist")]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var ds = _drugStockService.GetById(id);
            if (ds == null) return NotFound("Không tìm thấy bản ghi tồn kho.");

            var dto = new DrugStockDTO
            {
                StockId = ds.StockId,
                DrugId = ds.DrugId,
                DrugName = ds.Drug?.DrugName,
                QuantityAvailable = ds.QuantityAvailable ?? 0,
                LastUpdated = ds.LastUpdated
            };

            return Ok(dto);
        }

        // ✅ CREATE
        [Authorize(Roles = "Admin,Pharmacist")]
        [HttpPost]
        public IActionResult Create([FromBody] DrugStockDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var stock = new DrugStock
                {
                    DrugId = dto.DrugId,
                    QuantityAvailable = dto.QuantityAvailable,
                    LastUpdated = DateTime.Now
                };

                _drugStockService.Create(stock);
                return Ok(new { message = "✅ Thêm tồn kho thành công!", stock.StockId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ✅ UPDATE
        [Authorize(Roles = "Admin,Pharmacist")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] DrugStockDTO dto)
        {
            var existing = _drugStockService.GetById(id);
            if (existing == null)
                return NotFound("Không tìm thấy bản ghi tồn kho để cập nhật.");

            existing.QuantityAvailable = dto.QuantityAvailable;
            existing.DrugId = dto.DrugId;
            existing.LastUpdated = DateTime.Now;

            _drugStockService.Update(existing);
            return Ok(new { message = "✅ Cập nhật tồn kho thành công!" });
        }

        // ✅ DELETE
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _drugStockService.Delete(id);
                return Ok(new { message = "🗑️ Xóa bản ghi tồn kho thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Pharmacist")]
        [HttpGet("search")]
        public IActionResult Search(string? keyword, int? minQty, int? maxQty, int page = 1, int pageSize = 10)
        {
            var list = _drugStockService.Search(keyword, minQty, maxQty, page, pageSize)
                .Select(ds => new DrugStockDTO
                {
                    StockId = ds.StockId,
                    DrugName = ds.Drug?.DrugName,
                    QuantityAvailable = ds.QuantityAvailable ?? 0,
                    LastUpdated = ds.LastUpdated
                });

            return Ok(list);
        }

    }
}

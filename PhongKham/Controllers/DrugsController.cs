using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;
using PhongKham.API.Models.DTOs;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DrugsController : ControllerBase
    {
        private readonly DrugService _drugService;

        public DrugsController(DrugService drugService)
        {
            _drugService = drugService;
        }

        // ✅ GET ALL (Tìm kiếm / Lọc / Sắp xếp / Phân trang)
        [Authorize(Roles = "Admin,Pharmacist,Doctor")]
        [HttpGet]
        public ActionResult<IEnumerable<DrugDTO>> GetAll(
            [FromQuery] string? keyword,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? sortBy,
            [FromQuery] bool desc = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var drugs = _drugService.GetAll(keyword, minPrice, maxPrice, sortBy, desc, page, pageSize)
                .Select(d => new DrugDTO
                {
                    DrugId = d.DrugId,
                    DrugName = d.DrugName,
                    Unit = d.Unit,
                    Price = d.Price
                });

            return Ok(drugs);
        }

        // ✅ GET BY ID
        [Authorize(Roles = "Admin,Pharmacist,Doctor")]
        [HttpGet("{id}")]
        public ActionResult<DrugDTO> GetById(int id)
        {
            var d = _drugService.GetById(id);
            if (d == null)
                return NotFound("Không tìm thấy thuốc.");

            return Ok(new DrugDTO
            {
                DrugId = d.DrugId,
                DrugName = d.DrugName,
                Unit = d.Unit,
                Price = d.Price
            });
        }

        // ✅ CREATE
        [Authorize(Roles = "Admin,Pharmacist")]
        [HttpPost]
        public IActionResult Create([FromBody] DrugDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var drug = new Drug
                {
                    DrugName = dto.DrugName,
                    Unit = dto.Unit,
                    Price = dto.Price
                };
                _drugService.Create(drug);
                return Ok(new { message = "Thêm thuốc thành công!", drugId = drug.DrugId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ✅ UPDATE
        [Authorize(Roles = "Admin,Pharmacist")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] DrugDTO dto)
        {
            var existing = _drugService.GetById(id);
            if (existing == null)
                return NotFound("Không tìm thấy thuốc.");

            existing.DrugName = dto.DrugName;
            existing.Unit = dto.Unit;
            existing.Price = dto.Price;
            _drugService.Update(existing);
            return Ok(new { message = "Cập nhật thuốc thành công!" });
        }

        // ✅ DELETE
        [Authorize(Roles = "Admin,Pharmacist")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _drugService.Delete(id);
                return Ok(new { message = "Xóa thuốc thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}

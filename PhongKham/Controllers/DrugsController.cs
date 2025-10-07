using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;
using PhongKham.API.Models.DTOs;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // yêu cầu đăng nhập
    public class DrugsController : ControllerBase
    {
        private readonly DrugService _drugService;

        public DrugsController(DrugService drugService)
        {
            _drugService = drugService;
        }

        // ======================== 1️⃣ ADMIN & DƯỢC SĨ XEM DANH SÁCH ========================
        [Authorize(Roles = "Admin,Pharmacist")]
        [HttpGet]
        public ActionResult<IEnumerable<DrugResponseDTO>> GetAll()
        {
            var drugs = _drugService.GetAll()
                .Select(d => new DrugResponseDTO
                {
                    DrugId = d.DrugId,
                    DrugName = d.DrugName,
                    Unit = d.Unit,
                    Price = d.Price
                });

            return Ok(drugs);
        }

        // ======================== 2️⃣ XEM CHI TIẾT THUỐC ========================
        [Authorize(Roles = "Admin,Pharmacist,Doctor")]
        [HttpGet("{id}")]
        public ActionResult<DrugResponseDTO> GetById(int id)
        {
            var d = _drugService.GetById(id);
            if (d == null)
                return NotFound("Không tìm thấy thuốc.");

            var dto = new DrugResponseDTO
            {
                DrugId = d.DrugId,
                DrugName = d.DrugName,
                Unit = d.Unit,
                Price = d.Price
            };

            return Ok(dto);
        }

        // ======================== 3️⃣ ADMIN & DƯỢC SĨ THÊM THUỐC ========================
        [Authorize(Roles = "Admin,Pharmacist")]
        [HttpPost]
        public IActionResult Create([FromBody] DrugRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var drug = new Drug
            {
                DrugName = dto.DrugName,
                Unit = dto.Unit,
                Price = dto.Price
            };

            _drugService.Create(drug);
            return Ok(new { message = "Thêm thuốc thành công!", drugId = drug.DrugId });
        }

        // ======================== 4️⃣ ADMIN & DƯỢC SĨ CẬP NHẬT ========================
        [Authorize(Roles = "Admin,Pharmacist")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] DrugRequestDTO dto)
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

        // ======================== 5️⃣ ADMIN & DƯỢC SĨ XÓA ========================
        [Authorize(Roles = "Admin,Pharmacist")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var drug = _drugService.GetById(id);
            if (drug == null)
                return NotFound("Không tìm thấy thuốc.");

            _drugService.Delete(id);
            return Ok(new { message = "Xóa thuốc thành công!" });
        }
    }
}

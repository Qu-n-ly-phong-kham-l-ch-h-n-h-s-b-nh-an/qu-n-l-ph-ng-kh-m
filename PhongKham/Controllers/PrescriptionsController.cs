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
    public class PrescriptionsController : ControllerBase
    {
        private readonly PrescriptionService _prescriptionService;

        public PrescriptionsController(PrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }

        // ======================== 1️⃣ GET ALL ========================
        [Authorize(Roles = "Admin,Doctor,Pharmacist")]
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _prescriptionService.GetAll()
                .Select(p => new PrescriptionDTO
                {
                    PrescriptionId = p.PrescriptionId,
                    EncounterId = p.EncounterId,
                    DrugId = p.DrugId,
                    DrugName = p.Drug?.DrugName ?? "(Không rõ)",
                    Quantity = p.Quantity ?? 0,
                    Usage = p.Usage
                });

            return Ok(list);
        }

        // ======================== 2️⃣ GET BY ID ========================
        [Authorize(Roles = "Admin,Doctor,Pharmacist")]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var p = _prescriptionService.GetById(id);
            if (p == null)
                return NotFound("Không tìm thấy đơn thuốc.");

            var dto = new PrescriptionDTO
            {
                PrescriptionId = p.PrescriptionId,
                EncounterId = p.EncounterId,
                DrugId = p.DrugId,
                DrugName = p.Drug?.DrugName ?? "(Không rõ)",
                Quantity = p.Quantity ?? 0,
                Usage = p.Usage
            };

            return Ok(dto);
        }

        // ======================== 3️⃣ CREATE ========================
        [Authorize(Roles = "Doctor")]
        [HttpPost]
        public IActionResult Create([FromBody] PrescriptionDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var prescription = new Prescription
                {
                    EncounterId = dto.EncounterId,
                    DrugId = dto.DrugId,
                    Quantity = dto.Quantity,
                    Usage = dto.Usage
                };

                _prescriptionService.Create(prescription);

                return Ok(new
                {
                    message = "Thêm đơn thuốc thành công!",
                    prescriptionId = prescription.PrescriptionId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        // ======================== 4️⃣ UPDATE ========================
        [Authorize(Roles = "Admin,Doctor")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] PrescriptionDTO dto)
        {
            var existing = _prescriptionService.GetById(id);
            if (existing == null)
                return NotFound("Không tìm thấy đơn thuốc.");

            existing.DrugId = dto.DrugId;
            existing.Quantity = dto.Quantity;
            existing.Usage = dto.Usage;

            _prescriptionService.Update(existing);
            return Ok(new { message = "Cập nhật đơn thuốc thành công!" });
        }

        // ======================== 5️⃣ DELETE ========================
        [Authorize(Roles = "Admin,Pharmacist")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var prescription = _prescriptionService.GetById(id);
            if (prescription == null)
                return NotFound("Không tìm thấy đơn thuốc.");

            _prescriptionService.Delete(id);
            return Ok(new { message = "Xóa đơn thuốc thành công!" });
        }
    }
}
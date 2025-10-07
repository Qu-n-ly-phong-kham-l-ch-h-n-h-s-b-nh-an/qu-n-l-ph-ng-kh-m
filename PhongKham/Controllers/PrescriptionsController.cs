using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;
using PhongKham.API.Models.DTOs;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrescriptionsController : ControllerBase
    {
        private readonly PrescriptionService _prescriptionService;

        public PrescriptionsController(PrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }

        // ======================== 1️⃣ GET ALL ========================
        [HttpGet]
        public ActionResult<IEnumerable<PrescriptionResponseDTO>> GetAll()
        {
            var list = _prescriptionService.GetAll()
                .Select(p => new PrescriptionResponseDTO
                {
                    PrescriptionId = p.PrescriptionId,
                    DrugName = p.Drug?.DrugName ?? "(Không rõ)",
                    Quantity = p.Quantity ?? 0,
                    Usage = p.Usage
                });

            return Ok(list);
        }

        // ======================== 2️⃣ GET BY ID ========================
        [HttpGet("{id}")]
        public ActionResult<PrescriptionResponseDTO> GetById(int id)
        {
            var p = _prescriptionService.GetById(id);
            if (p == null)
                return NotFound("Không tìm thấy đơn thuốc");

            var dto = new PrescriptionResponseDTO
            {
                PrescriptionId = p.PrescriptionId,
                DrugName = p.Drug?.DrugName ?? "(Không rõ)",
                Quantity = p.Quantity ?? 0,
                Usage = p.Usage
            };

            return Ok(dto);
        }

        // ======================== 3️⃣ CREATE ========================
        [HttpPost]
        public IActionResult Create([FromBody] PrescriptionRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var prescription = new Prescription
            {
                EncounterId = dto.EncounterId,
                DrugId = dto.DrugId,
                Quantity = dto.Quantity,
                Usage = dto.Usage
            };

            _prescriptionService.Create(prescription);
            return Ok(new { message = "Thêm đơn thuốc thành công", prescription.PrescriptionId });
        }

        // ======================== 4️⃣ UPDATE ========================
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] PrescriptionRequestDTO dto)
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
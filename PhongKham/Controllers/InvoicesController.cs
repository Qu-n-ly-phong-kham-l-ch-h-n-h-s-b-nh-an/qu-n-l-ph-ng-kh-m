using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;
using PhongKham.API.Models.DTOs;
using System.Linq;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly InvoiceService _service;

        public InvoicesController(InvoiceService service)
        {
            _service = service;
        }

        // ================== 1️⃣ LẤY DANH SÁCH ==================
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet]
        public IActionResult GetAll()
        {
            var data = _service.GetAll()
                .Select(i => new InvoiceDTO
                {
                    InvoiceId = i.InvoiceId,
                    PatientName = i.Patient?.FullName,
                    EncounterId = i.EncounterId,
                    TotalAmount = i.TotalAmount ?? 0,
                    PaymentDate = i.PaymentDate,
                    PaymentMethod = i.PaymentMethod,
                    Status = i.Status
                });

            return Ok(data);
        }

        // ================== 2️⃣ LẤY CHI TIẾT ==================
        [Authorize(Roles = "Admin,Receptionist,Doctor")]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var inv = _service.GetById(id);
            if (inv == null)
                return NotFound("Không tìm thấy hóa đơn.");

            var dto = new InvoiceDTO
            {
                InvoiceId = inv.InvoiceId,
                PatientName = inv.Patient?.FullName,
                EncounterId = inv.EncounterId,
                TotalAmount = inv.TotalAmount ?? 0,
                PaymentDate = inv.PaymentDate,
                PaymentMethod = inv.PaymentMethod,
                Status = inv.Status
            };

            return Ok(dto);
        }

        // ================== 3️⃣ TẠO HÓA ĐƠN ==================
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPost]
        public IActionResult Create([FromBody] InvoiceDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var inv = new Invoice
            {
                PatientId = dto.PatientId,
                EncounterId = dto.EncounterId,
                PaymentMethod = dto.PaymentMethod,
                Status = dto.Status,
                PaymentDate = dto.PaymentDate
            };

            _service.Create(inv);
            return Ok(new { message = "Tạo hóa đơn thành công!", invoiceId = inv.InvoiceId });
        }

        // ================== 4️⃣ CẬP NHẬT ==================
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] InvoiceDTO dto)
        {
            var existing = _service.GetById(id);
            if (existing == null)
                return NotFound("Không tìm thấy hóa đơn.");

            existing.TotalAmount = dto.TotalAmount;
            existing.PaymentMethod = dto.PaymentMethod;
            existing.Status = dto.Status;
            existing.PaymentDate = dto.PaymentDate ?? DateTime.Now;

            _service.Update(existing);
            return Ok(new { message = "Cập nhật hóa đơn thành công!" });
        }

        // ================== 5️⃣ XÓA ==================
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var inv = _service.GetById(id);
            if (inv == null)
                return NotFound("Không tìm thấy hóa đơn.");

            _service.Delete(id);
            return Ok(new { message = "Xóa hóa đơn thành công!" });
        }
    }
}
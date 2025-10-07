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
    [Authorize] // 🔒 Yêu cầu đăng nhập
    public class InvoicesController : ControllerBase
    {
        private readonly InvoiceService _invoiceService;

        public InvoicesController(InvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        // ==================== 1️⃣ ADMIN & LỄ TÂN XEM TẤT CẢ ====================
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _invoiceService.GetAll()
                .Select(i => new InvoiceResponseDTO
                {
                    InvoiceId = i.InvoiceId,
                    PatientName = i.Patient?.FullName,
                    EncounterId = i.EncounterId,
                    TotalAmount = i.TotalAmount ?? 0,
                    PaymentDate = i.PaymentDate,
                    PaymentMethod = i.PaymentMethod,
                    Status = i.Status
                });

            return Ok(list);
        }

        // ==================== 2️⃣ XEM CHI TIẾT HÓA ĐƠN ====================
        [Authorize(Roles = "Admin,Receptionist,Doctor")]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var inv = _invoiceService.GetById(id);
            if (inv == null)
                return NotFound("Không tìm thấy hóa đơn.");

            var dto = new InvoiceResponseDTO
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

        // ==================== 3️⃣ TẠO HÓA ĐƠN ====================
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPost]
        public IActionResult Create([FromBody] InvoiceRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var invoice = new Invoice
            {
                PatientId = dto.PatientId,
                EncounterId = dto.EncounterId,
                TotalAmount = dto.TotalAmount,
                PaymentMethod = dto.PaymentMethod,
                Status = dto.Status ?? "Chưa thanh toán",
                PaymentDate = dto.PaymentDate ?? DateTime.Now
            };

            _invoiceService.Create(invoice);
            return Ok(new { message = "Tạo hóa đơn thành công!", invoiceId = invoice.InvoiceId });
        }
// ==================== 4️⃣ CẬP NHẬT HÓA ĐƠN ====================


[Authorize(Roles = "Admin,Receptionist")]
[HttpPut("{id}")]
public IActionResult Update(int id, [FromBody] InvoiceRequestDTO dto)
{
    var existing = _invoiceService.GetById(id);
    if (existing == null)
        return NotFound("Không tìm thấy hóa đơn.");

    existing.TotalAmount = dto.TotalAmount;
    existing.PaymentMethod = dto.PaymentMethod;
    existing.Status = dto.Status;
    existing.PaymentDate = dto.PaymentDate ?? DateTime.Now;

    _invoiceService.Update(existing);
    return Ok(new { message = "Cập nhật hóa đơn thành công!" });
}

// ==================== 5️⃣ XÓA HÓA ĐƠN ====================
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public IActionResult Delete(int id)
{
    var inv = _invoiceService.GetById(id);
    if (inv == null)
        return NotFound("Không tìm thấy hóa đơn.");

    _invoiceService.Delete(id);
    return Ok(new { message = "Xóa hóa đơn thành công!" });
}
    }
}
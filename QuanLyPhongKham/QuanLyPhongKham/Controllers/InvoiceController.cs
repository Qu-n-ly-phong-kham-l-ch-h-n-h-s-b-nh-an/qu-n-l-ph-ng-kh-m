// File: Controllers/InvoicesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyPhongKhamApi.BLL;
using QuanLyPhongKhamApi.Models;

namespace QuanLyPhongKhamApi.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
    [ApiController]
    [Route("api/[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly InvoiceBLL _bus;
        public InvoicesController(InvoiceBLL bus) { _bus = bus; }

        // GET: api/invoices?status=Chưa thanh toán
        [HttpGet]
        public IActionResult GetAll([FromQuery] string? status)
        {
            return Ok(_bus.GetAll(status));
        }

        // ✅ SỬA LẠI: Hàm GetById giờ sẽ trả về chi tiết
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var invoiceDetail = _bus.GetDetailById(id);
            if (invoiceDetail == null) return NotFound();
            return Ok(invoiceDetail);
        }

        // PATCH: api/invoices/5/payment
        [HttpPatch("{id}/payment")]
        public IActionResult ProcessPayment(int id, [FromBody] PaymentRequest req)
        {
            // ✅ DỌN DẸP: Xóa try-catch, Middleware sẽ tự động xử lý lỗi
            var ok = _bus.ProcessPayment(id, req.PaymentMethod);
            if (!ok) return NotFound(new { message = "Không tìm thấy hóa đơn hoặc hóa đơn đã được thanh toán." });
            return Ok(new { message = "Xác nhận thanh toán thành công." });
        }
    }
}
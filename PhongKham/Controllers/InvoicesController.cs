using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly InvoiceService _invoiceService;

        public InvoicesController(InvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        // GET: api/invoices
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_invoiceService.GetAll());
        }

        // GET: api/invoices/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var inv = _invoiceService.GetById(id);
            if (inv == null) return NotFound();
            return Ok(inv);
        }

        // POST: api/invoices
        [HttpPost]
        public IActionResult Create([FromBody] Invoice invoice)
        {
            _invoiceService.Create(invoice);
            return Ok(invoice);
        }

        // PUT: api/invoices/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Invoice invoice)
        {
            if (id != invoice.InvoiceId) return BadRequest();
            _invoiceService.Update(invoice);
            return Ok(invoice);
        }

        // DELETE: api/invoices/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _invoiceService.Delete(id);
            return NoContent();
        }
    }
}

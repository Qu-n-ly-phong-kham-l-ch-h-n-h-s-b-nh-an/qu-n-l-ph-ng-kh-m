using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly AuditLogService _auditLogService;

        public AuditLogsController(AuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        // GET: api/auditlogs
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_auditLogService.GetAll());
        }

        // GET: api/auditlogs/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var log = _auditLogService.GetById(id);
            if (log == null) return NotFound();
            return Ok(log);
        }

        // POST: api/auditlogs
        [HttpPost]
        public IActionResult Create(AuditLog log)
        {
            _auditLogService.Create(log);
            return Ok(log);
        }

        // PUT: api/auditlogs/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, AuditLog log)
        {
            if (id != log.LogId) return BadRequest();
            _auditLogService.Update(log);
            return Ok(log);
        }

        // DELETE: api/auditlogs/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _auditLogService.Delete(id);
            return NoContent();
        }
    }
}

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
    public class AuditLogsController : ControllerBase
    {
        private readonly AuditLogService _auditLogService;

        public AuditLogsController(AuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        // ======================== 1️⃣ ADMIN & LỄ TÂN XEM TẤT CẢ LOGS ========================
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet]
        public ActionResult<IEnumerable<AuditLogResponseDTO>> GetAll()
        {
            var logs = _auditLogService.GetAll()
                .Select(l => new AuditLogResponseDTO
                {
                    LogId = l.LogId,
                    AccountId = l.AccountId,
                    Action = l.Action,
                    TableName = l.TableName,
                    RecordId = l.RecordId,
                    AccessTime = l.AccessTime
                });

            return Ok(logs);
        }

        // ======================== 2️⃣ ADMIN XEM CHI TIẾT LOG ========================
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public ActionResult<AuditLogResponseDTO> GetById(int id)
        {
            var log = _auditLogService.GetById(id);
            if (log == null) return NotFound("Không tìm thấy bản ghi log.");

            var dto = new AuditLogResponseDTO
            {
                LogId = log.LogId,
                AccountId = log.AccountId,
                Action = log.Action,
                TableName = log.TableName,
                RecordId = log.RecordId,
                AccessTime = log.AccessTime
            };

            return Ok(dto);
        }

        // ======================== 3️⃣ GHI LOG (nếu cần) ========================
        // Admin hoặc hệ thống có thể ghi log thủ công
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Create([FromBody] AuditLogRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var log = new AuditLog
            {
                AccountId = dto.AccountId,
                Action = dto.Action,
                TableName = dto.TableName,
                RecordId = dto.RecordId,
                AccessTime = DateTime.Now
            };

            _auditLogService.Create(log);
            return Ok(new { message = "Tạo log thành công", logId = log.LogId });
        }

        // ❌ Không cho sửa hoặc xóa log
        // Nếu muốn vẫn cho Admin xóa log cũ, có thể bật lại phần này
    }
}
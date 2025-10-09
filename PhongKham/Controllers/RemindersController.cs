using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RemindersController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public RemindersController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // 🗓 Gửi mô phỏng email/SMS nhắc lịch hẹn
        [HttpPost("send")]
        public async Task<IActionResult> SendReminder([FromBody] ReminderRequest request)
        {
            try
            {
                string subject = "Nhắc lịch khám bệnh";
                string body = $"Xin chào {request.PatientName},\nBạn có lịch khám vào {request.AppointmentDate:dd/MM/yyyy HH:mm}.";

                if (!string.IsNullOrEmpty(request.Email))
                    await _notificationService.SendEmailAsync(request.Email, subject, body);

                if (!string.IsNullOrEmpty(request.Phone))
                    await _notificationService.SendSmsAsync(request.Phone, body);

                return Ok(new { message = "Nhắc lịch đã được gửi (mô phỏng)", request });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Gửi nhắc lịch thất bại", error = ex.Message });
            }
        }
    }

    public class ReminderRequest
    {
        public string PatientName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public DateTime AppointmentDate { get; set; }
    }
}

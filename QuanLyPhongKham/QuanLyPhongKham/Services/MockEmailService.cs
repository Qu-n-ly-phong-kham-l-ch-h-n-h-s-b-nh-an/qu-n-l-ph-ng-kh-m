namespace QuanLyPhongKhamApi.Services
{
    /// <summary>
    /// Đây là dịch vụ email "giả" (mock).
    /// Thay vì gửi email thật, nó chỉ ghi lại thông tin email vào log.
    /// </summary>
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;

        public MockEmailService(ILogger<MockEmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // Thay vì gửi email, chúng ta chỉ ghi log lại để mô phỏng
            _logger.LogInformation("--- MÔ PHỎNG GỬI EMAIL ---");
            _logger.LogInformation("Đến: {ToEmail}", toEmail);
            _logger.LogInformation("Chủ đề: {Subject}", subject);
            _logger.LogInformation("Nội dung: {Body}", body.Replace("\n", " ")); // Thay thế xuống dòng để log đẹp hơn
            _logger.LogInformation("--------------------------");

            // Vì không có hành động bất đồng bộ nào, chúng ta hoàn thành tác vụ ngay lập tức
            return Task.CompletedTask;
        }
    }
}
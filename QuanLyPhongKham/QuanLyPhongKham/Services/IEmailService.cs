namespace QuanLyPhongKhamApi.Services
{
    /// <summary>
    /// Đây là "bản thiết kế" (Interface) cho mọi dịch vụ gửi email.
    /// Nó quy định rằng bất kỳ dịch vụ email nào cũng phải có một hàm SendEmailAsync.
    /// </summary>
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
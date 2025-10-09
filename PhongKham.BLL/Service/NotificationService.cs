using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PhongKham.BLL.Service
{
    public class NotificationService
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            await Task.Run(() =>
            {
                Console.WriteLine("📧 Gửi Email đến: " + toEmail);
                Console.WriteLine("Tiêu đề: " + subject);
                Console.WriteLine("Nội dung: " + body);
                Console.WriteLine("-------------------------------------");
            });
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            await Task.Run(() =>
            {
                Console.WriteLine("📱 Gửi SMS đến: " + phoneNumber);
                Console.WriteLine("Nội dung: " + message);
                Console.WriteLine("-------------------------------------");
            });
        }
    }
}

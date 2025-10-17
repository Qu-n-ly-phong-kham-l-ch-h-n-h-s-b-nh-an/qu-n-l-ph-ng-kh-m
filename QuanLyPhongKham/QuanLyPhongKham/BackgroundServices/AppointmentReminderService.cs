using QuanLyPhongKhamApi.DAL;
using QuanLyPhongKhamApi.Services;

namespace QuanLyPhongKhamApi.BackgroundServices
{
    public class AppointmentReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppointmentReminderService> _logger;

        public AppointmentReminderService(IServiceProvider serviceProvider, ILogger<AppointmentReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Dịch vụ Nhắc lịch hẹn đang khởi động.");

            // Chờ 20 giây sau khi ứng dụng khởi động để CSDL sẵn sàng
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Bắt đầu chu kỳ kiểm tra lịch hẹn lúc: {time}", DateTimeOffset.Now);

                await DoWork();

                // Để test, dịch vụ sẽ chạy lại sau mỗi 1 phút
                // Khi triển khai thật, bạn có thể đổi thành TimeSpan.FromHours(1)
                _logger.LogInformation("Đã hoàn tất chu kỳ. Chờ 1 phút trước khi chạy lại.");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task DoWork()
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var appointmentDal = scope.ServiceProvider.GetRequiredService<AppointmentDAL>();
                    var patientDal = scope.ServiceProvider.GetRequiredService<PatientDAL>();

                    // SỬ DỤNG INTERFACE: Yêu cầu một dịch vụ bất kỳ miễn là nó tuân thủ "bản thiết kế" IEmailService
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    var appointmentsToSend = appointmentDal.GetUpcomingAppointmentsForReminder();

                    if (!appointmentsToSend.Any())
                    {
                        _logger.LogInformation("Không có lịch hẹn nào sắp tới cần nhắc nhở.");
                        return;
                    }

                    _logger.LogInformation("Tìm thấy {Count} lịch hẹn cần gửi nhắc nhở.", appointmentsToSend.Count);

                    foreach (var appt in appointmentsToSend)
                    {
                        var patient = patientDal.GetById(appt.PatientID);
                        if (patient != null && !string.IsNullOrEmpty(patient.Email))
                        {
                            var subject = $"Nhắc lịch hẹn khám tại Phòng khám XYZ";
                            var body = $"Chào {appt.PatientName},\n\nBạn có một lịch hẹn với {appt.DoctorName} vào lúc {appt.AppointmentDate:HH:mm dd/MM/yyyy}.\n\nVui lòng có mặt trước 15 phút. Trân trọng.";

                            await emailService.SendEmailAsync(patient.Email, subject, body);

                            // Đánh dấu là đã gửi để không gửi lại
                            appointmentDal.MarkReminderAsSent(appt.AppointmentID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi trong quá trình xử lý nhắc lịch hẹn.");
            }
        }
    }
}
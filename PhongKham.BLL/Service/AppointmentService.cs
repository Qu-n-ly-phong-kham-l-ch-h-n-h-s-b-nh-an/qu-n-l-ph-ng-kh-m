using Microsoft.EntityFrameworkCore;
using PhongKham.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PhongKham.BLL.Service
{
    public class AppointmentService
    {
        private readonly PhongKhamDbContext _context;

        public AppointmentService(PhongKhamDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả lịch hẹn
        public IEnumerable<Appointment> GetAll()
        {
            return _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();
        }

        // Lấy lịch hẹn theo ID
        public Appointment? GetById(int id)
        {
            return _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefault(a => a.AppointmentId == id);
        }

        // Đặt lịch mới (Bệnh nhân hoặc lễ tân)
        public void Create(Appointment appt)
        {
            // Kiểm tra trùng lịch bác sĩ
            bool conflict = _context.Appointments.Any(a =>
                a.DoctorId == appt.DoctorId &&
                a.AppointmentDate == appt.AppointmentDate &&
                a.Status != "Đã hủy"
            );

            if (conflict)
                throw new Exception("❌ Bác sĩ đã có lịch hẹn tại thời điểm này!");

            appt.Status ??= "Chờ xác nhận";
            _context.Appointments.Add(appt);
            _context.SaveChanges();

            Console.WriteLine($"📅 Đặt lịch thành công ({appt.AppointmentDate}) – mô phỏng gửi email xác nhận.");
        }

        // ✅ Xác nhận lịch hẹn và tạo Encounter tương ứng
        public void Approve(int id)
        {
            var appt = _context.Appointments.Include(a => a.Patient).FirstOrDefault(a => a.AppointmentId == id);
            if (appt == null) throw new Exception("Không tìm thấy lịch hẹn.");

            if (appt.Status == "Đã hủy") throw new Exception("Không thể duyệt lịch đã hủy.");

            appt.Status = "Đã xác nhận";
            _context.SaveChanges();

            // 🔄 Tạo Encounter tự động
            var encounter = new Encounter
            {
                AppointmentId = appt.AppointmentId,
                DoctorId = appt.DoctorId,
                Notes = "Chưa có ghi chú"
            };

            _context.Encounters.Add(encounter);
            _context.SaveChanges();

            Console.WriteLine($"✅ Đã tạo Encounter #{encounter.EncounterId} cho Appointment #{id}");
        }

        // ✅ Hoàn tất lịch hẹn sau khi khám
        public void Complete(int id)
        {
            var appt = _context.Appointments.Find(id);
            if (appt == null) throw new Exception("Không tìm thấy lịch hẹn.");

            appt.Status = "Đã hoàn tất";
            _context.SaveChanges();

            Console.WriteLine($"🏁 Lịch hẹn #{id} đã được đánh dấu hoàn tất.");
        }

        // Cập nhật lịch hẹn
        public void Update(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            _context.SaveChanges();
        }

        // Hủy lịch hẹn
        public void Cancel(int id)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.AppointmentId == id);
            if (appointment == null)
                throw new Exception("Không tìm thấy lịch hẹn để hủy.");

            appointment.Status = "Đã hủy";
            _context.SaveChanges();
        }

        // Xóa lịch hẹn
        public void Delete(int id)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.AppointmentId == id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                _context.SaveChanges();
            }
        }
    }
}
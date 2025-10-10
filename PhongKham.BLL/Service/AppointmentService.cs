using Microsoft.EntityFrameworkCore;
using PhongKham.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhongKham.BLL.Service
{
    public class AppointmentService
    {
        private readonly PhongKhamDbContext _context;

        public AppointmentService(PhongKhamDbContext context)
        {
            _context = context;
        }

        // 📋 Lấy tất cả lịch hẹn
        public IEnumerable<Appointment> GetAll()
        {
            return _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();
        }

        // 🔍 Lọc, tìm kiếm, phân trang, sắp xếp
        public object Filter(string? keyword, string? status, string? sortBy, int page = 1, int pageSize = 10)
        {
            var query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .AsQueryable();

            // 🔎 Tìm kiếm theo tên bệnh nhân hoặc bác sĩ
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(a =>
                    (a.Patient.FullName != null && a.Patient.FullName.Contains(keyword)) ||
                    (a.Doctor.FullName != null && a.Doctor.FullName.Contains(keyword)));
            }

            // ⚙️ Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }

            // ↕️ Sắp xếp
            query = sortBy switch
            {
                "date" => query.OrderByDescending(a => a.AppointmentDate),
                "doctor" => query.OrderBy(a => a.Doctor.FullName),
                _ => query.OrderByDescending(a => a.AppointmentId)
            };

            // 📄 Phân trang
            var totalRecords = query.Count();
            var data = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new
            {
                TotalRecords = totalRecords,
                Page = page,
                PageSize = pageSize,
                Data = data.Select(a => new
                {
                    a.AppointmentId,
                    DoctorName = a.Doctor?.FullName,
                    PatientName = a.Patient?.FullName,
                    a.AppointmentDate,
                    a.Status
                })
            };
        }

        // 📅 Đặt lịch mới
        public void Create(Appointment appt)
        {
            bool conflict = _context.Appointments.Any(a =>
                a.DoctorId == appt.DoctorId &&
                a.AppointmentDate == appt.AppointmentDate &&
                a.Status != "Đã hủy");

            if (conflict)
                throw new Exception("❌ Bác sĩ đã có lịch hẹn tại thời điểm này!");

            appt.Status ??= "Chờ xác nhận";
            _context.Appointments.Add(appt);
            _context.SaveChanges();
        }

        // ✅ Duyệt lịch (tự tạo Encounter)
        public void Approve(int id)
        {
            var appt = _context.Appointments.Include(a => a.Patient).FirstOrDefault(a => a.AppointmentId == id);
            if (appt == null) throw new Exception("Không tìm thấy lịch hẹn.");
            if (appt.Status == "Đã hủy") throw new Exception("Không thể duyệt lịch đã hủy.");

            appt.Status = "Đã xác nhận";
            _context.SaveChanges();

            var encounter = new Encounter
            {
                AppointmentId = appt.AppointmentId,
                DoctorId = appt.DoctorId,
                Notes = "Chưa có ghi chú"
            };

            _context.Encounters.Add(encounter);
            _context.SaveChanges();
        }

        // ✅ Hoàn tất
        public void Complete(int id)
        {
            var appt = _context.Appointments.Find(id);
            if (appt == null) throw new Exception("Không tìm thấy lịch hẹn.");
            appt.Status = "Đã hoàn tất";
            _context.SaveChanges();
        }

        // ❌ Hủy
        public void Cancel(int id)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.AppointmentId == id);
            if (appointment == null)
                throw new Exception("Không tìm thấy lịch hẹn để hủy.");

            appointment.Status = "Đã hủy";
            _context.SaveChanges();
        }

        // 🧹 Xóa
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

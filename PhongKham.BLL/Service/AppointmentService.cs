using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhongKham.DAL.Entities;

namespace PhongKham.BLL.Service
{
    public class AppointmentService
    {
        private readonly PhongKhamDbContext _context;

        public AppointmentService(PhongKhamDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả Appointment
        public IEnumerable<Appointment> GetAll()
        {
            return _context.Appointments
                           .ToList();
        }

        // Lấy Appointment theo ID
        public Appointment? GetById(int id)
        {
            return _context.Appointments.FirstOrDefault(a => a.AppointmentId == id);
        }

        // Tạo mới Appointment
        public void Create(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            _context.SaveChanges();
        }

        // Cập nhật Appointment
        public void Update(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            _context.SaveChanges();
        }

        // Xóa Appointment
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


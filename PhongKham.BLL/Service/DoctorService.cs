using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhongKham.DAL.Entities;

namespace PhongKham.BLL.Service
{
    public class DoctorService
    {
        private readonly PhongKhamDbContext _context;

        public DoctorService(PhongKhamDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả bác sĩ
        public IEnumerable<Doctor> GetAll()
        {
            return _context.Doctors.ToList();
        }

        // Lấy 1 bác sĩ theo ID
        public Doctor? GetById(int id)
        {
            return _context.Doctors.FirstOrDefault(d => d.DoctorId == id);
        }

        // Tạo mới
        public void Create(Doctor doctor)
        {
            _context.Doctors.Add(doctor);
            _context.SaveChanges();
        }

        // Cập nhật
        public void Update(Doctor doctor)
        {
            _context.Doctors.Update(doctor);
            _context.SaveChanges();
        }

        // Xóa
        public void Delete(int id)
        {
            var doc = _context.Doctors.FirstOrDefault(d => d.DoctorId == id);
            if (doc != null)
            {
                _context.Doctors.Remove(doc);
                _context.SaveChanges();
            }
        }
    }
}


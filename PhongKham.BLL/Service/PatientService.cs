using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhongKham.DAL.Entities;

namespace PhongKham.BLL.Service
{
    public class PatientService
    {
        private readonly PhongKhamDbContext _context;

        public PatientService(PhongKhamDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả bệnh nhân
        public IEnumerable<Patient> GetAll()
        {
            return _context.Patients.ToList();
        }

        // Lấy 1 bệnh nhân theo ID
        public Patient? GetById(int id)
        {
            return _context.Patients.FirstOrDefault(p => p.PatientId == id);
        }

        // Tạo mới bệnh nhân
        public void Create(Patient patient)
        {
            _context.Patients.Add(patient);
            _context.SaveChanges();
        }

        // Cập nhật bệnh nhân
        public void Update(Patient patient)
        {
            _context.Patients.Update(patient);
            _context.SaveChanges();
        }

        // Xóa bệnh nhân
        public void Delete(int id)
        {
            var p = _context.Patients.FirstOrDefault(p => p.PatientId == id);
            if (p != null)
            {
                _context.Patients.Remove(p);
                _context.SaveChanges();
            }
        }
    }
}
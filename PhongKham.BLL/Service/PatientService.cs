using Microsoft.EntityFrameworkCore;
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

        // ✅ Lấy tất cả bệnh nhân (bao gồm Account nếu có)
        public IEnumerable<Patient> GetAll()
        {
            return _context.Patients
                .Include(p => p.Account)
                .ToList();
        }

        // ✅ Lấy theo ID
        public Patient? GetById(int id)
        {
            return _context.Patients
                .Include(p => p.Account)
                .Include(p => p.Appointments)
                .Include(p => p.Invoices)
                .FirstOrDefault(p => p.PatientId == id);
        }

        // ✅ Lấy theo tài khoản
        public Patient? GetByAccountId(int accountId)
        {
            return _context.Patients
                .Include(p => p.Account)
                .FirstOrDefault(p => p.AccountId == accountId);
        }

        // ✅ Thêm mới bệnh nhân
        public void Create(Patient patient)
        {
            _context.Patients.Add(patient);
            _context.SaveChanges();
        }

        // ✅ Cập nhật thông tin bệnh nhân
        public void Update(Patient patient)
        {
            _context.Patients.Update(patient);
            _context.SaveChanges();
        }

        // ✅ Xóa bệnh nhân
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
using System.Collections.Generic;
using System.Linq;
using PhongKham.DAL.Entities;

namespace PhongKham.BLL.Service
{
    public class DiagnosisService
    {
        private readonly PhongKhamDbContext _context;

        public DiagnosisService(PhongKhamDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả
        public IEnumerable<Diagnosis> GetAll()
        {
            return _context.Diagnoses.ToList();
        }

        // Lấy theo ID
        public Diagnosis? GetById(int id)
        {
            return _context.Diagnoses.FirstOrDefault(d => d.DiagnosisId == id);
        }

        // Tạo mới
        public void Create(Diagnosis diagnosis)
        {
            _context.Diagnoses.Add(diagnosis);
            _context.SaveChanges();
        }

        // Cập nhật
        public void Update(Diagnosis diagnosis)
        {
            _context.Diagnoses.Update(diagnosis);
            _context.SaveChanges();
        }

        // Xóa
        public void Delete(int id)
        {
            var diag = _context.Diagnoses.FirstOrDefault(d => d.DiagnosisId == id);
            if (diag != null)
            {
                _context.Diagnoses.Remove(diag);
                _context.SaveChanges();
            }
        }
    }
}
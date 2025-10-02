using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhongKham.DAL.Entities;

namespace PhongKham.BLL.Service
{
    public class PrescriptionService
    {
        private readonly PhongKhamDbContext _context;

        public PrescriptionService(PhongKhamDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả đơn thuốc
        public IEnumerable<Prescription> GetAll()
        {
            return _context.Prescriptions
                .ToList();
        }

        // Lấy đơn thuốc theo ID
        public Prescription? GetById(int id)
        {
            return _context.Prescriptions
                .FirstOrDefault(p => p.PrescriptionId == id);
        }

        // Tạo đơn thuốc mới
        public void Create(Prescription prescription)
        {
            prescription.Usage ??= "Theo chỉ định bác sĩ";
            _context.Prescriptions.Add(prescription);
            _context.SaveChanges();
        }

        // Cập nhật đơn thuốc
        public void Update(Prescription prescription)
        {
            _context.Prescriptions.Update(prescription);
            _context.SaveChanges();
        }

        // Xóa đơn thuốc
        public void Delete(int id)
        {
            var prescription = _context.Prescriptions
                .FirstOrDefault(p => p.PrescriptionId == id);
            if (prescription != null)
            {
                _context.Prescriptions.Remove(prescription);
                _context.SaveChanges();
            }
        }
    }
}
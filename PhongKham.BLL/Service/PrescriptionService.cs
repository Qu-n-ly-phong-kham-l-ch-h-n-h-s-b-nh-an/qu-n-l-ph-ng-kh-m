using Microsoft.EntityFrameworkCore;
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

        // ✅ Lấy tất cả đơn thuốc (kèm tên thuốc)
        public IEnumerable<Prescription> GetAll()
        {
            return _context.Prescriptions
                .Include(p => p.Drug)
                .Include(p => p.Encounter)
                .ThenInclude(e => e.Appointment)
                .ThenInclude(a => a.Patient)
                .ToList();
        }

        // ✅ Lấy đơn thuốc theo ID
        public Prescription? GetById(int id)
        {
            return _context.Prescriptions
                .Include(p => p.Drug)
                .Include(p => p.Encounter)
                .ThenInclude(e => e.Appointment)
                .ThenInclude(a => a.Patient)
                .FirstOrDefault(p => p.PrescriptionId == id);
        }

        // ✅ Tạo đơn thuốc mới (kiểm tra tồn kho)
        public void Create(Prescription prescription)
        {
            var stock = _context.DrugStocks.FirstOrDefault(s => s.DrugId == prescription.DrugId);
            if (stock == null)
                throw new Exception("Thuốc không tồn tại trong kho.");

            if (stock.QuantityAvailable < prescription.Quantity)
                throw new Exception($"Không đủ thuốc trong kho. Còn lại: {stock.QuantityAvailable}");

            stock.QuantityAvailable -= prescription.Quantity;
            stock.LastUpdated = DateTime.Now;

            prescription.Usage ??= "Theo chỉ định bác sĩ";

            _context.Prescriptions.Add(prescription);
            _context.SaveChanges();
        }

        // ✅ Cập nhật đơn thuốc
        public void Update(Prescription prescription)
        {
            _context.Prescriptions.Update(prescription);
            _context.SaveChanges();
        }

        // ✅ Xóa đơn thuốc
        public void Delete(int id)
        {
            var prescription = _context.Prescriptions.FirstOrDefault(p => p.PrescriptionId == id);
            if (prescription != null)
            {
                _context.Prescriptions.Remove(prescription);
                _context.SaveChanges();
            }
        }
    }
}
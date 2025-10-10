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

        // ✅ Tìm kiếm, lọc, sắp xếp, phân trang
        public IEnumerable<Prescription> GetFiltered(
            string? keyword,
            string? drugName,
            string? patientName,
            string? sortBy,
            bool descending = false,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Prescriptions
                .Include(p => p.Drug)
                .Include(p => p.Encounter)
                .ThenInclude(e => e.Appointment)
                .ThenInclude(a => a.Patient)
                .AsQueryable();

            // 🔍 Tìm kiếm theo từ khóa (DrugName, Usage, PatientName)
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p =>
                    (p.Drug != null && p.Drug.DrugName.Contains(keyword)) ||
                    (p.Usage != null && p.Usage.Contains(keyword)) ||
                    (p.Encounter != null && p.Encounter.Appointment.Patient.FullName.Contains(keyword))
                );
            }

            // 🎯 Lọc theo tên thuốc
            if (!string.IsNullOrEmpty(drugName))
                query = query.Where(p => p.Drug != null && p.Drug.DrugName.Contains(drugName));

            // 🎯 Lọc theo tên bệnh nhân
            if (!string.IsNullOrEmpty(patientName))
                query = query.Where(p => p.Encounter.Appointment.Patient.FullName.Contains(patientName));

            // ⏫ Sắp xếp
            switch (sortBy?.ToLower())
            {
                case "drug":
                    query = descending ? query.OrderByDescending(p => p.Drug.DrugName) : query.OrderBy(p => p.Drug.DrugName);
                    break;
                case "patient":
                    query = descending ? query.OrderByDescending(p => p.Encounter.Appointment.Patient.FullName)
                                       : query.OrderBy(p => p.Encounter.Appointment.Patient.FullName);
                    break;
                case "quantity":
                    query = descending ? query.OrderByDescending(p => p.Quantity) : query.OrderBy(p => p.Quantity);
                    break;
                default:
                    query = query.OrderBy(p => p.PrescriptionId);
                    break;
            }

            // 📄 Phân trang
            return query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }


    }
}
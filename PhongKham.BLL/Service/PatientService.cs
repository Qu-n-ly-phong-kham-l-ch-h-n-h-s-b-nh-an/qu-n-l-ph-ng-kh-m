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

        // ✅ Lấy danh sách có tìm kiếm + sắp xếp + phân trang
        public object GetPaged(string? keyword, string? sortBy, string? order, int page, int pageSize)
        {
            var query = _context.Patients.AsQueryable();

            // 🔍 Tìm kiếm
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p =>
                    p.FullName.Contains(keyword) ||
                    p.Phone.Contains(keyword) ||
                    p.Email.Contains(keyword));
            }

            // 🔄 Sắp xếp
            sortBy = sortBy?.ToLower();
            order = order?.ToLower();

            switch (sortBy)
            {
                case "fullname":
                    query = (order == "desc") ? query.OrderByDescending(p => p.FullName) : query.OrderBy(p => p.FullName);
                    break;
                case "dob":
                    query = (order == "desc") ? query.OrderByDescending(p => p.Dob) : query.OrderBy(p => p.Dob);
                    break;
                default:
                    query = (order == "desc") ? query.OrderByDescending(p => p.PatientId) : query.OrderBy(p => p.PatientId);
                    break;
            }

            // 📄 Tổng số bản ghi
            int totalRecords = query.Count();

            // ⏩ Phân trang
            var data = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.PatientId,
                    p.FullName,
                    p.Dob,
                    p.Gender,
                    p.Phone,
                    p.Email,
                    p.Address
                })
                .ToList();

            // 📦 Kết quả trả về
            return new
            {
                totalRecords,
                page,
                pageSize,
                data
            };
        }

    }
}
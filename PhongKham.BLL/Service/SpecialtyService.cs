using PhongKham.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhongKham.BLL.Service
{
    public class SpecialtyService
    {
        private readonly PhongKhamDbContext _context;

        public SpecialtyService(PhongKhamDbContext context)
        {
            _context = context;
        }

        // ✅ Lấy tất cả chuyên khoa
        public IEnumerable<Specialty> GetAll()
        {
            return _context.Specialties.OrderBy(s => s.SpecialtyName).ToList();
        }

        // ✅ Lấy theo ID
        public Specialty? GetById(int id)
        {
            return _context.Specialties.FirstOrDefault(s => s.SpecialtyId == id);
        }

        // ✅ Thêm chuyên khoa (kiểm tra trùng)
        public void Create(Specialty specialty)
        {
            if (_context.Specialties.Any(s => s.SpecialtyName == specialty.SpecialtyName))
                throw new Exception("Tên chuyên khoa đã tồn tại.");

            _context.Specialties.Add(specialty);
            _context.SaveChanges();
        }

        // ✅ Cập nhật
        public void Update(Specialty specialty)
        {
            _context.Specialties.Update(specialty);
            _context.SaveChanges();
        }

        // ✅ Xóa
        public void Delete(int id)
        {
            var s = _context.Specialties.FirstOrDefault(s => s.SpecialtyId == id);
            if (s == null)
                throw new Exception("Không tìm thấy chuyên khoa để xóa.");

            _context.Specialties.Remove(s);
            _context.SaveChanges();
        }

        // ✅ Tìm kiếm, lọc, sắp xếp, phân trang
        public IEnumerable<Specialty> GetFiltered(
            string? keyword,
            string? sortBy = "name",
            bool descending = false,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Specialties.AsQueryable();

            // 🔍 Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(s => s.SpecialtyName.Contains(keyword));

            // ⏫ Sắp xếp
            switch (sortBy?.ToLower())
            {
                case "id":
                    query = descending ? query.OrderByDescending(s => s.SpecialtyId) : query.OrderBy(s => s.SpecialtyId);
                    break;
                case "name":
                default:
                    query = descending ? query.OrderByDescending(s => s.SpecialtyName) : query.OrderBy(s => s.SpecialtyName);
                    break;
            }

            // 📄 Phân trang
            return query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }
    }
}

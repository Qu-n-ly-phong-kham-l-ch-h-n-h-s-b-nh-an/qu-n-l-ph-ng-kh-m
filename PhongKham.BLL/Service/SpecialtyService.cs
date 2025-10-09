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

            Console.WriteLine($"✅ Đã thêm chuyên khoa mới: {specialty.SpecialtyName}");
        }

        // ✅ Cập nhật
        public void Update(Specialty specialty)
        {
            _context.Specialties.Update(specialty);
            _context.SaveChanges();

            Console.WriteLine($"✏️ Đã cập nhật chuyên khoa: {specialty.SpecialtyName}");
        }

        // ✅ Xóa
        public void Delete(int id)
        {
            var s = _context.Specialties.FirstOrDefault(s => s.SpecialtyId == id);
            if (s == null)
                throw new Exception("Không tìm thấy chuyên khoa để xóa.");

            _context.Specialties.Remove(s);
            _context.SaveChanges();

            Console.WriteLine($"🗑️ Đã xóa chuyên khoa: {s.SpecialtyName}");
        }
    }
}
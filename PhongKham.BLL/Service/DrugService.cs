using PhongKham.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhongKham.BLL.Service
{
    public class DrugService
    {
        private readonly PhongKhamDbContext _context;

        public DrugService(PhongKhamDbContext context)
        {
            _context = context;
        }

        // ✅ Lấy tất cả + Tìm kiếm + Lọc + Sắp xếp + Phân trang
        public IEnumerable<Drug> GetAll(
            string? keyword = null,         // tìm kiếm theo tên thuốc
            decimal? minPrice = null,       // lọc theo giá tối thiểu
            decimal? maxPrice = null,       // lọc theo giá tối đa
            string? sortBy = null,          // sắp xếp theo "name" hoặc "price"
            bool desc = false,              // sắp xếp giảm dần
            int page = 1,                   // trang hiện tại
            int pageSize = 10               // số dòng mỗi trang
        )
        {
            var query = _context.Drugs.AsQueryable();

            // 🔍 Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(d => d.DrugName.Contains(keyword));

            // 💰 Lọc theo khoảng giá
            if (minPrice.HasValue)
                query = query.Where(d => d.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(d => d.Price <= maxPrice.Value);

            // 🔃 Sắp xếp
            query = sortBy switch
            {
                "name" => desc ? query.OrderByDescending(d => d.DrugName)
                               : query.OrderBy(d => d.DrugName),
                "price" => desc ? query.OrderByDescending(d => d.Price)
                                : query.OrderBy(d => d.Price),
                _ => query.OrderBy(d => d.DrugId)
            };

            // 📄 Phân trang
            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            return query.ToList();
        }

        // ✅ Lấy thuốc theo ID
        public Drug? GetById(int id)
        {
            return _context.Drugs.FirstOrDefault(d => d.DrugId == id);
        }

        // ✅ Tạo mới thuốc
        public void Create(Drug drug)
        {
            bool exists = _context.Drugs
                .Any(d => d.DrugName.ToLower() == drug.DrugName.ToLower());
            if (exists)
                throw new Exception($"Thuốc '{drug.DrugName}' đã tồn tại!");

            _context.Drugs.Add(drug);
            _context.SaveChanges();
        }

        // ✅ Cập nhật thuốc
        public void Update(Drug drug)
        {
            var existing = _context.Drugs.FirstOrDefault(d => d.DrugId == drug.DrugId);
            if (existing == null)
                throw new Exception("Không tìm thấy thuốc để cập nhật.");

            bool nameExists = _context.Drugs
                .Any(d => d.DrugId != drug.DrugId && d.DrugName.ToLower() == drug.DrugName.ToLower());
            if (nameExists)
                throw new Exception($"Tên thuốc '{drug.DrugName}' đã tồn tại.");

            existing.DrugName = drug.DrugName;
            existing.Unit = drug.Unit;
            existing.Price = drug.Price;
            _context.SaveChanges();
        }

        // ✅ Xóa thuốc
        public void Delete(int id)
        {
            var drug = _context.Drugs.FirstOrDefault(d => d.DrugId == id);
            if (drug == null)
                throw new Exception("Không tìm thấy thuốc để xóa.");

            bool inPrescription = _context.Prescriptions.Any(p => p.DrugId == id);
            if (inPrescription)
                throw new Exception("Không thể xóa thuốc vì đang được kê trong đơn thuốc.");

            _context.Drugs.Remove(drug);
            _context.SaveChanges();
        }
    }
}

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

        // ✅ Lấy tất cả thuốc
        public IEnumerable<Drug> GetAll()
        {
            return _context.Drugs.ToList();
        }

        // ✅ Lấy thuốc theo ID
        public Drug? GetById(int id)
        {
            return _context.Drugs.FirstOrDefault(d => d.DrugId == id);
        }

        // ✅ Tạo mới thuốc
        public void Create(Drug drug)
        {
            // Kiểm tra trùng tên (case-insensitive)
            bool exists = _context.Drugs
                .Any(d => d.DrugName.ToLower() == drug.DrugName.ToLower());

            if (exists)
                throw new Exception($"Thuốc '{drug.DrugName}' đã tồn tại trong danh mục!");

            _context.Drugs.Add(drug);
            _context.SaveChanges();
        }

        // ✅ Cập nhật thuốc
        public void Update(Drug drug)
        {
            var existing = _context.Drugs.FirstOrDefault(d => d.DrugId == drug.DrugId);
            if (existing == null)
                throw new Exception("Không tìm thấy thuốc để cập nhật.");

            // Kiểm tra trùng tên (ngoại trừ bản thân)
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

            // Kiểm tra thuốc có đang nằm trong đơn thuốc nào không
            bool inPrescription = _context.Prescriptions.Any(p => p.DrugId == id);
            if (inPrescription)
                throw new Exception("Không thể xóa thuốc vì đang được kê trong đơn thuốc.");

            _context.Drugs.Remove(drug);
            _context.SaveChanges();
        }
    }
}
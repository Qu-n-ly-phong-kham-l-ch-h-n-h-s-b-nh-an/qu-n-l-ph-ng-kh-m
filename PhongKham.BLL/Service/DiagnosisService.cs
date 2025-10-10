using System;
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

        // ✅ Lấy tất cả (có lọc, tìm kiếm, sắp xếp, phân trang)
        public IEnumerable<Diagnosis> GetAll(
            string? keyword = null,
            string? sortBy = null,
            bool descending = false,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Diagnoses.AsQueryable();

            // 🔍 Tìm kiếm theo mô tả hoặc ID Encounter
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(d =>
                    d.Description.Contains(keyword) ||
                    d.EncounterId.ToString().Contains(keyword));
            }

            // 🔢 Sắp xếp
            switch (sortBy?.ToLower())
            {
                case "description":
                    query = descending ? query.OrderByDescending(d => d.Description)
                                       : query.OrderBy(d => d.Description);
                    break;
                case "encounterid":
                    query = descending ? query.OrderByDescending(d => d.EncounterId)
                                       : query.OrderBy(d => d.EncounterId);
                    break;
                default:
                    query = descending ? query.OrderByDescending(d => d.DiagnosisId)
                                       : query.OrderBy(d => d.DiagnosisId);
                    break;
            }

            // 📄 Phân trang
            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            return query.ToList();
        }

        // ✅ Lấy theo ID
        public Diagnosis? GetById(int id)
        {
            return _context.Diagnoses.FirstOrDefault(d => d.DiagnosisId == id);
        }

        // ✅ Tạo mới
        public void Create(Diagnosis diagnosis)
        {
            _context.Diagnoses.Add(diagnosis);
            _context.SaveChanges();
        }

        // ✅ Cập nhật
        public void Update(Diagnosis diagnosis)
        {
            _context.Diagnoses.Update(diagnosis);
            _context.SaveChanges();
        }

        // ✅ Xóa
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

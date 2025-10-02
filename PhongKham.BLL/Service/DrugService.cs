using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhongKham.DAL.Entities;

namespace PhongKham.BLL.Service
{
    public class DrugService
    {
        private readonly PhongKhamDbContext _context;

        public DrugService(PhongKhamDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả thuốc
        public IEnumerable<Drug> GetAll()
        {
            return _context.Drugs.ToList();
        }

        // Lấy 1 thuốc theo ID
        public Drug? GetById(int id)
        {
            return _context.Drugs.FirstOrDefault(d => d.DrugId == id);
        }

        // Tạo mới
        public void Create(Drug drug)
        {
            _context.Drugs.Add(drug);
            _context.SaveChanges();
        }

        // Cập nhật
        public void Update(Drug drug)
        {
            _context.Drugs.Update(drug);
            _context.SaveChanges();
        }

        // Xóa
        public void Delete(int id)
        {
            var drug = _context.Drugs.FirstOrDefault(d => d.DrugId == id);
            if (drug != null)
            {
                _context.Drugs.Remove(drug);
                _context.SaveChanges();
            }
        }
    }
}

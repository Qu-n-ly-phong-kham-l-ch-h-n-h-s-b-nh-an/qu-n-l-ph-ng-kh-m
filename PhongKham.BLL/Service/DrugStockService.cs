using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhongKham.DAL.Entities;

namespace PhongKham.BLL.Service
{
    public class DrugStockService
    {
        private readonly PhongKhamDbContext _context;

        public DrugStockService(PhongKhamDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả tồn kho thuốc
        public IEnumerable<DrugStock> GetAll()
        {
            return _context.DrugStocks
                           .OrderByDescending(ds => ds.LastUpdated)
                           .ToList();
        }

        // Lấy tồn kho theo ID
        public DrugStock? GetById(int id)
        {
            return _context.DrugStocks.FirstOrDefault(ds => ds.StockId == id);
        }

        // Tạo mới
        public void Create(DrugStock stock)
        {
            stock.LastUpdated = DateTime.Now;
            _context.DrugStocks.Add(stock);
            _context.SaveChanges();
        }

        // Cập nhật
        public void Update(DrugStock stock)
        {
            stock.LastUpdated = DateTime.Now;
            _context.DrugStocks.Update(stock);
            _context.SaveChanges();
        }

        // Xóa
        public void Delete(int id)
        {
            var stock = _context.DrugStocks.FirstOrDefault(ds => ds.StockId == id);
            if (stock != null)
            {
                _context.DrugStocks.Remove(stock);
                _context.SaveChanges();
            }
        }
    }
}
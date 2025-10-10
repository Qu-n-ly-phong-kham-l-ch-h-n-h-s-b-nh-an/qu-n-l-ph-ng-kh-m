using Microsoft.EntityFrameworkCore;
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

        // ✅ Lấy tất cả tồn kho thuốc (có thông tin thuốc)
        public IEnumerable<DrugStock> GetAll()
        {
            return _context.DrugStocks
                .Include(ds => ds.Drug)
                .OrderByDescending(ds => ds.LastUpdated)
                .ToList();
        }

        // ✅ Lấy theo ID
        public DrugStock? GetById(int id)
        {
            return _context.DrugStocks
                .Include(ds => ds.Drug)
                .FirstOrDefault(ds => ds.StockId == id);
        }

        // ✅ Tạo mới (kèm kiểm tra thuốc tồn tại)
        public void Create(DrugStock stock)
        {
            var drugExists = _context.Drugs.Any(d => d.DrugId == stock.DrugId);
            if (!drugExists)
                throw new Exception("❌ Thuốc không tồn tại trong danh mục.");

            stock.LastUpdated = DateTime.Now;
            _context.DrugStocks.Add(stock);
            _context.SaveChanges();

            Console.WriteLine($"✅ Thêm mới tồn kho cho thuốc ID {stock.DrugId}");
        }

        // ✅ Cập nhật (có cảnh báo số lượng thấp)
        public void Update(DrugStock stock)
        {
            _context.DrugStocks.Update(stock);
            stock.LastUpdated = DateTime.Now;
            _context.SaveChanges();

            if (stock.QuantityAvailable < 10)
                Console.WriteLine($"⚠️ Cảnh báo: Thuốc {stock.Drug?.DrugName ?? stock.DrugId.ToString()} sắp hết!");
        }

        // ✅ Xóa tồn kho
        public void Delete(int id)
        {
            var stock = _context.DrugStocks.FirstOrDefault(ds => ds.StockId == id);
            if (stock == null)
                throw new Exception("Không tìm thấy bản ghi tồn kho để xóa.");

            _context.DrugStocks.Remove(stock);
            _context.SaveChanges();
        }

        public IEnumerable<DrugStock> Search(string? keyword, int? minQty, int? maxQty, int page = 1, int pageSize = 10)
        {
            var query = _context.DrugStocks.Include(ds => ds.Drug).AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(ds => ds.Drug.DrugName.Contains(keyword));

            if (minQty.HasValue)
                query = query.Where(ds => ds.QuantityAvailable >= minQty.Value);

            if (maxQty.HasValue)
                query = query.Where(ds => ds.QuantityAvailable <= maxQty.Value);

            return query
                .OrderByDescending(ds => ds.LastUpdated)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

    }
}
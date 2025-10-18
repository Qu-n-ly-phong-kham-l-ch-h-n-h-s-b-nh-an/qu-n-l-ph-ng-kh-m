// File: BLL/DrugBLL.cs
using QuanLyPhongKhamApi.DAL;
using QuanLyPhongKhamApi.Models;
using System; // Thêm using System nếu chưa có

namespace QuanLyPhongKhamApi.BLL
{
    public class DrugBLL
    {
        private readonly DrugDAL _dal;
        public DrugBLL(DrugDAL dal) { _dal = dal; }

        public List<Drug> GetAll() => _dal.GetAll();
        public Drug? GetById(int id) => _dal.GetById(id);

        public int Create(DrugRequest req, int createdBy)
        {
            if (_dal.NameExists(req.DrugName))
            {
                throw new ApplicationException("Tên thuốc đã tồn tại.");
            }
            // Hàm Create của DAL đã tự khởi tạo tồn kho
            return _dal.Create(req, createdBy);
        }

        public bool Update(int id, DrugRequest req)
        {
            if (_dal.NameExists(req.DrugName, id))
            {
                throw new ApplicationException("Tên thuốc đã tồn tại.");
            }
            return _dal.Update(id, req);
        }

        public bool Delete(int id) => _dal.SoftDelete(id);

        public List<DrugStockDTO> GetStockReport() => _dal.GetStockReport();

        // Xử lý logic import/export để tính quantityChange
        public bool AdjustStock(StockAdjustRequest req)
        {
            // Validation req.Quantity > 0 đã được Model Binding xử lý
            // Validation req.Type là 'import'/'export' đã được Model Binding xử lý

            // Tính toán quantityChange dựa trên Type
            int quantityChange = req.Type.ToLower() == "import" ? req.Quantity : -req.Quantity;

            // Gọi DAL với quantityChange đã tính toán
            return _dal.AdjustStock(req.DrugID, quantityChange);
        }
    }
}
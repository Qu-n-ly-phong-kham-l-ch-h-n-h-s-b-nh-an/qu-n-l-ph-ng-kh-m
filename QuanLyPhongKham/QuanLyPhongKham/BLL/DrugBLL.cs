// File: BLL/DrugBLL.cs
using QuanLyPhongKhamApi.DAL;
using QuanLyPhongKhamApi.Models;

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
            var newDrugId = _dal.Create(req, createdBy);
            // Tự động khởi tạo tồn kho bằng 0 khi tạo thuốc mới
            _dal.AdjustStock(newDrugId, 0);
            return newDrugId;
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

        public bool AdjustStock(StockAdjustRequest req)
        {
            int quantityChange = req.Type == "import" ? req.Quantity : -req.Quantity;
            return _dal.AdjustStock(req.DrugID, quantityChange);
        }
    }
}
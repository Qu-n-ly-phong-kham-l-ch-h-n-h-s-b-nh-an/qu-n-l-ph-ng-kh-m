// File: BLL/SpecialtyBLL.cs
using QuanLyPhongKhamApi.DAL;
using QuanLyPhongKhamApi.Models;

namespace QuanLyPhongKhamApi.BLL
{
    public class SpecialtyBLL
    {
        private readonly SpecialtyDAL _dal;
        public SpecialtyBLL(SpecialtyDAL dal)
        {
            _dal = dal;
        }

        public List<Specialty> GetAll() => _dal.GetAll();

        public Specialty? GetById(int id) => _dal.GetById(id);

        public int Create(SpecialtyRequest req)
        {
            // Logic nghiệp vụ: Không cho phép tạo chuyên khoa có tên trùng lặp
            if (_dal.NameExists(req.SpecialtyName))
            {
                throw new ApplicationException("Tên chuyên khoa đã tồn tại.");
            }
            return _dal.Create(req);
        }

        public bool Update(int id, SpecialtyRequest req)
        {
            // Logic nghiệp vụ: Không cho phép đổi tên thành một chuyên khoa khác đã tồn tại
            if (_dal.NameExists(req.SpecialtyName, id))
            {
                throw new ApplicationException("Tên chuyên khoa đã tồn tại.");
            }
            return _dal.Update(id, req);
        }

        public bool Delete(int id) => _dal.SoftDelete(id);
    }
}
// File: BLL/DoctorBLL.cs
using QuanLyPhongKhamApi.DAL;
using QuanLyPhongKhamApi.Models;

namespace QuanLyPhongKhamApi.BLL
{
    public class DoctorBLL
    {
        private readonly DoctorDAL _dal;
        private readonly AccountBLL _accountBLL;

        public DoctorBLL(DoctorDAL dal, AccountBLL accountBLL)
        {
            _dal = dal;
            _accountBLL = accountBLL;
        }

        public List<DoctorDTO> GetAll() => _dal.GetAll();

        // ✅ BỔ SUNG: Thêm hàm GetById để khắc phục lỗi
        public DoctorDTO? GetById(int id) => _dal.GetById(id);

        public int Create(DoctorCreateRequest req)
        {
            // Logic nghiệp vụ phức tạp: Tạo tài khoản và hồ sơ trong một giao dịch.
            // Nếu một trong hai bước thất bại, bước trước đó phải được hoàn tác.
            int newAccountId = 0;
            try
            {
                newAccountId = _accountBLL.Register(req.AccountUsername, req.AccountPassword, "Doctor");

                if (newAccountId <= 0)
                {
                    throw new ApplicationException("Tạo tài khoản cho bác sĩ thất bại.");
                }

                return _dal.Create(req, newAccountId);
            }
            catch (Exception)
            {
                // Hoàn tác: Nếu đã tạo tài khoản nhưng tạo hồ sơ bác sĩ thất bại,
                // hãy xóa tài khoản vừa tạo để tránh rác dữ liệu.
                if (newAccountId > 0)
                {
                    _accountBLL.Delete(newAccountId);
                }
                // Ném lại lỗi để Middleware bắt và xử lý
                throw;
            }
        }

        public bool Update(int id, DoctorUpdateRequest req) => _dal.Update(id, req);

        // ✅ BỔ SUNG: Thêm hàm Delete để khắc phục lỗi
        public bool Delete(int id) => _dal.SoftDelete(id);
    }
}
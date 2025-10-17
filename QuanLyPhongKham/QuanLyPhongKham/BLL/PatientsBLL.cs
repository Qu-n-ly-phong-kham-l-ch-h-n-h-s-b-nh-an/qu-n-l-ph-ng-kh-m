using QuanLyPhongKhamApi.DAL;
using QuanLyPhongKhamApi.Models;
using System;
using System.Collections.Generic;

namespace QuanLyPhongKhamApi.BLL
{
    public class PatientBLL
    {
        private readonly PatientDAL _dal;
        private readonly AccountBLL _accountBLL;

        public PatientBLL(PatientDAL dal, AccountBLL accountBLL)
        {
            _dal = dal;
            _accountBLL = accountBLL;
        }

        public List<Patient> GetAll() => _dal.GetAll();
        public Patient? GetById(int id) => _dal.GetById(id);
        public Patient? GetByAccountId(int accountId) => _dal.GetByAccountId(accountId);

        // HÀM CREATE ĐÃ ĐƯỢC CẬP NHẬT LOGIC
        public int Create(PatientCreateRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.FullName))
                throw new ArgumentException("Họ tên không được để trống.");

            int? finalAccountId = null;

            // Ưu tiên 1: Kiểm tra xem có AccountID được gửi lên để liên kết không
            if (req.AccountID.HasValue && req.AccountID > 0)
            {
                // TODO (Nâng cao): Kiểm tra xem AccountID này đã được liên kết với bệnh nhân khác chưa
                finalAccountId = req.AccountID;
            }
            // Ưu tiên 2: Nếu không, kiểm tra xem có thông tin để tạo tài khoản mới không
            else if (!string.IsNullOrWhiteSpace(req.AccountUsername) && !string.IsNullOrWhiteSpace(req.AccountPassword))
            {
                try
                {
                    finalAccountId = _accountBLL.Register(req.AccountUsername, req.AccountPassword, "Patient");
                }
                catch (ApplicationException ex)
                {
                    throw new ApplicationException($"Lỗi đăng ký tài khoản: {ex.Message}");
                }
            }

            // Cuối cùng, gọi DAL để tạo hồ sơ bệnh nhân với AccountID đã xác định (hoặc null)
            int newPatientId = _dal.Create(req, finalAccountId);

            if (newPatientId <= 0)
            {
                // Hoàn tác: Nếu tạo hồ sơ thất bại nhưng đã lỡ tạo tài khoản, hãy xóa tài khoản đó
                if (finalAccountId.HasValue && !req.AccountID.HasValue) // Chỉ xóa nếu là tài khoản mới tạo
                {
                    _accountBLL.Delete(finalAccountId.Value);
                }
                throw new ApplicationException("Tạo hồ sơ bệnh nhân thất bại.");
            }

            return newPatientId;
        }

        public bool Update(int id, PatientUpdateRequest req)
        {
            if (id <= 0) throw new ArgumentException("PatientID không hợp lệ.");
            if (string.IsNullOrWhiteSpace(req.FullName))
                throw new ArgumentException("Họ tên không được để trống.");

            // 1. Lấy thông tin bệnh nhân hiện tại từ CSDL
            var patientInDb = _dal.GetById(id);
            if (patientInDb == null)
            {
                return false; // Trả về false nếu không tìm thấy bệnh nhân
            }

            // 2. Cập nhật các trường từ dữ liệu request (req) vào đối tượng đã lấy từ DB (patientInDb)
            patientInDb.FullName = req.FullName;
            patientInDb.DOB = req.DOB;
            patientInDb.Gender = req.Gender;
            patientInDb.Phone = req.Phone;
            patientInDb.Email = req.Email;
            patientInDb.Address = req.Address;
            patientInDb.MedicalHistory = req.MedicalHistory;
            patientInDb.AccountID = req.AccountID; // Gán AccountID từ request

            // 3. Gọi DAL để lưu đối tượng đã được cập nhật đầy đủ
            return _dal.Update(patientInDb);
        }

        public bool Delete(int id)
        {
            if (id <= 0) throw new ArgumentException("PatientID không hợp lệ.");
            return _dal.SoftDelete(id);
        }
    }
}
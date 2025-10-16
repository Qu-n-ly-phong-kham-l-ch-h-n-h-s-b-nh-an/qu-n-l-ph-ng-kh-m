// File: BLL/PatientBLL.cs
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

        // ✅ BỔ SUNG: Thêm hàm GetByAccountId để khắc phục lỗi
        public Patient? GetByAccountId(int accountId) => _dal.GetByAccountId(accountId);

        public int Create(PatientCreateRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.FullName))
                throw new ArgumentException("Họ tên không được để trống.");

            int? newAccountId = null;

            if (!string.IsNullOrWhiteSpace(req.AccountUsername) && !string.IsNullOrWhiteSpace(req.AccountPassword))
            {
                // ApplicationException sẽ được Middleware bắt và xử lý
                newAccountId = _accountBLL.Register(req.AccountUsername, req.AccountPassword, "Patient");
            }

            int newPatientId = _dal.Create(req, newAccountId);

            if (newPatientId <= 0)
            {
                // Nếu tạo hồ sơ thất bại, hoàn tác việc tạo tài khoản
                if (newAccountId.HasValue)
                {
                    _accountBLL.Delete(newAccountId.Value);
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

            return _dal.Update(id, req);
        }

        public bool Delete(int id)
        {
            if (id <= 0) throw new ArgumentException("PatientID không hợp lệ.");
            return _dal.SoftDelete(id);
        }
    }
}
using QuanLyPhongKhamApi.DAL;
using QuanLyPhongKhamApi.Models;
using System.Collections.Generic;
using System;
using System.Linq;

namespace QuanLyPhongKhamApi.BLL
{
    public class EncounterBLL
    {
        private readonly EncounterDAL _dal;
        private readonly DoctorDAL _doctorDal; // <-- THAY ĐỔI 1

        // Sửa constructor để inject DoctorDAL
        public EncounterBLL(EncounterDAL dal, DoctorDAL doctorDal) // <-- THAY ĐỔI 2
        {
            _dal = dal;
            _doctorDal = doctorDal; // <-- THAY ĐỔI 2
        }

        public List<EncounterDTO> GetAllEncounters()
        {
            return _dal.GetAllEncounters();
        }

        public EncounterDTO? GetEncounterById(int id)
        {
            return _dal.GetEncounterById(id);
        }

        public int CompleteEncounter(int doctorAccountId, CompleteEncounterRequest request)
        {
            // =============================================================
            // THAY ĐỔI 3: LOGIC LẤY DOCTORID ĐÃ ĐƯỢC SỬA LẠI HOÀN TOÀN
            // =============================================================

            // 1. Dùng AccountID của bác sĩ đang đăng nhập để tìm hồ sơ Bác sĩ tương ứng
            var doctorProfile = _doctorDal.GetByAccountId(doctorAccountId);
            if (doctorProfile == null)
            {
                // Ném ra lỗi nếu tài khoản bác sĩ không được liên kết với hồ sơ nào
                throw new ApplicationException("Tài khoản đăng nhập không được liên kết với bất kỳ hồ sơ bác sĩ nào.");
            }

            // 2. Lấy DoctorID chính xác từ hồ sơ bác sĩ đã tìm thấy
            var doctorId = doctorProfile.DoctorID;

            // =============================================================

            // Phần code validation bên dưới giữ nguyên
            if (request.ServiceFee < 0)
            {
                throw new ArgumentException("Phí dịch vụ không được âm.");
            }

















            if (request.PrescriptionItems != null)
            {
                if (request.PrescriptionItems.Any(item => item.Quantity <= 0))
                {
                    throw new ArgumentException("Số lượng thuốc trong đơn phải lớn hơn 0.");
                }
            }

            List<PrescriptionItemDTO> items = request.PrescriptionItems ?? new List<PrescriptionItemDTO>();

            try
            {
                // Gọi xuống DAL với DoctorID chính xác
                return _dal.CompleteEncounter(
                    request.AppointmentID,
                    doctorId, // <-- DoctorID chính xác đã được lấy ở trên
                    request.ExaminationNotes,
                    request.DiagnosisDescription,
                    request.ServiceFee,
                    items,
                    doctorAccountId // CurrentUserID vẫn là AccountID của người thực hiện
                );
            }
            catch (ApplicationException ex)
            {
                // Bắt lỗi nghiệp vụ từ SP (Thiếu tồn kho, Cuộc hẹn không hợp lệ) và throw lại
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public bool DeleteEncounter(int id)
        {
            return _dal.SoftDelete(id);
        }
    }
}
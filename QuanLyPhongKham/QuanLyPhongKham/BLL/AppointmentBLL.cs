// File: BLL/AppointmentBLL.cs
using QuanLyPhongKhamApi.DAL;
using QuanLyPhongKhamApi.Models;

namespace QuanLyPhongKhamApi.BLL
{
    public class AppointmentBLL
    {
        private readonly AppointmentDAL _dal;

        public AppointmentBLL(AppointmentDAL dal)
        {
            _dal = dal;
        }

        public List<AppointmentDTO> GetAll(int? doctorId, int? patientId, DateTime? date, string? status)
        {
            return _dal.GetAll(doctorId, patientId, date, status);
        }

        public AppointmentDTO? GetById(int id) => _dal.GetById(id);

        public int Create(AppointmentCreateRequest req, int createdByAccountId)
        {
            // <-- KẾT HỢP: Dùng logic validation thực tế hơn
            if (req.AppointmentDate < DateTime.Now.AddMinutes(15))
            {
                throw new ArgumentException("Thời gian hẹn phải sau thời gian hiện tại ít nhất 15 phút.");
            }

            // <-- KẾT HỢP: Logic nghiệp vụ cốt lõi: Kiểm tra trùng lịch
            if (_dal.CheckScheduleConflict(req.DoctorID, req.AppointmentDate))
            {
                throw new ApplicationException("Bác sĩ đã có lịch hẹn vào thời điểm này. Vui lòng chọn thời gian khác.");
            }

            var newId = _dal.Create(req, createdByAccountId);
            if (newId <= 0)
            {
                throw new ApplicationException("Tạo lịch hẹn thất bại.");
            }
            return newId;
        }

        // <-- KẾT HỢP: Hàm Cancel với logic nghiệp vụ an toàn
        public bool Cancel(int id)
        {
            var appointment = _dal.GetById(id);
            if (appointment == null)
            {
                return false; // Not Found
            }
            if (appointment.Status == "Đã khám")
            {
                throw new ApplicationException("Không thể hủy lịch hẹn đã hoàn thành.");
            }
            if (appointment.Status == "Đã hủy")
            {
                // Không cần báo lỗi, coi như đã thành công
                return true;
            }
            return _dal.UpdateStatus(id, "Đã hủy");
        }

        public bool UpdateStatus(int id, string status)
        {
            var appointment = _dal.GetById(id);
            if (appointment == null) return false; // Not Found

            // Logic phòng ngừa: không cho phép cập nhật trạng thái của lịch đã hủy/đã khám
            if (appointment.Status == "Đã hủy" || appointment.Status == "Đã khám")
            {
                throw new ApplicationException($"Không thể thay đổi trạng thái của lịch hẹn đã '{appointment.Status}'.");
            }
            return _dal.UpdateStatus(id, status);
        }

        public bool Delete(int id) => _dal.SoftDelete(id);

        public bool Update(int id, AppointmentUpdateRequest req)
        {
            // 1. Lấy thông tin lịch hẹn hiện tại
            var existingAppointment = _dal.GetById(id);
            if (existingAppointment == null)
            {
                return false; // Not Found
            }

            // 2. Kiểm tra trạng thái: Không cho sửa lịch hẹn đã hoàn tất hoặc đã hủy
            if (existingAppointment.Status == "Đã khám" || existingAppointment.Status == "Đã hủy")
            {
                throw new ApplicationException($"Không thể sửa lịch hẹn đã ở trạng thái '{existingAppointment.Status}'.");
            }

            // 3. Kiểm tra thời gian hợp lệ
            if (req.AppointmentDate < DateTime.Now.AddMinutes(15))
            {
                throw new ArgumentException("Thời gian hẹn mới phải sau thời gian hiện tại ít nhất 15 phút.");
            }

            // 4. KIỂM TRA LẠI TRÙNG LỊCH (Logic cốt lõi)
            // Chỉ kiểm tra nếu bác sĩ hoặc thời gian bị thay đổi
            if (existingAppointment.DoctorID != req.DoctorID || existingAppointment.AppointmentDate != req.AppointmentDate)
            {
                // Khi kiểm tra trùng lịch, phải loại trừ chính lịch hẹn đang sửa
                // Cách đơn giản là kiểm tra trực tiếp ở đây, hoặc nâng cấp hàm CheckScheduleConflict để truyền vào ID cần loại trừ
                if (_dal.CheckScheduleConflict(req.DoctorID, req.AppointmentDate))
                {
                    throw new ApplicationException("Bác sĩ đã có lịch hẹn vào thời điểm mới. Vui lòng chọn thời gian khác.");
                }
            }

            // 5. Nếu mọi thứ hợp lệ, gọi DAL để cập nhật
            return _dal.Update(id, req);
        }
    }  }
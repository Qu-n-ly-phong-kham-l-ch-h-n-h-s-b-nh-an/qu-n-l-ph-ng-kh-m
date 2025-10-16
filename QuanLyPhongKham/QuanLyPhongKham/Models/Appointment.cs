
using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyPhongKhamApi.Models
{
    // Model chính ánh xạ bảng Appointments trong CSDL
    public class Appointment
    {
        public int AppointmentID { get; set; }
        public int PatientID { get; set; }
        public int DoctorID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedByAccountID { get; set; }
        public bool IsDeleted { get; set; }
    }

    // DTO để hiển thị danh sách lịch hẹn (thân thiện hơn cho frontend)
    public class AppointmentDTO
    {
        public int AppointmentID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int PatientID { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int DoctorID { get; set; }
        public string DoctorName { get; set; } = string.Empty;
    }

    // DTO cho request tạo mới lịch hẹn
    public class AppointmentCreateRequest
    {
        [Required(ErrorMessage = "Mã bệnh nhân là bắt buộc.")]
        public int PatientID { get; set; }

        [Required(ErrorMessage = "Mã bác sĩ là bắt buộc.")]
        public int DoctorID { get; set; }

        [Required(ErrorMessage = "Thời gian hẹn là bắt buộc.")]
        public DateTime AppointmentDate { get; set; }

        public string? Notes { get; set; }
    }

    // DTO cho request cập nhật trạng thái
    public class AppointmentStatusUpdateRequest
    {
        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        // <-- KẾT HỢP: Dùng Regex để giới hạn các trạng thái hợp lệ
        [RegularExpression("^(Đã xác nhận|Đã khám|Đã hủy)$", ErrorMessage = "Trạng thái cập nhật không hợp lệ.")]
        public string Status { get; set; } = string.Empty;
    }
    public class AppointmentUpdateRequest
    {
        [Required(ErrorMessage = "Mã bác sĩ là bắt buộc.")]
        public int DoctorID { get; set; }

        [Required(ErrorMessage = "Thời gian hẹn là bắt buộc.")]
        public DateTime AppointmentDate { get; set; }

        public string? Notes { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace QuanLyPhongKhamApi.Models
{
    // DTO cho từng loại thuốc trong đơn (Phần TVP)
    public class PrescriptionItemDTO
    {
        [Required(ErrorMessage = "ID thuốc là bắt buộc.")]
        public int DrugID { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc.")]
        [Range(1, 1000, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int Quantity { get; set; }

        public string? Usage { get; set; } // Liều dùng/cách dùng
    }

    // DTO dùng cho Request hoàn tất lần khám (SẠCH: KHÔNG CÓ DoctorID/CurrentUserID)
    public class CompleteEncounterRequest
    {
        [Required(ErrorMessage = "Mã cuộc hẹn là bắt buộc.")]
        public int AppointmentID { get; set; }

        [Required(ErrorMessage = "Ghi chú khám bệnh là bắt buộc.")]
        public string ExaminationNotes { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả chẩn đoán là bắt buộc.")]
        public string DiagnosisDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phí dịch vụ là bắt buộc.")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Phí dịch vụ không hợp lệ.")]
        public decimal ServiceFee { get; set; }

        // Danh sách thuốc được kê đơn
        public List<PrescriptionItemDTO>? PrescriptionItems { get; set; }
    }

    // Model/DTO cho việc hiển thị Lần khám
    public class EncounterDTO
    {
        public int EncounterID { get; set; }
        public int AppointmentID { get; set; }
        public int DoctorID { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string ExaminationNotes { get; set; } = string.Empty;
        public DateTime EncounterDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}

// File: Models/Invoice.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyPhongKhamApi.Models
{
    // Model chính cho bảng Invoices
    public class Invoice
    {
        public int InvoiceID { get; set; }
        public int PatientID { get; set; }
        public int EncounterID { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal DrugFee { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? CreatedByAccountID { get; set; }
    }

    // DTO để hiển thị danh sách hóa đơn
    public class InvoiceDTO
    {
        public int InvoiceID { get; set; }
        public int PatientID { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int EncounterID { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }


    // DTO cho request xác nhận thanh toán
    public class PaymentRequest
    {
        [Required(ErrorMessage = "Phương thức thanh toán không được để trống.")]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;
    }

    // ✅ BỔ SUNG: DTO cho chi tiết hóa đơn
    public class InvoiceDetailDTO : InvoiceDTO
    {
        public DateTime EncounterDate { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string ExaminationNotes { get; set; } = string.Empty;
        public string DiagnosisDescription { get; set; } = string.Empty;
        public decimal ServiceFee { get; set; }
        public decimal DrugFee { get; set; }
        public List<PrescriptionItemDetailDTO> PrescriptionItems { get; set; } = new();
    }

    public class PrescriptionItemDetailDTO
    {
        public string DrugName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? Usage { get; set; }
    }
}
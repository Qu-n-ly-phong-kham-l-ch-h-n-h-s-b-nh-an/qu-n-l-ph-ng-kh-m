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
}
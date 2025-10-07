namespace PhongKham.API.Models.DTOs
{
    public class InvoiceRequestDTO
    {
        public int PatientId { get; set; }
        public int EncounterId { get; set; }
        public decimal TotalAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Status { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}
namespace PhongKham.API.Models.DTOs
{
    public class InvoiceResponseDTO
    {
        public int InvoiceId { get; set; }
        public string? PatientName { get; set; }
        public int EncounterId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Status { get; set; }
    }
}
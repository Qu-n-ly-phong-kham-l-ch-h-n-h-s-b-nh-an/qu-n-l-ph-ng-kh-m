namespace PhongKham.API.Models.DTOs
{
    public class PrescriptionRequestDTO
    {
        public int EncounterId { get; set; }
        public int DrugId { get; set; }
        public int Quantity { get; set; }
        public string? Usage { get; set; }
    }

    public class PrescriptionResponseDTO
    {
        public int PrescriptionId { get; set; }
        public string DrugName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Usage { get; set; }
    }
}
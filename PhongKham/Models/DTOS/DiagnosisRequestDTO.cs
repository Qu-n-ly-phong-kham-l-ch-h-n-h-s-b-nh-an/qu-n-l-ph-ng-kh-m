namespace PhongKham.API.Models.DTOs
{
    public class DiagnosisRequestDTO
    {
        public int EncounterId { get; set; }
        public string? Description { get; set; }
        public byte[]? ResultFile { get; set; } // Có thể null
    }

    public class DiagnosisResponseDTO
    {
        public int DiagnosisId { get; set; }
        public int EncounterId { get; set; }
        public string? Description { get; set; }
        public bool HasResultFile { get; set; } // Chỉ để biết có file kết quả hay không
    }
}
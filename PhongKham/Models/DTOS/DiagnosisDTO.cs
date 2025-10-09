namespace PhongKham.API.Models.DTOs
{
    public class DiagnosisDTO
    {
        public int DiagnosisId { get; set; }
        public int EncounterId { get; set; }
        public string? Description { get; set; }
        public byte[]? ResultFile { get; set; } // file kết quả (nếu có)
        public bool HasResultFile { get; set; }
    }
}
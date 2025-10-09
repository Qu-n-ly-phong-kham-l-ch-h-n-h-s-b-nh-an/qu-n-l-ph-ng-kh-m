namespace PhongKham.API.Models.DTOs
{
    public class PatientDTO
    {
        public int PatientId { get; set; }
        public string? FullName { get; set; }
        public DateTime? Dob { get; set; }   // ✅ Đổi sang DateTime để dễ thao tác
        public string? Gender { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? MedicalHistory { get; set; }
        public int? AccountId { get; set; }
    }
}
namespace PhongKham.API.Models.DTOs
{
    public class DoctorResponseDTO
    {
        public int DoctorId { get; set; }
        public string? FullName { get; set; }
        public string? SpecialtyName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
}

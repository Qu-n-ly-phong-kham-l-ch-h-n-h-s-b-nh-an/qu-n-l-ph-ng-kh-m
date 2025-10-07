namespace PhongKham.API.Models.DTOs
{
    public class DoctorRequestDTO
    {
        public string FullName { get; set; } = null!;
        public int SpecialtyId { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
}

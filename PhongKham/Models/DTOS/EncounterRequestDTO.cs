namespace PhongKham.API.Models.DTOs
{
    public class EncounterRequestDTO
    {
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public string? Notes { get; set; }
    }
}
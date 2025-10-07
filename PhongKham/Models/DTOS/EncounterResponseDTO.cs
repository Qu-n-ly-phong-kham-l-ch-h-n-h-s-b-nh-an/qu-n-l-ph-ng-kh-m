namespace PhongKham.API.Models.DTOs
{
    public class EncounterResponseDTO
    {
        public int EncounterId { get; set; }
        public string? DoctorName { get; set; }
        public string? PatientName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Notes { get; set; }
    }
}
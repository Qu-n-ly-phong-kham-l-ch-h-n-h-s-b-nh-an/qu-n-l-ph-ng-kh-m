namespace PhongKham.API.Models.DTOs
{
    public class AppointmentResponseDTO
    {
        public int AppointmentId { get; set; }
        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Status { get; set; }
    }
}

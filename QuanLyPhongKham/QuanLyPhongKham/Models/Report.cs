// File: Models/Report.cs
namespace QuanLyPhongKhamApi.Models
{
    // DTO cho báo cáo doanh thu theo bác sĩ
    public class DoctorRevenueReport
    {
        public int DoctorID { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string? SpecialtyName { get; set; }
        public int TotalEncounters { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageFee { get; set; }
    }
}
// File: BLL/ReportBLL.cs
using QuanLyPhongKhamApi.DAL;
using QuanLyPhongKhamApi.Models;

namespace QuanLyPhongKhamApi.BLL
{
    public class ReportBLL
    {
        private readonly ReportDAL _dal;
        public ReportBLL(ReportDAL dal) { _dal = dal; }

        public List<DoctorRevenueReport> GetDoctorRevenue(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
            }
            return _dal.GetDoctorRevenue(startDate, endDate);
        }
    }
}
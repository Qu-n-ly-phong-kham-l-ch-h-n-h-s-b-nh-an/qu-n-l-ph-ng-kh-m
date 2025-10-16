// File: DAL/ReportDAL.cs
using Dapper; // Thêm Dapper
using Microsoft.Data.SqlClient;
using QuanLyPhongKhamApi.Models;
using System.Data;

namespace QuanLyPhongKhamApi.DAL
{
    public class ReportDAL
    {
        private readonly string _conn;
        public ReportDAL(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection")!;
        }

        public List<DoctorRevenueReport> GetDoctorRevenue(DateTime startDate, DateTime endDate)
        {
            using var connection = new SqlConnection(_conn);

            // Dapper sẽ tự động map tên tham số của Stored Procedure với thuộc tính của object
            var parameters = new { StartDate = startDate, EndDate = endDate };

            // Gọi Stored Procedure và Dapper sẽ tự động map kết quả vào list DoctorRevenueReport
            return connection.Query<DoctorRevenueReport>(
                "sp_report_revenue_by_doctor",
                parameters,
                commandType: CommandType.StoredProcedure
            ).ToList();
        }
    }
}
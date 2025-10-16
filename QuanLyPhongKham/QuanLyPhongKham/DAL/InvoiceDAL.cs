// File: DAL/InvoiceDAL.cs
using Dapper; // Thêm Dapper
using Microsoft.Data.SqlClient;
using QuanLyPhongKhamApi.Models;
using System.Data;

namespace QuanLyPhongKhamApi.DAL
{
    public class InvoiceDAL
    {
        private readonly string _conn;
        public InvoiceDAL(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection")!;
        }

        public List<InvoiceDTO> GetAll(string? status)
        {
            using var connection = new SqlConnection(_conn);
            var sql = @"SELECT i.InvoiceID, i.PatientID, p.FullName AS PatientName, i.EncounterID, i.TotalAmount, i.Status, i.CreatedAt
                        FROM Invoices i
                        INNER JOIN Patients p ON i.PatientID = p.PatientID
                        WHERE 1=1";

            var parameters = new DynamicParameters();
            if (!string.IsNullOrEmpty(status))
            {
                sql += " AND i.Status = @status";
                parameters.Add("status", status);
            }
            sql += " ORDER BY i.CreatedAt DESC";

            return connection.Query<InvoiceDTO>(sql, parameters).ToList();
        }

        // ✅ HOÀN THIỆN: Viết đầy đủ hàm GetById
        public InvoiceDTO? GetById(int id)
        {
            using var connection = new SqlConnection(_conn);
            var sql = @"SELECT i.InvoiceID, i.PatientID, p.FullName AS PatientName, i.EncounterID, i.TotalAmount, i.Status, i.CreatedAt
                        FROM Invoices i
                        INNER JOIN Patients p ON i.PatientID = p.PatientID
                        WHERE i.InvoiceID = @id";
            return connection.QueryFirstOrDefault<InvoiceDTO>(sql, new { id });
        }

        public bool UpdatePaymentStatus(int id, string paymentMethod)
        {
            using var connection = new SqlConnection(_conn);
            var sql = @"UPDATE Invoices SET 
                            Status = N'Đã thanh toán', 
                            PaymentMethod = @paymentMethod,
                            PaymentDate = GETDATE()
                        WHERE InvoiceID = @id AND Status = N'Chưa thanh toán'";

            return connection.Execute(sql, new { id, paymentMethod }) > 0;
        }
    }
}
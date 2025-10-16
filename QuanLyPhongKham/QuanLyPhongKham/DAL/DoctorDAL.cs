// File: DAL/DoctorDAL.cs
using Dapper; // Thêm Dapper
using Microsoft.Data.SqlClient;
using QuanLyPhongKhamApi.Models;

namespace QuanLyPhongKhamApi.DAL
{
    public class DoctorDAL
    {
        private readonly string _conn;
        public DoctorDAL(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection")!;
        }

        public List<DoctorDTO> GetAll()
        {
            using var connection = new SqlConnection(_conn);
            var sql = @"SELECT d.*, s.SpecialtyName, a.Username
                        FROM Doctors d
                        LEFT JOIN Specialties s ON d.SpecialtyID = s.SpecialtyID
                        LEFT JOIN Accounts a ON d.AccountID = a.AccountID
                        WHERE d.IsDeleted = 0 ORDER BY d.FullName";
            return connection.Query<DoctorDTO>(sql).ToList();
        }

        public DoctorDTO? GetById(int id)
        {
            using var connection = new SqlConnection(_conn);
            var sql = @"SELECT d.*, s.SpecialtyName, a.Username
                        FROM Doctors d
                        LEFT JOIN Specialties s ON d.SpecialtyID = s.SpecialtyID
                        LEFT JOIN Accounts a ON d.AccountID = a.AccountID
                        WHERE d.DoctorID = @id AND d.IsDeleted = 0";
            return connection.QueryFirstOrDefault<DoctorDTO>(sql, new { id });
        }

        public DoctorDTO? GetByAccountId(int accountId)
        {
            using var connection = new SqlConnection(_conn);
            var sql = @"SELECT d.*, s.SpecialtyName, a.Username
                        FROM Doctors d
                        LEFT JOIN Specialties s ON d.SpecialtyID = s.SpecialtyID
                        LEFT JOIN Accounts a ON d.AccountID = a.AccountID
                        WHERE d.AccountID = @accountId AND d.IsDeleted = 0";
            return connection.QueryFirstOrDefault<DoctorDTO>(sql, new { accountId });
        }

        public int Create(DoctorCreateRequest req, int accountId)
        {
            using var connection = new SqlConnection(_conn);
            var sql = @"INSERT INTO Doctors (FullName, SpecialtyID, Phone, Email, AccountID)
                        OUTPUT INSERTED.DoctorID
                        VALUES (@FullName, @SpecialtyID, @Phone, @Email, @AccountID)";
            return connection.ExecuteScalar<int>(sql, new { req.FullName, req.SpecialtyID, req.Phone, req.Email, accountId });
        }

        public bool Update(int id, DoctorUpdateRequest req)
        {
            using var connection = new SqlConnection(_conn);
            var sql = @"UPDATE Doctors SET
                            FullName = @FullName, SpecialtyID = @SpecialtyID,
                            Phone = @Phone, Email = @Email
                        WHERE DoctorID = @id AND IsDeleted = 0";
            return connection.Execute(sql, new { id, req.FullName, req.SpecialtyID, req.Phone, req.Email }) > 0;
        }

        public bool SoftDelete(int id)
        {
            using var connection = new SqlConnection(_conn);
            var sql = "UPDATE Doctors SET IsDeleted = 1 WHERE DoctorID = @id";
            return connection.Execute(sql, new { id }) > 0;
        }
    }
}
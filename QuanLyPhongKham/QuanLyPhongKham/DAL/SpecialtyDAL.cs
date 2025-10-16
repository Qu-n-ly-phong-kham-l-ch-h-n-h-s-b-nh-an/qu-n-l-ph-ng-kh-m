// File: DAL/SpecialtyDAL.cs
using Dapper; // Thêm Dapper
using Microsoft.Data.SqlClient;
using QuanLyPhongKhamApi.Models;

namespace QuanLyPhongKhamApi.DAL
{
    public class SpecialtyDAL
    {
        private readonly string _conn;
        public SpecialtyDAL(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection")!;
        }

        public List<Specialty> GetAll()
        {
            using var connection = new SqlConnection(_conn);
            var sql = "SELECT SpecialtyID, SpecialtyName, IsDeleted FROM Specialties WHERE IsDeleted = 0 ORDER BY SpecialtyName";
            return connection.Query<Specialty>(sql).ToList();
        }

        public Specialty? GetById(int id)
        {
            using var connection = new SqlConnection(_conn);
            var sql = "SELECT SpecialtyID, SpecialtyName, IsDeleted FROM Specialties WHERE SpecialtyID = @id AND IsDeleted = 0";
            return connection.QueryFirstOrDefault<Specialty>(sql, new { id });
        }

        public int Create(SpecialtyRequest req)
        {
            using var connection = new SqlConnection(_conn);
            var sql = "INSERT INTO Specialties (SpecialtyName) OUTPUT INSERTED.SpecialtyID VALUES (@SpecialtyName)";
            return connection.ExecuteScalar<int>(sql, new { req.SpecialtyName });
        }

        public bool Update(int id, SpecialtyRequest req)
        {
            using var connection = new SqlConnection(_conn);
            var sql = "UPDATE Specialties SET SpecialtyName = @SpecialtyName WHERE SpecialtyID = @id AND IsDeleted = 0";
            return connection.Execute(sql, new { id, req.SpecialtyName }) > 0;
        }

        public bool SoftDelete(int id)
        {
            using var connection = new SqlConnection(_conn);
            var sql = "UPDATE Specialties SET IsDeleted = 1 WHERE SpecialtyID = @id";
            return connection.Execute(sql, new { id }) > 0;
        }

        public bool NameExists(string name, int excludeId = 0)
        {
            using var connection = new SqlConnection(_conn);
            var sql = "SELECT 1 FROM Specialties WHERE SpecialtyName = @name AND IsDeleted = 0 AND SpecialtyID != @excludeId";
            return connection.QueryFirstOrDefault<int>(sql, new { name, excludeId }) > 0;
        }
    }
}
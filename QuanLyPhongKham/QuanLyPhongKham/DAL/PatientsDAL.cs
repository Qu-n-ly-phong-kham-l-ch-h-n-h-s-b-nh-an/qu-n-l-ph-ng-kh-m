// File: DAL/PatientDAL.cs
using Dapper; // Thêm Dapper
using Microsoft.Data.SqlClient;
using QuanLyPhongKhamApi.Models;
using System.Data;

namespace QuanLyPhongKhamApi.DAL
{
    public class PatientDAL
    {
        private readonly string _conn;
        public PatientDAL(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection")!;
        }

        public List<Patient> GetAll()
        {
            using var connection = new SqlConnection(_conn);
            var sql = "SELECT * FROM Patients WHERE IsDeleted = 0 ORDER BY FullName";
            return connection.Query<Patient>(sql).ToList();
        }

        public Patient? GetById(int id)
        {
            using var connection = new SqlConnection(_conn);
            var sql = "SELECT * FROM Patients WHERE PatientID = @id AND IsDeleted = 0";
            return connection.QueryFirstOrDefault<Patient>(sql, new { id });
        }

        // Hàm này rất quan trọng cho việc kiểm tra quyền sở hữu
        public Patient? GetByAccountId(int accountId)
        {
            using var connection = new SqlConnection(_conn);
            var sql = "SELECT TOP 1 * FROM Patients WHERE AccountID = @accountId AND IsDeleted = 0";
            return connection.QueryFirstOrDefault<Patient>(sql, new { accountId });
        }

        public int Create(PatientCreateRequest req, int? accountId)
        {
            using var connection = new SqlConnection(_conn);
            var parameters = new DynamicParameters();
            parameters.Add("@FullName", req.FullName);
            parameters.Add("@DOB", req.DOB, DbType.DateTime);
            parameters.Add("@Gender", req.Gender, DbType.String);
            parameters.Add("@Phone", req.Phone, DbType.String);
            parameters.Add("@Email", req.Email, DbType.String);
            parameters.Add("@Address", req.Address, DbType.String);
            parameters.Add("@MedicalHistory", req.MedicalHistory, DbType.String);
            parameters.Add("@AccountID", accountId, DbType.Int32);
            parameters.Add("@NewPatientID", dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("sp_patient_create_with_account", parameters, commandType: CommandType.StoredProcedure);

            return parameters.Get<int>("@NewPatientID");
        }

        // HÀM UPDATE ĐÃ ĐƯỢC SỬA LẠI (✅ ĐÃ SỬA)
        // Nhận vào đối tượng Patient đầy đủ thay vì DTO
        public bool Update(Patient patient)
        {
            using var connection = new SqlConnection(_conn);
            var sql = @"UPDATE Patients SET
                            FullName = @FullName,
                            DOB = @DOB,
                            Gender = @Gender,
                            Phone = @Phone,
                            Email = @Email,
                            Address = @Address,
                            MedicalHistory = @MedicalHistory,
                            AccountID = @AccountID   -- Bổ sung AccountID
                        WHERE PatientID = @PatientID AND IsDeleted = 0";
            return connection.Execute(sql, patient) > 0;
        }

        public bool SoftDelete(int id)
        {
            using var connection = new SqlConnection(_conn);
            var sql = "UPDATE Patients SET IsDeleted = 1 WHERE PatientID = @id";
            return connection.Execute(sql, new { id }) > 0;
        }
    }
}
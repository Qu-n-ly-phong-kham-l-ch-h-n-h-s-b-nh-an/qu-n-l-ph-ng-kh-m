// File: DAL/EncounterDAL.cs
using Dapper; // Thêm Dapper
using Microsoft.Data.SqlClient;
using QuanLyPhongKhamApi.Models;
using System.Data;

namespace QuanLyPhongKhamApi.DAL
{
    public class EncounterDAL
    {
        private readonly string _conn;
        public EncounterDAL(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection")!;
        }

        public List<EncounterDTO> GetAllEncounters()
        {
            using var connection = new SqlConnection(_conn);
            var sql = @"SELECT e.EncounterID, e.AppointmentID, e.DoctorID, d.FullName AS DoctorName,
                               p.FullName AS PatientName, e.ExaminationNotes, e.EncounterDate, e.IsDeleted
                        FROM Encounters e
                        INNER JOIN Appointments a ON e.AppointmentID = a.AppointmentID
                        INNER JOIN Doctors d ON e.DoctorID = d.DoctorID
                        INNER JOIN Patients p ON a.PatientID = p.PatientID
                        WHERE e.IsDeleted = 0
                        ORDER BY e.EncounterDate DESC";
            return connection.Query<EncounterDTO>(sql).ToList();
        }

        public EncounterDTO? GetEncounterById(int id)
        {
            using var connection = new SqlConnection(_conn);
            var sql = @"SELECT e.EncounterID, e.AppointmentID, e.DoctorID, d.FullName AS DoctorName,
                               p.FullName AS PatientName, e.ExaminationNotes, e.EncounterDate, e.IsDeleted
                        FROM Encounters e
                        INNER JOIN Appointments a ON e.AppointmentID = a.AppointmentID
                        INNER JOIN Doctors d ON e.DoctorID = d.DoctorID
                        INNER JOIN Patients p ON a.PatientID = p.PatientID
                        WHERE e.EncounterID = @id AND e.IsDeleted = 0";
            return connection.QueryFirstOrDefault<EncounterDTO>(sql, new { id });
        }

        public int CompleteEncounter(int appointmentId, int doctorId, string examinationNotes,
                                     string diagnosisDescription, decimal serviceFee,
                                     List<PrescriptionItemDTO> prescriptionItems, int currentUserId)
        {
            using var connection = new SqlConnection(_conn);

            DataTable prescriptionTable = CreatePrescriptionTable(prescriptionItems);

            var parameters = new DynamicParameters();
            parameters.Add("@AppointmentID", appointmentId);
            parameters.Add("@DoctorID", doctorId);
            parameters.Add("@ExaminationNotes", examinationNotes);
            parameters.Add("@DiagnosisDescription", diagnosisDescription);
            parameters.Add("@ServiceFee", serviceFee);
            parameters.Add("@CurrentUserID", currentUserId);
            parameters.Add("@PrescriptionList", prescriptionTable.AsTableValuedParameter("tt_PrescriptionItems"));
            parameters.Add("@NewEncounterID", dbType: DbType.Int32, direction: ParameterDirection.Output);

            try
            {
                connection.Execute("sp_encounter_complete_v2", parameters, commandType: CommandType.StoredProcedure);
                return parameters.Get<int>("@NewEncounterID");
            }
            catch (SqlException ex)
            {
                // Bắt lỗi RAISERROR từ SP và ném lại dưới dạng lỗi nghiệp vụ
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public bool SoftDelete(int id)
        {
            using var connection = new SqlConnection(_conn);
            var sql = "UPDATE Encounters SET IsDeleted = 1 WHERE EncounterID = @id";
            return connection.Execute(sql, new { id }) > 0;
        }

        // Helper function này vẫn cần thiết để tạo TVP cho Dapper
        private static DataTable CreatePrescriptionTable(List<PrescriptionItemDTO> items)
        {
            var table = new DataTable();
            table.Columns.Add("DrugID", typeof(int));
            table.Columns.Add("Quantity", typeof(int));
            table.Columns.Add("Usage", typeof(string));

            if (items != null)
            {
                foreach (var item in items)
                {
                    table.Rows.Add(item.DrugID, item.Quantity, item.Usage ?? (object)DBNull.Value);
                }
            }
            return table;
        }
    }
}
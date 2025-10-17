// File: DAL/AppointmentDAL.cs
using Dapper; // Thêm Dapper
using Microsoft.Data.SqlClient;
using QuanLyPhongKhamApi.Models;

namespace QuanLyPhongKhamApi.DAL
{
    public class AppointmentDAL
    {
        private readonly string _conn;

        public AppointmentDAL(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection")!;
        }

        public List<AppointmentDTO> GetAll(int? doctorId, int? patientId, DateTime? date, string? status)
        {
            using var connection = new SqlConnection(_conn);
            var sql = @"SELECT a.AppointmentID, a.AppointmentDate, a.Status, a.Notes,
                               p.PatientID, p.FullName AS PatientName,
                               d.DoctorID, d.FullName AS DoctorName
                        FROM Appointments a
                        INNER JOIN Patients p ON a.PatientID = p.PatientID
                        INNER JOIN Doctors d ON a.DoctorID = d.DoctorID
                        WHERE a.IsDeleted = 0";

            var parameters = new DynamicParameters();
            if (doctorId.HasValue) { sql += " AND a.DoctorID = @doctorId"; parameters.Add("doctorId", doctorId.Value); }
            if (patientId.HasValue) { sql += " AND a.PatientID = @patientId"; parameters.Add("patientId", patientId.Value); }
            if (date.HasValue) { sql += " AND CAST(a.AppointmentDate AS DATE) = @date"; parameters.Add("date", date.Value.Date); }
            if (!string.IsNullOrEmpty(status)) { sql += " AND a.Status = @status"; parameters.Add("status", status); }
            sql += " ORDER BY a.AppointmentDate";

            return connection.Query<AppointmentDTO>(sql, parameters).ToList();
        }

        public AppointmentDTO? GetById(int id)
        {
            using var connection = new SqlConnection(_conn);
            var sql = @"SELECT a.AppointmentID, a.AppointmentDate, a.Status, a.Notes,
                               p.PatientID, p.FullName AS PatientName,
                               d.DoctorID, d.FullName AS DoctorName
                        FROM Appointments a
                        INNER JOIN Patients p ON a.PatientID = p.PatientID
                        INNER JOIN Doctors d ON a.DoctorID = d.DoctorID
                        WHERE a.AppointmentID = @id AND a.IsDeleted = 0";
            return connection.QueryFirstOrDefault<AppointmentDTO>(sql, new { id });
        }

        public int Create(AppointmentCreateRequest req, int createdByAccountId)
        {
            using var connection = new SqlConnection(_conn);
            var sql = @"INSERT INTO Appointments (PatientID, DoctorID, AppointmentDate, Notes, CreatedByAccountID)
                        OUTPUT INSERTED.AppointmentID
                        VALUES (@PatientID, @DoctorID, @AppointmentDate, @Notes, @CreatedByAccountID);";
            return connection.ExecuteScalar<int>(sql, new { req.PatientID, req.DoctorID, req.AppointmentDate, req.Notes, createdByAccountId });
        }

        public bool Update(int id, AppointmentUpdateRequest req)
        {
            using var connection = new SqlConnection(_conn);
            var sql = @"UPDATE Appointments SET DoctorID = @DoctorID, AppointmentDate = @AppointmentDate, Notes = @Notes
                        WHERE AppointmentID = @id AND IsDeleted = 0";
            return connection.Execute(sql, new { id, req.DoctorID, req.AppointmentDate, req.Notes }) > 0;
        }

        public bool UpdateStatus(int id, string status)
        {
            using var connection = new SqlConnection(_conn);
            var sql = "UPDATE Appointments SET Status = @status WHERE AppointmentID = @id";
            return connection.Execute(sql, new { id, status }) > 0;
        }

        public bool CheckScheduleConflict(int doctorId, DateTime appointmentDate, int excludeAppointmentId = 0)
        {
            using var connection = new SqlConnection(_conn);
            var sql = @"SELECT 1 FROM Appointments
                        WHERE DoctorID = @DoctorID AND Status NOT IN (N'Đã hủy', N'Đã khám') AND IsDeleted = 0
                        AND AppointmentID != @excludeAppointmentId
                        AND AppointmentDate BETWEEN DATEADD(minute, -29, @AppointmentDate) AND DATEADD(minute, 29, @AppointmentDate)";
            return connection.QueryFirstOrDefault<int>(sql, new { DoctorID = doctorId, AppointmentDate = appointmentDate, excludeAppointmentId }) > 0;
        }

        public bool SoftDelete(int id)
        {
            using var connection = new SqlConnection(_conn);
            var sql = "UPDATE Appointments SET IsDeleted = 1 WHERE AppointmentID = @id";
            return connection.Execute(sql, new { id }) > 0;
        }

        public List<AppointmentDTO> GetUpcomingAppointmentsForReminder()
        {
            using var connection = new SqlConnection(_conn);
            // Lấy các lịch hẹn trong vòng 24 giờ tới mà chưa được gửi nhắc nhở
            var sql = @"SELECT a.AppointmentID, a.AppointmentDate, a.Status, a.Notes,
                       p.PatientID, p.FullName AS PatientName,
                       d.DoctorID, d.FullName AS DoctorName
                FROM Appointments a
                INNER JOIN Patients p ON a.PatientID = p.PatientID
                INNER JOIN Doctors d ON a.DoctorID = d.DoctorID
                WHERE a.IsDeleted = 0 AND a.Status = N'Đã đặt' AND a.IsReminderSent = 0
                  AND a.AppointmentDate BETWEEN GETDATE() AND DATEADD(hour, 24, GETDATE())";
            return connection.Query<AppointmentDTO>(sql).ToList();
        }

        public bool MarkReminderAsSent(int appointmentId)
        {
            using var connection = new SqlConnection(_conn);
            var sql = "UPDATE Appointments SET IsReminderSent = 1 WHERE AppointmentID = @appointmentId";
            return connection.Execute(sql, new { appointmentId }) > 0;
        }
    }
}
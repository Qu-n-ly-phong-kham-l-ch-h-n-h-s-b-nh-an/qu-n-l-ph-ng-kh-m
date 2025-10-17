using Dapper;
using Microsoft.Data.SqlClient;
using QuanLyPhongKhamApi.Models;
using System.Data;

namespace QuanLyPhongKhamApi.DAL
{
    public class AccountDAL
    {
        private readonly string _conn;
        public AccountDAL(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection")!;
        }

        public List<Account> GetAll()
        {
            using var connection = new SqlConnection(_conn);
            var sql = "SELECT AccountID, Username, Role, IsActive, CreatedAt FROM Accounts";
            return connection.Query<Account>(sql).ToList();
        }

        public Account? GetByUsername(string username)
        {
            using var connection = new SqlConnection(_conn);
            var sql = "SELECT TOP 1 AccountID, Username, PasswordHash, Role, IsActive, CreatedAt FROM Accounts WHERE Username = @username";
            return connection.QueryFirstOrDefault<Account>(sql, new { username });
        }

        public Account? GetById(int id)
        {
            using var connection = new SqlConnection(_conn);
            var sql = "SELECT AccountID, Username, Role, IsActive, CreatedAt FROM Accounts WHERE AccountID = @id";
            return connection.QueryFirstOrDefault<Account>(sql, new { id });
        }

        public int Register(string username, string passwordHash, string role)
        {
            using var connection = new SqlConnection(_conn);
            var parameters = new DynamicParameters();
            parameters.Add("@Username", username);
            parameters.Add("@PasswordHash", passwordHash);
            parameters.Add("@Role", role);
            parameters.Add("@NewAccountID", dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("sp_account_register", parameters, commandType: CommandType.StoredProcedure);

            return parameters.Get<int>("@NewAccountID");
        }

        public bool UpdateInfo(Account acc)
        {
            using var connection = new SqlConnection(_conn);
            var sql = @"UPDATE Accounts SET Username = @Username, Role = @Role, IsActive = @IsActive
                        WHERE AccountID = @AccountID";
            return connection.Execute(sql, acc) > 0;
        }

        public bool UpdatePasswordHash(int id, string passwordHash)
        {
            using var connection = new SqlConnection(_conn);
            var sql = "UPDATE Accounts SET PasswordHash = @passwordHash WHERE AccountID = @id";
            return connection.Execute(sql, new { id, passwordHash }) > 0;
        }

        public bool Delete(int id)
        {
            using var connection = new SqlConnection(_conn);
            var sql = "UPDATE Accounts SET IsActive = 0 WHERE AccountID = @id";
            return connection.Execute(sql, new { id }) > 0;
        }

        public bool SetActive(int id, bool isActive)
        {
            using var connection = new SqlConnection(_conn);
            var sql = "UPDATE Accounts SET IsActive = @isActive WHERE AccountID = @id";
            return connection.Execute(sql, new { id, isActive }) > 0;
        }
    }
}
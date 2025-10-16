// File: DAL/DrugDAL.cs
using Dapper; // Thêm Dapper
using Microsoft.Data.SqlClient;
using QuanLyPhongKhamApi.Models;

namespace QuanLyPhongKhamApi.DAL
{
    public class DrugDAL
    {
        private readonly string _conn;
        public DrugDAL(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection")!;
        }

        #region Drug CRUD Methods
        public List<Drug> GetAll()
        {
            using var connection = new SqlConnection(_conn);
            var sql = "SELECT * FROM Drugs WHERE IsDeleted = 0 ORDER BY DrugName";
            return connection.Query<Drug>(sql).ToList();
        }

        public Drug? GetById(int id)
        {
            using var connection = new SqlConnection(_conn);
            var sql = "SELECT * FROM Drugs WHERE DrugID = @id AND IsDeleted = 0";
            return connection.QueryFirstOrDefault<Drug>(sql, new { id });
        }

        public int Create(DrugRequest req, int createdBy)
        {
            using var connection = new SqlConnection(_conn);
            var sql = @"INSERT INTO Drugs (DrugName, Unit, Price, CreatedByAccountID) 
                        OUTPUT INSERTED.DrugID 
                        VALUES (@DrugName, @Unit, @Price, @CreatedBy)";
            return connection.ExecuteScalar<int>(sql, new { req.DrugName, req.Unit, req.Price, CreatedBy = createdBy });
        }

        public bool Update(int id, DrugRequest req)
        {
            using var connection = new SqlConnection(_conn);
            var sql = @"UPDATE Drugs SET DrugName = @DrugName, Unit = @Unit, Price = @Price 
                        WHERE DrugID = @id AND IsDeleted = 0";
            return connection.Execute(sql, new { id, req.DrugName, req.Unit, req.Price }) > 0;
        }

        public bool SoftDelete(int id)
        {
            using var connection = new SqlConnection(_conn);
            var sql = "UPDATE Drugs SET IsDeleted = 1 WHERE DrugID = @id";
            return connection.Execute(sql, new { id }) > 0;
        }

        public bool NameExists(string name, int excludeId = 0)
        {
            using var connection = new SqlConnection(_conn);
            var sql = "SELECT 1 FROM Drugs WHERE DrugName = @name AND IsDeleted = 0 AND DrugID != @excludeId";
            return connection.QueryFirstOrDefault<int>(sql, new { name, excludeId }) > 0;
        }
        #endregion

        #region Stock Management Methods
        public List<DrugStockDTO> GetStockReport()
        {
            using var connection = new SqlConnection(_conn);
            var sql = @"SELECT d.DrugID, d.DrugName, d.Unit, d.Price, ISNULL(ds.QuantityAvailable, 0) AS QuantityAvailable, ds.LastUpdated
                        FROM Drugs d
                        LEFT JOIN Drug_Stocks ds ON d.DrugID = ds.DrugID
                        WHERE d.IsDeleted = 0 ORDER BY d.DrugName";
            return connection.Query<DrugStockDTO>(sql).ToList();
        }

        public bool AdjustStock(int drugId, int quantityChange)
        {
            using var connection = new SqlConnection(_conn);
            var sql = @"MERGE Drug_Stocks AS target
                        USING (SELECT @DrugID AS DrugID) AS source
                        ON (target.DrugID = source.DrugID)
                        WHEN MATCHED THEN
                            UPDATE SET QuantityAvailable = IIF(target.QuantityAvailable + @QuantityChange < 0, 0, target.QuantityAvailable + @QuantityChange), LastUpdated = GETDATE()
                        WHEN NOT MATCHED AND @QuantityChange >= 0 THEN
                            INSERT (DrugID, QuantityAvailable) VALUES (@DrugID, @QuantityChange);";
            return connection.Execute(sql, new { DrugID = drugId, QuantityChange = quantityChange }) > 0;
        }
        #endregion
    }
}
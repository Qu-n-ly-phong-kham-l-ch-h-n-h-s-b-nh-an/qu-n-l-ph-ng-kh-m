using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhongKham.DAL.Entities;

namespace PhongKham.BLL.Service
{
    public class AuditLogService
    {
        private readonly PhongKhamDbContext _context;

        public AuditLogService(PhongKhamDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả AuditLogs
        public IEnumerable<AuditLog> GetAll()
        {
            return _context.AuditLogs
                           .OrderByDescending(a => a.AccessTime) // mới nhất lên đầu
                           .ToList();
        }

        // Lấy AuditLog theo ID
        public AuditLog? GetById(int id)
        {
            return _context.AuditLogs.FirstOrDefault(a => a.LogId == id);
        }

        // Tạo mới AuditLog
        public void Create(AuditLog log)
        {
            log.AccessTime = DateTime.Now; // tự set thời gian log
            _context.AuditLogs.Add(log);
            _context.SaveChanges();
        }

        // Cập nhật AuditLog
        public void Update(AuditLog log)
        {
            _context.AuditLogs.Update(log);
            _context.SaveChanges();
        }

        // Xóa AuditLog
        public void Delete(int id)
        {
            var log = _context.AuditLogs.FirstOrDefault(a => a.LogId == id);
            if (log != null)
            {
                _context.AuditLogs.Remove(log);
                _context.SaveChanges();
            }
        }
    }
}
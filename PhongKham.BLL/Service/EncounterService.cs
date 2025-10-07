using System.Collections.Generic;
using System.Linq;
using PhongKham.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace PhongKham.BLL.Service
{
    public class EncounterService
    {
        private readonly PhongKhamDbContext _context;

        public EncounterService(PhongKhamDbContext context)
        {
            _context = context;
        }

        // ✅ Lấy tất cả Encounter có include dữ liệu liên quan
        public IEnumerable<Encounter> GetAll()
        {
            return _context.Encounters
                .Include(e => e.Doctor)
                .Include(e => e.Appointment)
                    .ThenInclude(a => a.Patient)
                .ToList();
        }

        // ✅ Lấy theo ID
        public Encounter? GetById(int id)
        {
            return _context.Encounters
                .Include(e => e.Doctor)
                .Include(e => e.Appointment)
                    .ThenInclude(a => a.Patient)
                .FirstOrDefault(e => e.EncounterId == id);
        }

        // ✅ Tạo mới Encounter
        public void Create(Encounter encounter)
        {
            _context.Encounters.Add(encounter);
            _context.SaveChanges();
        }

        // ✅ Cập nhật Encounter
        public void Update(Encounter encounter)
        {
            _context.Encounters.Update(encounter);
            _context.SaveChanges();
        }

        // ✅ Xóa Encounter
        public void Delete(int id)
        {
            var encounter = _context.Encounters.FirstOrDefault(e => e.EncounterId == id);
            if (encounter != null)
            {
                _context.Encounters.Remove(encounter);
                _context.SaveChanges();
            }
        }
    }
}
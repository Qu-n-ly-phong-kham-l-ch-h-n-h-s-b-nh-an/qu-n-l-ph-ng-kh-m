using Microsoft.EntityFrameworkCore;
using PhongKham.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhongKham.BLL.Service
{
    public class EncounterService
    {
        private readonly PhongKhamDbContext _context;

        public EncounterService(PhongKhamDbContext context)
        {
            _context = context;
        }

        // ✅ Lấy tất cả Encounter (có lọc, sắp xếp, phân trang)
        public IEnumerable<Encounter> Search(
            string? keyword,
            string? doctorName,
            string? patientName,
            DateTime? fromDate,
            DateTime? toDate,
            string? sortBy,
            bool ascending = true,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Encounters
                .Include(e => e.Doctor)
                .Include(e => e.Appointment).ThenInclude(a => a.Patient)
                .Include(e => e.Prescriptions).ThenInclude(p => p.Drug)
                .AsQueryable();

            // 🔍 Bộ lọc
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(e =>
                    e.Notes.Contains(keyword) ||
                    e.Appointment.Patient.FullName.Contains(keyword) ||
                    e.Doctor.FullName.Contains(keyword));

            if (!string.IsNullOrEmpty(doctorName))
                query = query.Where(e => e.Doctor.FullName.Contains(doctorName));

            if (!string.IsNullOrEmpty(patientName))
                query = query.Where(e => e.Appointment.Patient.FullName.Contains(patientName));

            if (fromDate.HasValue)
                query = query.Where(e => e.Appointment.AppointmentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(e => e.Appointment.AppointmentDate <= toDate.Value);

            // 🔽 Sắp xếp
            query = sortBy?.ToLower() switch
            {
                "doctor" => ascending ? query.OrderBy(e => e.Doctor.FullName) : query.OrderByDescending(e => e.Doctor.FullName),
                "patient" => ascending ? query.OrderBy(e => e.Appointment.Patient.FullName) : query.OrderByDescending(e => e.Appointment.Patient.FullName),
                "date" => ascending ? query.OrderBy(e => e.Appointment.AppointmentDate) : query.OrderByDescending(e => e.Appointment.AppointmentDate),
                _ => query.OrderByDescending(e => e.EncounterId)
            };

            // 📄 Phân trang
            return query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        // =================== Các hàm cũ giữ nguyên ===================
        public IEnumerable<Encounter> GetAll()
        {
            return _context.Encounters
                .Include(e => e.Doctor)
                .Include(e => e.Appointment).ThenInclude(a => a.Patient)
                .Include(e => e.Prescriptions).ThenInclude(p => p.Drug)
                .Include(e => e.Diagnoses)
                .ToList();
        }

        public Encounter? GetById(int id)
        {
            return _context.Encounters
                .Include(e => e.Doctor)
                .Include(e => e.Appointment).ThenInclude(a => a.Patient)
                .Include(e => e.Prescriptions).ThenInclude(p => p.Drug)
                .Include(e => e.Diagnoses)
                .FirstOrDefault(e => e.EncounterId == id);
        }

        public void Create(Encounter encounter)
        {
            var appointment = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefault(a => a.AppointmentId == encounter.AppointmentId)
                ?? throw new Exception("Không tìm thấy lịch hẹn tương ứng.");

            encounter.DoctorId = appointment.DoctorId;
            _context.Encounters.Add(encounter);
            appointment.Status = "Đang khám";
            _context.SaveChanges();
        }

        public void AddDiagnosis(int encounterId, string description, byte[]? resultFile = null)
        {
            var encounter = _context.Encounters.Find(encounterId)
                ?? throw new Exception("Không tìm thấy lần khám.");

            _context.Diagnoses.Add(new Diagnosis
            {
                EncounterId = encounterId,
                Description = description,
                ResultFile = resultFile
            });
            _context.SaveChanges();
        }

        public void AddPrescription(int encounterId, int drugId, int quantity, string usage)
        {
            var drug = _context.Drugs.FirstOrDefault(d => d.DrugId == drugId)
                ?? throw new Exception("Không tìm thấy thuốc.");

            _context.Prescriptions.Add(new Prescription
            {
                EncounterId = encounterId,
                DrugId = drugId,
                Quantity = quantity,
                Usage = usage
            });

            var stock = _context.DrugStocks.FirstOrDefault(s => s.DrugId == drugId);
            if (stock != null && stock.QuantityAvailable >= quantity)
            {
                stock.QuantityAvailable -= quantity;
                stock.LastUpdated = DateTime.Now;
            }

            _context.SaveChanges();
        }

        public void CompleteEncounter(int encounterId)
        {
            var encounter = _context.Encounters
                .Include(e => e.Appointment)
                .Include(e => e.Prescriptions).ThenInclude(p => p.Drug)
                .FirstOrDefault(e => e.EncounterId == encounterId)
                ?? throw new Exception("Không tìm thấy lần khám.");

            decimal total = encounter.Prescriptions.Sum(p =>
                (p.Drug?.Price ?? 0) * (p.Quantity ?? 0));

            _context.Invoices.Add(new Invoice
            {
                EncounterId = encounter.EncounterId,
                PatientId = encounter.Appointment.PatientId,
                TotalAmount = total,
                PaymentMethod = "Tiền mặt",
                Status = "Chưa thanh toán"
            });

            encounter.Appointment.Status = "Đã hoàn tất";
            _context.SaveChanges();
        }
    }
}

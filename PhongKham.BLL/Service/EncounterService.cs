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

        // ✅ Lấy tất cả Encounter có đủ thông tin liên quan
        public IEnumerable<Encounter> GetAll()
        {
            return _context.Encounters
                .Include(e => e.Doctor)
                .Include(e => e.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(e => e.Prescriptions)
                    .ThenInclude(p => p.Drug)
                .Include(e => e.Diagnoses)
                .ToList();
        }

        // ✅ Lấy 1 Encounter theo ID
        public Encounter? GetById(int id)
        {
            return _context.Encounters
                .Include(e => e.Doctor)
                .Include(e => e.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(e => e.Prescriptions)
                    .ThenInclude(p => p.Drug)
                .Include(e => e.Diagnoses)
                .FirstOrDefault(e => e.EncounterId == id);
        }

        // ✅ Tạo lần khám mới (dựa theo lịch hẹn)
        public void Create(Encounter encounter)
        {
            var appointment = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefault(a => a.AppointmentId == encounter.AppointmentId);

            if (appointment == null)
                throw new Exception("Không tìm thấy lịch hẹn tương ứng.");

            // Gán bác sĩ từ lịch hẹn (tránh sửa sai ID)
            encounter.DoctorId = appointment.DoctorId;

            _context.Encounters.Add(encounter);
            appointment.Status = "Đang khám"; // cập nhật trạng thái lịch
            _context.SaveChanges();
        }

        // ✅ Thêm chẩn đoán vào lần khám
        public void AddDiagnosis(int encounterId, string description, byte[]? resultFile = null)
        {
            var encounter = _context.Encounters.Find(encounterId)
                ?? throw new Exception("Không tìm thấy lần khám.");

            var diag = new Diagnosis
            {
                EncounterId = encounterId,
                Description = description,
                ResultFile = resultFile
            };

            _context.Diagnoses.Add(diag);
            _context.SaveChanges();
        }

        // ✅ Kê thuốc vào đơn
        public void AddPrescription(int encounterId, int drugId, int quantity, string usage)
        {
            var drug = _context.Drugs.FirstOrDefault(d => d.DrugId == drugId)
                ?? throw new Exception("Không tìm thấy thuốc.");
            var prescription = new Prescription
            {
                EncounterId = encounterId,
                DrugId = drugId,
                Quantity = quantity,
                Usage = usage
            };

            _context.Prescriptions.Add(prescription);

            // Giảm tồn kho nếu có dữ liệu
            var stock = _context.DrugStocks.FirstOrDefault(s => s.DrugId == drugId);
            if (stock != null && stock.QuantityAvailable >= quantity)
            {
                stock.QuantityAvailable -= quantity;
                stock.LastUpdated = DateTime.Now;
            }

            _context.SaveChanges();
        }

        // ✅ Hoàn tất khám (tự tạo hóa đơn)
        public void CompleteEncounter(int encounterId)
        {
            var encounter = _context.Encounters
                .Include(e => e.Appointment)
                .Include(e => e.Prescriptions)
                    .ThenInclude(p => p.Drug)
                .FirstOrDefault(e => e.EncounterId == encounterId)
                ?? throw new Exception("Không tìm thấy lần khám.");

            decimal total = encounter.Prescriptions.Sum(p =>
                (p.Drug?.Price ?? 0) * (p.Quantity ?? 0));

            var invoice = new Invoice
            {
                EncounterId = encounter.EncounterId,
                PatientId = encounter.Appointment.PatientId,
                TotalAmount = total,
                PaymentMethod = "Tiền mặt",
                Status = "Chưa thanh toán"
            };

            _context.Invoices.Add(invoice);
            encounter.Appointment.Status = "Đã hoàn tất";
            _context.SaveChanges();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using PhongKham.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhongKham.BLL.Service
{
    public class InvoiceService
    {
        private readonly PhongKhamDbContext _context;

        public InvoiceService(PhongKhamDbContext context)
        {
            _context = context;
        }

        // ✅ Lấy tất cả hóa đơn (kèm thông tin bệnh nhân và lần khám)
        public IEnumerable<Invoice> GetAll()
        {
            return _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Encounter)
                    .ThenInclude(e => e.Doctor)
                .AsNoTracking()
                .ToList();
        }

        // ✅ Lấy theo ID
        public Invoice? GetById(int id)
        {
            return _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Encounter)
                    .ThenInclude(e => e.Doctor)
                .FirstOrDefault(i => i.InvoiceId == id);
        }

        // ✅ Tạo mới hóa đơn (tự tính tiền thuốc + phí khám)
        public void Create(Invoice inv)
        {
            const decimal consultationFee = 200000m; // phí khám
            decimal total = consultationFee;

            var prescriptions = _context.Prescriptions
                .Include(p => p.Drug)
                .Where(p => p.EncounterId == inv.EncounterId)
                .ToList();

            foreach (var item in prescriptions)
            {
                total += (item.Drug.Price ?? 0) * (item.Quantity ?? 0);
            }

            inv.TotalAmount = total;
            inv.Status ??= "Chưa thanh toán";
            inv.PaymentDate ??= DateTime.Now;

            _context.Invoices.Add(inv);
            _context.SaveChanges();
        }

        // ✅ Cập nhật hóa đơn
        public void Update(Invoice invoice)
        {
            _context.Invoices.Update(invoice);
            _context.SaveChanges();
        }

        // ✅ Xóa hóa đơn
        public void Delete(int id)
        {
            var inv = _context.Invoices.FirstOrDefault(i => i.InvoiceId == id);
            if (inv != null)
            {
                _context.Invoices.Remove(inv);
                _context.SaveChanges();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhongKham.DAL.Entities;

namespace PhongKham.BLL.Service
{
    public class InvoiceService
    {
        private readonly PhongKhamDbContext _context;

        public InvoiceService(PhongKhamDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả hóa đơn
        public IEnumerable<Invoice> GetAll()
        {
            return _context.Invoices.ToList();
        }

        // Lấy 1 hóa đơn theo ID
        public Invoice? GetById(int id)
        {
            return _context.Invoices.FirstOrDefault(i => i.InvoiceId == id);
        }

        // Tạo mới hóa đơn
        public void Create(Invoice invoice)
        {
            // Thiết lập mặc định nếu cần
            if (invoice.PaymentDate == null)
                invoice.PaymentDate = DateTime.Now;

            if (string.IsNullOrEmpty(invoice.Status))
                invoice.Status = "Chưa thanh toán";

            _context.Invoices.Add(invoice);
            _context.SaveChanges();
        }

        // Cập nhật hóa đơn
        public void Update(Invoice invoice)
        {
            _context.Invoices.Update(invoice);
            _context.SaveChanges();
        }

        // Xóa hóa đơn
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
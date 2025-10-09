using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PhongKham.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

        // ✅ Lấy tất cả hóa đơn (kèm thông tin bệnh nhân và bác sĩ)
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
            const decimal consultationFee = 200000m; // Phí khám
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

        // ✅ Xuất Excel danh sách hóa đơn
        public byte[] ExportInvoicesToExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("DanhSachHoaDon");

                // ====== Tiêu đề ======
                ws.Cells["A1"].Value = "DANH SÁCH HÓA ĐƠN PHÒNG KHÁM";
                ws.Cells["A1:F1"].Merge = true;
                ws.Cells["A1"].Style.Font.Bold = true;
                ws.Cells["A1"].Style.Font.Size = 16;
                ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // ====== Header ======
                string[] headers = { "Mã HĐ", "Bệnh nhân", "Bác sĩ", "Tổng tiền", "Ngày thanh toán", "Trạng thái" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cells[3, i + 1].Value = headers[i];
                    ws.Cells[3, i + 1].Style.Font.Bold = true;
                    ws.Cells[3, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[3, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    ws.Cells[3, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // ====== Dữ liệu ======
                var invoices = _context.Invoices
                    .Include(i => i.Patient)
                    .Include(i => i.Encounter)
                        .ThenInclude(e => e.Doctor)
                    .ToList();

                int row = 4;
                foreach (var inv in invoices)
                {
                    ws.Cells[row, 1].Value = inv.InvoiceId;
                    ws.Cells[row, 2].Value = inv.Patient?.FullName;
                    ws.Cells[row, 3].Value = inv.Encounter?.Doctor?.FullName;
                    ws.Cells[row, 4].Value = inv.TotalAmount ?? 0;
                    ws.Cells[row, 4].Style.Numberformat.Format = "#,##0 đ";
                    ws.Cells[row, 5].Value = inv.PaymentDate?.ToString("dd/MM/yyyy");
                    ws.Cells[row, 6].Value = inv.Status;

                    for (int col = 1; col <= 6; col++)
                        ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    row++;
                }

                ws.Cells.AutoFitColumns();
                return package.GetAsByteArray();
            }
        }
    }
}

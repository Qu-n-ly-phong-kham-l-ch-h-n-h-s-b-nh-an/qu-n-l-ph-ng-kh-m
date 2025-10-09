using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using PhongKham.DAL.Entities;

namespace PhongKham.BLL.Service
{
    public class ReportService
    {
        private readonly PhongKhamDbContext _context;

        public ReportService(PhongKhamDbContext context)
        {
            _context = context;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // ⚡ Thiết lập license EPPlus
        }

        // 📊 Thống kê doanh thu theo bác sĩ
        public IEnumerable<object> GetRevenueByDoctor()
        {
            var data = _context.Invoices
                .Where(i => i.PaymentDate != null)
                .Select(i => new
                {
                    DoctorId = i.Encounter == null ? 0 : i.Encounter.DoctorId,
                    DoctorName = i.Encounter == null || i.Encounter.Doctor == null
                        ? "Không rõ bác sĩ"
                        : i.Encounter.Doctor.FullName,
                    TotalAmount = i.TotalAmount ?? 0
                })
                .ToList();

            return data
                .GroupBy(x => new { x.DoctorId, x.DoctorName })
                .Select(g => new
                {
                    g.Key.DoctorId,
                    g.Key.DoctorName,
                    TotalRevenue = g.Sum(x => x.TotalAmount),
                    TotalInvoices = g.Count()
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToList();
        }

        // 📈 Thống kê doanh thu theo chuyên khoa
        public IEnumerable<object> GetRevenueBySpecialty()
        {
            var data = _context.Invoices
                .Where(i => i.PaymentDate != null)
                .Select(i => new
                {
                    SpecialtyId = i.Encounter == null || i.Encounter.Doctor == null
                        ? 0
                        : i.Encounter.Doctor.SpecialtyId,
                    SpecialtyName = i.Encounter == null || i.Encounter.Doctor == null || i.Encounter.Doctor.Specialty == null
                        ? "Không rõ chuyên khoa"
                        : i.Encounter.Doctor.Specialty.SpecialtyName,
                    TotalAmount = i.TotalAmount ?? 0
                })
                .ToList();

            return data
                .GroupBy(x => new { x.SpecialtyId, x.SpecialtyName })
                .Select(g => new
                {
                    g.Key.SpecialtyId,
                    g.Key.SpecialtyName,
                    TotalRevenue = g.Sum(x => x.TotalAmount),
                    TotalInvoices = g.Count()
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToList();
        }

        // 📦 Xuất Excel thống kê theo bác sĩ
        public byte[] ExportRevenueByDoctorToExcel()
        {
            var list = GetRevenueByDoctor().ToList();

            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("Doanh thu bác sĩ");
                sheet.Cells["A1"].Value = "Mã bác sĩ";
                sheet.Cells["B1"].Value = "Tên bác sĩ";
                sheet.Cells["C1"].Value = "Số hóa đơn";
                sheet.Cells["D1"].Value = "Tổng doanh thu";

                int row = 2;
                foreach (dynamic item in list)
                {
                    sheet.Cells[row, 1].Value = item.DoctorId;
                    sheet.Cells[row, 2].Value = item.DoctorName;
                    sheet.Cells[row, 3].Value = item.TotalInvoices;
                    sheet.Cells[row, 4].Value = item.TotalRevenue;
                    row++;
                }

                sheet.Cells.AutoFitColumns();
                return package.GetAsByteArray();
            }
        }

        // 📦 Xuất Excel thống kê theo chuyên khoa
        public byte[] ExportRevenueBySpecialtyToExcel()
        {
            var list = GetRevenueBySpecialty().ToList();

            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("Doanh thu chuyên khoa");
                sheet.Cells["A1"].Value = "Mã chuyên khoa";
                sheet.Cells["B1"].Value = "Tên chuyên khoa";
                sheet.Cells["C1"].Value = "Số hóa đơn";
                sheet.Cells["D1"].Value = "Tổng doanh thu";

                int row = 2;
                foreach (dynamic item in list)
                {
                    sheet.Cells[row, 1].Value = item.SpecialtyId;
                    sheet.Cells[row, 2].Value = item.SpecialtyName;
                    sheet.Cells[row, 3].Value = item.TotalInvoices;
                    sheet.Cells[row, 4].Value = item.TotalRevenue;
                    row++;
                }

                sheet.Cells.AutoFitColumns();
                return package.GetAsByteArray();
            }
        }
    }
}

using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PhongKham.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PhongKham.BLL.Service
{
    public class DoctorService
    {
        private readonly PhongKhamDbContext _context;

        public DoctorService(PhongKhamDbContext context)
        {
            _context = context;
        }

        // ================= CRUD cơ bản =================
        public IEnumerable<Doctor> GetAll()
        {
            return _context.Doctors
                .Include(d => d.Specialty)
                .OrderBy(d => d.FullName)
                .ToList();
        }

        public Doctor? GetById(int id)
        {
            return _context.Doctors
                .Include(d => d.Specialty)
                .FirstOrDefault(d => d.DoctorId == id);
        }

        public void Create(Doctor doctor)
        {
            if (!string.IsNullOrEmpty(doctor.Email) && _context.Doctors.Any(d => d.Email == doctor.Email))
                throw new Exception("Email đã được sử dụng cho bác sĩ khác.");

            if (!string.IsNullOrEmpty(doctor.Phone) && _context.Doctors.Any(d => d.Phone == doctor.Phone))
                throw new Exception("Số điện thoại đã được sử dụng cho bác sĩ khác.");

            _context.Doctors.Add(doctor);
            _context.SaveChanges();
            Console.WriteLine($"✅ Thêm bác sĩ: {doctor.FullName}");
        }

        public void Update(Doctor doctor)
        {
            _context.Doctors.Update(doctor);
            _context.SaveChanges();
            Console.WriteLine($"✏️ Cập nhật thông tin bác sĩ: {doctor.FullName}");
        }

        public void Delete(int id)
        {
            var doc = _context.Doctors.FirstOrDefault(d => d.DoctorId == id);
            if (doc == null)
                throw new Exception("Không tìm thấy bác sĩ để xóa.");

            _context.Doctors.Remove(doc);
            _context.SaveChanges();
            Console.WriteLine($"🗑️ Đã xóa bác sĩ: {doc.FullName}");
        }

        // ================= Filter / Paging Models =================
        public class DoctorFilterRequest
        {
            public string? Keyword { get; set; }           // tìm theo tên hoặc chuyên khoa
            public int? SpecialtyId { get; set; }         // lọc theo chuyên khoa
            public string? SortBy { get; set; } = "DoctorId";  // FullName | SpecialtyName | Phone | Email | DoctorId
            public bool IsDescending { get; set; } = false;
            public int PageNumber { get; set; } = 1;
            public int PageSize { get; set; } = 10;
        }

        public class DoctorPagedResult
        {
            public int TotalRecords { get; set; }
            public int TotalPages { get; set; }
            public List<Doctor> Data { get; set; } = new();
        }

        // ================= Tìm kiếm - Lọc - Sắp xếp - Phân trang =================
        public DoctorPagedResult GetFilteredDoctors(DoctorFilterRequest request)
        {
            var query = _context.Doctors
                .Include(d => d.Specialty)
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                var k = request.Keyword.Trim().ToLower();
                query = query.Where(d =>
                    (d.FullName != null && d.FullName.ToLower().Contains(k)) ||
                    (d.Specialty != null && d.Specialty.SpecialtyName != null && d.Specialty.SpecialtyName.ToLower().Contains(k))
                );
            }

            // Lọc chuyên khoa
            if (request.SpecialtyId.HasValue)
                query = query.Where(d => d.SpecialtyId == request.SpecialtyId.Value);

            // Sắp xếp có lựa chọn
            switch (request.SortBy?.ToLower())
            {
                case "fullname":
                    query = request.IsDescending ? query.OrderByDescending(d => d.FullName) : query.OrderBy(d => d.FullName);
                    break;
                case "specialtyname":
                    query = request.IsDescending ? query.OrderByDescending(d => d.Specialty!.SpecialtyName) : query.OrderBy(d => d.Specialty!.SpecialtyName);
                    break;
                case "phone":
                    query = request.IsDescending ? query.OrderByDescending(d => d.Phone) : query.OrderBy(d => d.Phone);
                    break;
                case "email":
                    query = request.IsDescending ? query.OrderByDescending(d => d.Email) : query.OrderBy(d => d.Email);
                    break;
                default:
                    query = request.IsDescending ? query.OrderByDescending(d => d.DoctorId) : query.OrderBy(d => d.DoctorId);
                    break;
            }

            // Phân trang
            var totalRecords = query.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);
            var data = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new DoctorPagedResult
            {
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                Data = data
            };
        }

        // ================= Xuất Excel (có thể truyền filter) =================
        public byte[] ExportDoctorsToExcel(DoctorFilterRequest? request = null)
        {
            // Thiết lập license cho EPPlus (phiên bản <=7.x)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Build query với filter nếu có
            var query = _context.Doctors.Include(d => d.Specialty).AsQueryable();
            if (request != null)
            {
                if (!string.IsNullOrEmpty(request.Keyword))
                {
                    var k = request.Keyword.Trim().ToLower();
                    query = query.Where(d =>
                        (d.FullName != null && d.FullName.ToLower().Contains(k)) ||
                        (d.Specialty != null && d.Specialty.SpecialtyName != null && d.Specialty.SpecialtyName.ToLower().Contains(k))
                    );
                }

                if (request.SpecialtyId.HasValue)
                    query = query.Where(d => d.SpecialtyId == request.SpecialtyId.Value);

                // sắp xếp (chỉ áp dụng cho export nếu muốn)
                switch (request.SortBy?.ToLower())
                {
                    case "fullname": query = request.IsDescending ? query.OrderByDescending(d => d.FullName) : query.OrderBy(d => d.FullName); break;
                    case "specialtyname": query = request.IsDescending ? query.OrderByDescending(d => d.Specialty!.SpecialtyName) : query.OrderBy(d => d.Specialty!.SpecialtyName); break;
                    case "phone": query = request.IsDescending ? query.OrderByDescending(d => d.Phone) : query.OrderBy(d => d.Phone); break;
                    case "email": query = request.IsDescending ? query.OrderByDescending(d => d.Email) : query.OrderBy(d => d.Email); break;
                    default: query = request.IsDescending ? query.OrderByDescending(d => d.DoctorId) : query.OrderBy(d => d.DoctorId); break;
                }
            }

            var doctors = query.ToList();

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("DanhSachBacSi");

                // Title
                ws.Cells["A1"].Value = "DANH SÁCH BÁC SĨ PHÒNG KHÁM";
                ws.Cells["A1:E1"].Merge = true;
                ws.Cells["A1"].Style.Font.Bold = true;
                ws.Cells["A1"].Style.Font.Size = 16;
                ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Header
                string[] headers = { "Mã BS", "Họ tên", "Chuyên khoa", "Số điện thoại", "Email" };
                for (int c = 0; c < headers.Length; c++)
                {
                    var cell = ws.Cells[3, c + 1];
                    cell.Value = headers[c];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Data
                int row = 4;
                foreach (var d in doctors)
                {
                    ws.Cells[row, 1].Value = d.DoctorId;
                    ws.Cells[row, 2].Value = d.FullName;
                    ws.Cells[row, 3].Value = d.Specialty?.SpecialtyName;
                    ws.Cells[row, 4].Value = d.Phone;
                    ws.Cells[row, 5].Value = d.Email;

                    for (int col = 1; col <= 5; col++)
                        ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    row++;
                }

                ws.Cells.AutoFitColumns();
                return package.GetAsByteArray();
            }
        }
    }
}

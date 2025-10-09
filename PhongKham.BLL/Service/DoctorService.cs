using Microsoft.EntityFrameworkCore;
using PhongKham.DAL.Entities;
using System;
using System.Collections.Generic;
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

        // ✅ Lấy tất cả bác sĩ kèm chuyên khoa
        public IEnumerable<Doctor> GetAll()
        {
            return _context.Doctors
                .Include(d => d.Specialty)
                .OrderBy(d => d.FullName)
                .ToList();
        }

        // ✅ Lấy 1 bác sĩ theo ID (kèm chuyên khoa)
        public Doctor? GetById(int id)
        {
            return _context.Doctors
                .Include(d => d.Specialty)
                .FirstOrDefault(d => d.DoctorId == id);
        }

        // ✅ Tạo mới bác sĩ (kiểm tra trùng Email/SĐT)
        public void Create(Doctor doctor)
        {
            if (_context.Doctors.Any(d => d.Email == doctor.Email))
                throw new Exception("Email đã được sử dụng cho bác sĩ khác.");

            if (_context.Doctors.Any(d => d.Phone == doctor.Phone))
                throw new Exception("Số điện thoại đã được sử dụng cho bác sĩ khác.");

            _context.Doctors.Add(doctor);
            _context.SaveChanges();

            Console.WriteLine($"✅ Thêm bác sĩ: {doctor.FullName}");
        }

        // ✅ Cập nhật bác sĩ
        public void Update(Doctor doctor)
        {
            _context.Doctors.Update(doctor);
            _context.SaveChanges();

            Console.WriteLine($"✏️ Cập nhật thông tin bác sĩ: {doctor.FullName}");
        }

        // ✅ Xóa bác sĩ
        public void Delete(int id)
        {
            var doc = _context.Doctors.FirstOrDefault(d => d.DoctorId == id);
            if (doc == null)
                throw new Exception("Không tìm thấy bác sĩ để xóa.");

            _context.Doctors.Remove(doc);
            _context.SaveChanges();

            Console.WriteLine($"🗑️ Đã xóa bác sĩ: {doc.FullName}");
        }
    }
}
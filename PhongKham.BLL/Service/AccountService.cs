using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhongKham.DAL.Entities;

namespace PhongKham.BLL.Service
{
    public class AccountService
    {
        private readonly PhongKhamDbContext _context;

        public AccountService(PhongKhamDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả tài khoản
        public IEnumerable<Account> GetAll()
        {
            return _context.Accounts.ToList();
        }

        // Lấy 1 tài khoản theo ID
        public Account? GetById(int id)
        {
            return _context.Accounts.FirstOrDefault(a => a.AccountId == id);
        }

        public Account? GetByUsername(string username)
        {
            return _context.Accounts.FirstOrDefault(a => a.Username == username);
        }


        // Tạo mới
        public void Create(Account account)
        {
            _context.Accounts.Add(account);
            _context.SaveChanges();
        }

        // Cập nhật
        public void Update(Account account)
        {
            _context.Accounts.Update(account);
            _context.SaveChanges();
        }

        // Xóa
        public void Delete(int id)
        {
            var acc = _context.Accounts.FirstOrDefault(a => a.AccountId == id);
            if (acc != null)
            {
                _context.Accounts.Remove(acc);
                _context.SaveChanges();
            }
        }
    }
}

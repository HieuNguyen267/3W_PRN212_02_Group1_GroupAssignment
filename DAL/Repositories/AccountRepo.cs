using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class AccountRepo
    {
        private readonly ShoppingOnlineContext _context;

        public AccountRepo()
        {
            _context = new ShoppingOnlineContext();
        }

        public Account? GetAccountByEmail(string email)
        {
            return _context.Accounts
                .Include(a => a.Customers)
                .Include(a => a.Admins)
                .FirstOrDefault(a => a.Email == email);
        }

        public Account? GetAccountByEmailOrPhone(string emailOrPhone)
        {
            return _context.Accounts
                .Include(a => a.Customers)
                .Include(a => a.Admins)
                .FirstOrDefault(a => a.Email == emailOrPhone);
        }

        public bool ValidatePassword(string emailOrPhone, string password)
        {
            var account = GetAccountByEmailOrPhone(emailOrPhone);
            return account != null && account.Password == password; // In real app, use hashed password
        }

        public List<Account> GetAllAccounts()
        {
            return _context.Accounts
                .Include(a => a.Customers)
                .Include(a => a.Admins)
                .ToList();
        }

        public void AddAccount(Account account)
        {
            _context.Accounts.Add(account);
            _context.SaveChanges();
        }

        public void UpdateAccount(Account account)
        {
            _context.Accounts.Update(account);
            _context.SaveChanges();
        }

        public void DeleteAccount(int accountId)
        {
            var account = _context.Accounts.Find(accountId);
            if (account != null)
            {
                _context.Accounts.Remove(account);
                _context.SaveChanges();
            }
        }
    }
}

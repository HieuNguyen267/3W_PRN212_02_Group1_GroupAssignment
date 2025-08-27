using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DAL.Entities;

namespace DAL.Repositories
{
    public class CarrierRepo
    {
        private readonly ShoppingOnlineContext _context;

        public CarrierRepo()
        {
            _context = new ShoppingOnlineContext();
        }

        public List<Carrier> GetAllCarriers()
        {
            return _context.Carriers
                .Include(c => c.Account)
                .OrderByDescending(c => c.CreatedDate)
                .ToList();
        }

        public Carrier? GetCarrierById(int carrierId)
        {
            return _context.Carriers
                .Include(c => c.Account)
                .FirstOrDefault(c => c.CarrierId == carrierId);
        }

        public bool AddCarrier(Carrier carrier, Account account)
        {
            try
            {
                // Check duplication by username or email
                bool exists = _context.Accounts.Any(a => a.Username == account.Username || a.Email == account.Email);
                if (exists) return false;

                using var transaction = _context.Database.BeginTransaction();

                account.AccountType = "Carrier";
                account.CreatedDate = DateTime.Now;
                account.IsActive = true;
                _context.Accounts.Add(account);
                _context.SaveChanges();

                carrier.AccountId = account.AccountId;
                carrier.CreatedDate = DateTime.Now;
                carrier.IsAvailable = true;
                _context.Carriers.Add(carrier);
                _context.SaveChanges();

                transaction.Commit();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateCarrier(Carrier carrier)
        {
            try
            {
                var existing = _context.Carriers
                    .Include(c => c.Account)
                    .FirstOrDefault(c => c.CarrierId == carrier.CarrierId);
                if (existing == null) return false;

                existing.FullName = carrier.FullName;
                existing.Phone = carrier.Phone;
                existing.VehicleNumber = carrier.VehicleNumber;
                existing.IsAvailable = carrier.IsAvailable;

                if (existing.Account != null && carrier.Account != null)
                {
                    existing.Account.Username = carrier.Account.Username;
                    existing.Account.Email = carrier.Account.Email;
                    if (!string.IsNullOrWhiteSpace(carrier.Account.Password))
                    {
                        existing.Account.Password = carrier.Account.Password;
                    }
                }

                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteCarrier(int carrierId)
        {
            try
            {
                var carrier = _context.Carriers
                    .Include(c => c.Account)
                    .FirstOrDefault(c => c.CarrierId == carrierId);
                if (carrier?.Account == null) return false;

                // Soft delete: deactivate account
                carrier.Account.IsActive = false;
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateCarrierAvailable(int carrierId, bool isAvailable)
        {
            try
            {
                var carrier = _context.Carriers.Find(carrierId);
                if (carrier == null) return false;
                carrier.IsAvailable = isAvailable;
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateAccountStatus(int accountId, bool isActive)
        {
            try
            {
                var account = _context.Accounts.Find(accountId);
                if (account == null) return false;
                account.IsActive = isActive;
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UsernameOrEmailExists(string username, string email)
        {
            return _context.Accounts.Any(a => a.Username == username || a.Email == email);
        }

        public List<Carrier> GetAvailableCarriers()
        {
            return _context.Carriers
                .Include(c => c.Account)
                .Where(c => c.IsAvailable == true && c.Account!.IsActive == true)
                .ToList();
        }
    }
}



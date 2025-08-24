using DAL.Entities;
using DAL.Repositories;

namespace BLL.Services
{
    public class AccountService
    {
        private readonly AccountRepo _accountRepo;

        public AccountService()
        {
            _accountRepo = new AccountRepo();
        }

        public LoginResult Login(string emailOrPhone, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(emailOrPhone) || string.IsNullOrEmpty(password))
                {
                    return new LoginResult
                    {
                        IsSuccess = false,
                        Message = "Vui lòng nhập đầy đủ thông tin!"
                    };
                }

                var account = _accountRepo.GetAccountByEmailOrPhone(emailOrPhone);
                
                if (account == null)
                {
                    return new LoginResult
                    {
                        IsSuccess = false,
                        Message = "Email không tồn tại!"
                    };
                }

                if (account.Password != password) // In real app, use hashed password comparison
                {
                    return new LoginResult
                    {
                        IsSuccess = false,
                        Message = "Mật khẩu không đúng!"
                    };
                }

                // Check if it's an Admin account
                if (account.AccountType == "Admin")
                {
                    var admin = account.Admins.FirstOrDefault();
                    if (admin == null)
                    {
                        return new LoginResult
                        {
                            IsSuccess = false,
                            Message = "Thông tin admin không hợp lệ!"
                        };
                    }

                    return new LoginResult
                    {
                        IsSuccess = true,
                        Message = "Đăng nhập thành công với quyền Admin!",
                        IsAdmin = true,
                        Admin = admin
                    };
                }

                // Handle Customer accounts
                var customer = account.Customers.FirstOrDefault();
                if (customer == null)
                {
                    return new LoginResult
                    {
                        IsSuccess = false,
                        Message = "Thông tin khách hàng không hợp lệ!"
                    };
                }

                return new LoginResult
                {
                    IsSuccess = true,
                    Message = "Đăng nhập thành công!",
                    IsAdmin = false,
                    Customer = customer
                };
            }
            catch (Exception ex)
            {
                return new LoginResult
                {
                    IsSuccess = false,
                    Message = $"Lỗi hệ thống: {ex.Message}"
                };
            }
        }

        public List<Account> GetAllAccounts()
        {
            return _accountRepo.GetAllAccounts();
        }

        public void AddAccount(Account account)
        {
            _accountRepo.AddAccount(account);
        }

        public void UpdateAccount(Account account)
        {
            _accountRepo.UpdateAccount(account);
        }

        public void DeleteAccount(int accountId)
        {
            _accountRepo.DeleteAccount(accountId);
        }

        public Customer? GetCustomerById(int customerId)
        {
            return _accountRepo.GetCustomerById(customerId);
        }

        public void UpdateCustomer(Customer customer)
        {
            _accountRepo.UpdateCustomer(customer);
        }
    }

    public class LoginResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public Customer? Customer { get; set; }
        public bool IsAdmin { get; set; } // Indicates if the logged-in user is an admin
        public Admin? Admin { get; set; } // Admin information, if applicable
    }
}

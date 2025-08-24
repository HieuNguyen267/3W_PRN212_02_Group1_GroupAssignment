using DAL.Entities;
using BLL.Services;
using System.Windows;

namespace ShoppingOnline
{
    public static class AdminSession
    {
        public static bool IsLoggedIn { get; private set; } = false;
        public static int? AdminId { get; private set; } = null;
        public static string? AdminName { get; private set; } = null;
        public static string? Email { get; private set; } = null;
        public static string? Phone { get; private set; } = null;

        public static void Login(Admin admin)
        {
            IsLoggedIn = true;
            AdminId = admin.AdminId;
            AdminName = admin.FullName;
            Email = admin.Account?.Email;
            Phone = admin.Phone;
        }

        public static void Logout()
        {
            IsLoggedIn = false;
            AdminId = null;
            AdminName = null;
            Email = null;
            Phone = null;
        }
    }
}
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
        public static bool IsNavigating { get; set; } = false; // Track navigation state

        // Event to notify when admin operations are completed
        public static event EventHandler? AdminOperationsCompleted;

        public static void Login(Admin admin)
        {
            IsLoggedIn = true;
            AdminId = admin.AdminId;
            AdminName = admin.FullName;
            Email = admin.Account?.Email;
            Phone = admin.Phone;
            IsNavigating = false; // Reset navigation state on login
        }

        public static void Logout()
        {
            IsLoggedIn = false;
            AdminId = null;
            AdminName = null;
            Email = null;
            Phone = null;
            IsNavigating = false; // Reset navigation state on logout
        }

        public static void StartNavigation()
        {
            IsNavigating = true;
        }

        public static void EndNavigation()
        {
            IsNavigating = false;
        }

        // Method to notify that admin operations are completed
        public static void NotifyOperationsCompleted()
        {
            AdminOperationsCompleted?.Invoke(null, EventArgs.Empty);
        }
    }
}
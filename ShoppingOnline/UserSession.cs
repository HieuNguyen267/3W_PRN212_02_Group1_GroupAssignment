namespace ShoppingOnline
{
    public static class UserSession
    {
        public static bool IsLoggedIn { get; private set; } = false;
        public static int? CustomerId { get; private set; } = null;
        public static string? CustomerName { get; private set; } = null;

        public static void Login(int customerId, string customerName)
        {
            IsLoggedIn = true;
            CustomerId = customerId;
            CustomerName = customerName;
        }

        public static void Logout()
        {
            IsLoggedIn = false;
            CustomerId = null;
            CustomerName = null;
        }

        public static bool ShowLoginDialog()
        {
            var loginWindow = new LoginWindow();
            var result = loginWindow.ShowDialog();
            
            if (result == true && loginWindow.IsLoggedIn)
            {
                Login(loginWindow.LoggedInCustomerId!.Value, loginWindow.LoggedInCustomerName!);
                return true;
            }
            
            return false;
        }
    }
}

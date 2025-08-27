using BLL.Services;
using DAL.Entities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ShoppingOnline
{
    public partial class UserProfileWindow : Window, INotifyPropertyChanged
    {
        private readonly AccountService _accountService;
        private readonly OrderService _orderService;
        private Customer _currentCustomer;

        public UserProfileWindow()
        {
            InitializeComponent();
            _accountService = new AccountService();
            _orderService = new OrderService();
            
            LoadUserData();
            DataContext = this;
        }

        #region Properties
        private string _customerName = "";
        public string CustomerName
        {
            get => _customerName;
            set
            {
                _customerName = value;
                OnPropertyChanged(nameof(CustomerName));
            }
        }

        private string _email = "";
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        private string _phone = "";
        public string Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged(nameof(Phone));
            }
        }

        private string _address = "";
        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        private string _createdDate = "";
        public string CreatedDate
        {
            get => _createdDate;
            set
            {
                _createdDate = value;
                OnPropertyChanged(nameof(CreatedDate));
            }
        }

        private int _totalOrders = 0;
        public int TotalOrders
        {
            get => _totalOrders;
            set
            {
                _totalOrders = value;
                OnPropertyChanged(nameof(TotalOrders));
            }
        }

        private string _totalSpent = "0₫";
        public string TotalSpent
        {
            get => _totalSpent;
            set
            {
                _totalSpent = value;
                OnPropertyChanged(nameof(TotalSpent));
            }
        }

        private string _membershipLevel = "Thành viên mới";
        public string MembershipLevel
        {
            get => _membershipLevel;
            set
            {
                _membershipLevel = value;
                OnPropertyChanged(nameof(MembershipLevel));
            }
        }

        public ObservableCollection<UserOrderViewModel> RecentOrders { get; set; } = new ObservableCollection<UserOrderViewModel>();


        #endregion

        private void LoadUserData()
        {
            if (!UserSession.IsLoggedIn || UserSession.CustomerId == null)
            {
                MessageBox.Show("Vui lòng đăng nhập để xem thông tin cá nhân!", 
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Close();
                return;
            }

            try
            {
                // Load customer data
                _currentCustomer = _accountService.GetCustomerById(UserSession.CustomerId.Value);
                if (_currentCustomer != null)
                {
                    CustomerName = _currentCustomer.FullName ?? "";
                    Email = _currentCustomer.Account?.Email ?? "";
                    Phone = _currentCustomer.Phone ?? "";
                    Address = _currentCustomer.Address ?? "";
                    CreatedDate = _currentCustomer.CreatedDate?.ToString("dd/MM/yyyy") ?? "";
                }

                // Load orders data
                LoadOrdersData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOrdersData()
        {
            try
            {
                var orders = _orderService.GetAllOrdersByCustomer(UserSession.CustomerId.Value);
                
                // Calculate statistics - only count non-cancelled orders
                var nonCancelledOrders = orders.Where(o => o.Status != "Cancelled" && (o.Notes == null || !o.Notes.Contains("[CANCELLED]"))).ToList();
                TotalOrders = nonCancelledOrders.Count;
                var totalAmount = nonCancelledOrders.Sum(o => o.TotalAmount);
                TotalSpent = $"{totalAmount:N0}₫";
                
                // Calculate membership level
                MembershipLevel = CalculateMembershipLevel(totalAmount);
                
                // Load recent orders (last 5 orders) - show all orders including cancelled ones
                RecentOrders.Clear();
                var recentOrders = orders.OrderByDescending(o => o.OrderDate).Take(5);
                
                foreach (var order in recentOrders)
                {
                    RecentOrders.Add(new UserOrderViewModel
                    {
                        OrderId = order.OrderId,
                        OrderDate = order.OrderDate ?? DateTime.Now,
                        Status = order.Status ?? "Pending",
                        TotalAmount = order.TotalAmount
                    });
                }
                
                // Update visibility of no orders text
                if (RecentOrders.Count == 0)
                {
                    NoOrdersText.Visibility = Visibility.Visible;
                }
                else
                {
                    NoOrdersText.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu đơn hàng: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string CalculateMembershipLevel(decimal totalSpent)
        {
            return totalSpent switch
            {
                >= 10000000 => "VIP Diamond",
                >= 5000000 => "VIP Gold", 
                >= 2000000 => "VIP Silver",
                >= 500000 => "Thành viên Bạc",
                >= 100000 => "Thành viên Đồng",
                _ => "Thành viên mới"
            };
        }

        #region Event Handlers
        private void AccountSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var accountSettingsWindow = new AccountSettingsWindow();
                var result = accountSettingsWindow.ShowDialog();
                
                if (result == true)
                {
                    // Reload user data if settings were changed
                    LoadUserData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở cài đặt tài khoản: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewOrders_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var orderHistoryWindow = new OrderHistoryWindow(UserSession.CustomerId.Value);
                orderHistoryWindow.ShowDialog();
                
                // Reload orders data
                LoadOrdersData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở lịch sử đơn hàng: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewCart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cartWindow = new CartWindow();
                cartWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở giỏ hàng: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OrderDetail_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                try
                {
                    // Show order details in a message box for now
                    var order = _orderService.GetOrderById(orderId);
                    if (order != null)
                    {
                        var details = $"Đơn hàng #{orderId}\n" +
                                     $"Ngày đặt: {order.OrderDate:dd/MM/yyyy HH:mm}\n" +
                                     $"Trạng thái: {order.Status}\n" +
                                     $"Tổng tiền: {order.TotalAmount:N0}₫\n" +
                                     $"Địa chỉ giao: {order.ShippingAddress}\n" +
                                     $"Ghi chú: {order.Notes}";
                        
                        MessageBox.Show(details, "Chi tiết đơn hàng", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xem chi tiết đơn hàng: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public class UserOrderViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = "";
        public decimal TotalAmount { get; set; }
    }
}

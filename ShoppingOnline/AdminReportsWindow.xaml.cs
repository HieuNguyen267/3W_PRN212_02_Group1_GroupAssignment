using BLL.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ShoppingOnline
{
    public partial class AdminReportsWindow : Window, INotifyPropertyChanged
    {
        private readonly IAdminService _adminService;
        private ObservableCollection<ReportData> _reportData = new();

        public AdminReportsWindow()
        {
            InitializeComponent();
            _adminService = new AdminService();
            DataContext = this;
            
            // Set default date range (last 30 days)
            EndDatePicker.SelectedDate = DateTime.Today;
            StartDatePicker.SelectedDate = DateTime.Today.AddDays(-30);
        }

        public ObservableCollection<ReportData> ReportData
        {
            get => _reportData;
            set
            {
                _reportData = value;
                OnPropertyChanged(nameof(ReportData));
            }
        }

        #region Event Handlers
        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            var startDate = StartDatePicker.SelectedDate ?? DateTime.Today.AddDays(-30);
            var endDate = EndDatePicker.SelectedDate ?? DateTime.Today;
            
            GenerateReportForPeriod(startDate, endDate);
        }

        private void TodayReport_Click(object sender, RoutedEventArgs e)
        {
            var today = DateTime.Today;
            StartDatePicker.SelectedDate = today;
            EndDatePicker.SelectedDate = today;
            GenerateReportForPeriod(today, today);
        }

        private void WeekReport_Click(object sender, RoutedEventArgs e)
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-7);
            StartDatePicker.SelectedDate = startDate;
            EndDatePicker.SelectedDate = endDate;
            GenerateReportForPeriod(startDate, endDate);
        }

        private void MonthReport_Click(object sender, RoutedEventArgs e)
        {
            var endDate = DateTime.Today;
            var startDate = new DateTime(endDate.Year, endDate.Month, 1);
            StartDatePicker.SelectedDate = startDate;
            EndDatePicker.SelectedDate = endDate;
            GenerateReportForPeriod(startDate, endDate);
        }

        private void YearReport_Click(object sender, RoutedEventArgs e)
        {
            var endDate = DateTime.Today;
            var startDate = new DateTime(endDate.Year, 1, 1);
            StartDatePicker.SelectedDate = startDate;
            EndDatePicker.SelectedDate = endDate;
            GenerateReportForPeriod(startDate, endDate);
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tinh nang xuat Excel se duoc phat trien sau!", "Thong bao", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PrintReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tinh nang in bao cao se duoc phat trien sau!", "Thong bao", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BackToDashboard_Click(object sender, RoutedEventArgs e)
        {
            // Not needed anymore in single-window navigation
            MessageBox.Show("Navigation ?� ???c c?p nh?t! Vui l�ng s? d?ng menu b�n tr�i ?? chuy?n trang.", "Th�ng b�o", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Not needed anymore in single-window navigation
            AdminSession.EndNavigation();
            base.OnClosing(e);
        }
        #endregion

        private void GenerateReportForPeriod(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Get basic statistics
                var totalOrders = _adminService.GetTotalOrders();
                var totalRevenue = _adminService.GetTotalRevenue();
                var totalCustomers = _adminService.GetTotalCustomers();

                // Update summary
                TotalOrdersReport.Text = totalOrders.ToString("N0");
                TotalRevenueReport.Text = $"{totalRevenue:N0}d";
                TopProductReport.Text = "Dang cap nhat...";
                NewCustomersReport.Text = totalCustomers.ToString("N0");

                // Generate sample report data
                ReportData.Clear();
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var random = new Random(date.GetHashCode());
                    ReportData.Add(new ReportData
                    {
                        Date = date,
                        OrderCount = random.Next(0, 20),
                        Revenue = random.Next(1000000, 10000000),
                        NewCustomers = random.Next(0, 10),
                        ProductsSold = random.Next(0, 50)
                    });
                }

                ReportDataGrid.ItemsSource = ReportData;

                MessageBox.Show($"Da tao bao cao tu {startDate:dd/MM/yyyy} den {endDate:dd/MM/yyyy}", 
                    "Thong bao", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo báo cáo: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    // Report data model
    public class ReportData
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public int NewCustomers { get; set; }
        public int ProductsSold { get; set; }
    }
}
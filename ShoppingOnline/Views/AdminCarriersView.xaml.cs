using BLL.Services;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ShoppingOnline.Views
{
    public partial class AdminCarriersView : UserControl, INotifyPropertyChanged
    {
        private readonly IAdminService _adminService;
        private ObservableCollection<Carrier> _carriers = new();
        private List<Carrier> _allCarriers = new();

        public AdminCarriersView()
        {
            InitializeComponent();
            _adminService = new AdminService();
            DataContext = this;
            
            Loaded += AdminCarriersView_Loaded;
        }

        private void AdminCarriersView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCarriers();
        }

        public ObservableCollection<Carrier> Carriers
        {
            get => _carriers;
            set
            {
                _carriers = value;
                OnPropertyChanged(nameof(Carriers));
            }
        }

        private void LoadCarriers()
        {
            try
            {
                _allCarriers = _adminService.GetAllCarriers();
                ApplyCarrierFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi t?i d? li?u nh� v?n chuy?n: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyCarrierFilter()
        {
            try
            {
                var filteredCarriers = _allCarriers.AsEnumerable();

                // Apply search filter
                var searchText = CarrierSearchBox?.Text?.Trim();
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    var searchLower = searchText.ToLower();
                    filteredCarriers = filteredCarriers.Where(c => 
                        (c.FullName?.ToLower().Contains(searchLower) == true) ||
                        (c.Account?.Email?.ToLower().Contains(searchLower) == true) ||
                        (c.Phone?.ToLower().Contains(searchLower) == true) ||
                        (c.VehicleNumber?.ToLower().Contains(searchLower) == true));
                }

                // Apply status filter using stable Tag values from XAML (All | Active | Locked)
                var selectedItem = StatusFilterComboBox?.SelectedItem as ComboBoxItem;
                var selectedTag = selectedItem?.Tag?.ToString();
                if (!string.IsNullOrEmpty(selectedTag) && selectedTag != "All")
                {
                    bool isActive = selectedTag == "Active";
                    filteredCarriers = filteredCarriers.Where(c => c.Account?.IsActive == isActive);
                }

                // Update carriers collection
                Carriers.Clear();
                foreach (var carrier in filteredCarriers.OrderBy(c => c.FullName))
                {
                    Carriers.Add(carrier);
                }

                // Update DataGrid
                if (CarriersDataGrid != null)
                {
                    CarriersDataGrid.ItemsSource = Carriers;
                }
                
                // Update count
                if (CarrierCountText != null)
                {
                    CarrierCountText.Text = $"T?ng: {filteredCarriers.Count()} nh� v?n chuy?n";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi l?c nh� v?n chuy?n: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Event Handlers
        private void CarrierSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CarrierSearchBox != null)
            {
                CarrierSearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(CarrierSearchBox.Text) 
                    ? Visibility.Visible : Visibility.Hidden;
                
                ApplyCarrierFilter();
            }
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyCarrierFilter();
        }

        private void RefreshCarriers_Click(object sender, RoutedEventArgs e)
        {
            LoadCarriers();
            MessageBox.Show("?� l�m m?i danh s�ch nh� v?n chuy?n!", "Th�ng b�o", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddCarrier_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addCarrierWindow = new CarrierEditWindow();
                if (addCarrierWindow.ShowDialog() == true)
                {
                    LoadCarriers();
                    MessageBox.Show("Th�m nh� v?n chuy?n th�nh c�ng!", "Th�nh c�ng", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi th�m nh� v?n chuy?n: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewCarrier_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int carrierId)
            {
                try
                {
                    var carrier = _allCarriers.FirstOrDefault(c => c.CarrierId == carrierId);
                    if (carrier != null)
                    {
                        var info = $"Th�ng tin nh� v?n chuy?n #{carrierId}\n\n" +
                                  $"T�n: {carrier.FullName}\n" +
                                  $"Email: {carrier.Account?.Email}\n" +
                                  $"?i?n tho?i: {carrier.Phone}\n" +
                                  $"S? xe: {carrier.VehicleNumber}\n" +
                                  $"Ng�y t?o: {carrier.CreatedDate:dd/MM/yyyy}\n" +
                                  $"Tr?ng th�i TK: {(carrier.Account?.IsActive == true ? "Ho?t ??ng" : "Kh�a")}\n" +
                                  $"S?n s�ng giao h�ng: {(carrier.IsAvailable == true ? "C�" : "Kh�ng")}";
                        
                        MessageBox.Show(info, "Th�ng tin nh� v?n chuy?n", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"L?i khi xem th�ng tin nh� v?n chuy?n: {ex.Message}", "L?i", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditCarrier_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int carrierId)
            {
                try
                {
                    var carrier = _allCarriers.FirstOrDefault(c => c.CarrierId == carrierId);
                    if (carrier != null)
                    {
                        var editCarrierWindow = new CarrierEditWindow(carrier);
                        if (editCarrierWindow.ShowDialog() == true)
                        {
                            LoadCarriers();
                            MessageBox.Show("C?p nh?t th�ng tin nh� v?n chuy?n th�nh c�ng!", "Th�nh c�ng", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"L?i khi s?a th�ng tin nh� v?n chuy?n: {ex.Message}", "L?i", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteCarrier_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int carrierId)
            {
                var result = MessageBox.Show($"B?n c� ch?c mu?n x�a nh� v?n chuy?n #{carrierId}?\n" +
                    "Vi?c n�y s? v� hi?u h�a t�i kho?n c?a nh� v?n chuy?n.", 
                    "X�c nh?n x�a", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (_adminService.DeleteCarrier(carrierId))
                        {
                            LoadCarriers();
                            MessageBox.Show("?� x�a nh� v?n chuy?n th�nh c�ng!", "Th�nh c�ng", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Kh�ng th? x�a nh� v?n chuy?n!", "L?i", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"L?i khi x�a nh� v?n chuy?n: {ex.Message}", "L?i", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ToggleCarrierAvailable_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int carrierId)
            {
                try
                {
                    var carrier = _allCarriers.FirstOrDefault(c => c.CarrierId == carrierId);
                    if (carrier != null)
                    {
                        bool newStatus = !carrier.IsAvailable.GetValueOrDefault();
                        string statusText = newStatus ? "??t s?n s�ng" : "??t kh�ng s?n s�ng";
                        
                        var result = MessageBox.Show($"B?n c� mu?n {statusText} giao h�ng cho nh� v?n chuy?n n�y?", 
                            "X�c nh?n", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        
                        if (result == MessageBoxResult.Yes)
                        {
                            if (_adminService.UpdateCarrierStatus(carrierId, newStatus))
                            {
                                LoadCarriers();
                                MessageBox.Show($"?� {statusText} giao h�ng th�nh c�ng!", "Th�nh c�ng", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Kh�ng th? c?p nh?t tr?ng th�i s?n s�ng!", "L?i", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"L?i khi c?p nh?t tr?ng th�i: {ex.Message}", "L?i", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ToggleCarrierStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int carrierId)
            {
                try
                {
                    var carrier = _allCarriers.FirstOrDefault(c => c.CarrierId == carrierId);
                    if (carrier?.Account != null)
                    {
                        bool newStatus = !carrier.Account.IsActive.GetValueOrDefault();
                        string statusText = newStatus ? "k�ch ho?t" : "v� hi?u h�a";
                        
                        var result = MessageBox.Show($"B?n c� mu?n {statusText} t�i kho?n nh� v?n chuy?n n�y?", 
                            "X�c nh?n", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        
                        if (result == MessageBoxResult.Yes)
                        {
                            if (_adminService.UpdateAccountStatus(carrier.Account.AccountId, newStatus))
                            {
                                LoadCarriers();
                                MessageBox.Show($"?� {statusText} t�i kho?n th�nh c�ng!", "Th�nh c�ng", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Kh�ng th? c?p nh?t tr?ng th�i t�i kho?n!", "L?i", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"L?i khi c?p nh?t tr?ng th�i: {ex.Message}", "L?i", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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
}
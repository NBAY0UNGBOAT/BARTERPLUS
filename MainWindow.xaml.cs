using System;
using System.Windows;
using System.Windows.Input;
using BarterPOS.Models;
using BarterPOS.Services;
using BarterPOS.ViewModels;

namespace BarterPOS
{
    public partial class MainWindow : Window
    {
        private SalesViewModel? ViewModel => DataContext as SalesViewModel;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                DataContext = new SalesViewModel();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting DataContext: {ex.Message}\n{ex.StackTrace}");
            }

            ManageUsersButton.Visibility = Session.CurrentUser?.Role == "Admin"
                ? Visibility.Visible
                : Visibility.Collapsed;
            ClearPendingButton.Visibility = Session.CurrentUser?.Role == "Admin"
                ? Visibility.Visible
                : Visibility.Collapsed;

            PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        private void ManageUsers_Click(object sender, RoutedEventArgs e)
        {
            var window = new UserManagementWindow();
            window.ShowDialog();
        }

        private void TransactionHistory_Click(object sender, RoutedEventArgs e)
        {
            var window = new TransactionHistoryWindow
            {
                Owner = this
            };

            window.ShowDialog();
        }

        private void EndOfDayReconciliation_Click(object sender, RoutedEventArgs e)
        {
            var window = new EndOfDayReconciliationWindow
            {
                Owner = this
            };

            window.ShowDialog();
        }

        private void ClearSale_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.ClearSale();
        }

        private void AddCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                MessageBox.Show("The sales screen is not ready yet.", "Customer Loyalty", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new CustomerLoyaltyWindow
            {
                Owner = this
            };

            if (window.ShowDialog() != true || window.SelectedCustomer == null)
            {
                return;
            }

            bool assigned = ViewModel.SetCustomer(window.SelectedCustomer, out string message);
            MessageBox.Show(
                message,
                "Customer Loyalty",
                MessageBoxButton.OK,
                assigned ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }

        private void ItemSearch_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                MessageBox.Show("The sales screen is not ready yet.", "Item Search", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new ProductSearchWindow
            {
                Owner = this
            };

            if (window.ShowDialog() != true || window.SelectedProduct == null)
            {
                FocusScanBox();
                return;
            }

            ViewModel.AddProduct(window.SelectedProduct);
            FocusScanBox();
        }

        private void VoidLine_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                MessageBox.Show("The sales screen is not ready yet.", "Void Line", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ViewModel.RemoveSelectedLineItem(out string message))
            {
                MessageBox.Show(message, "Void Line", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            FocusScanBox();
        }

        private void ClearCustomer_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.ClearCustomer();
        }

        private void CreateLoyaltyCard_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                MessageBox.Show("The sales screen is not ready yet.", "Create Loyalty Card", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new CreateLoyaltyCustomerWindow
            {
                Owner = this
            };

            if (window.ShowDialog() != true || window.CreatedCustomer == null)
            {
                return;
            }

            bool assigned = ViewModel.SetCustomer(window.CreatedCustomer, out string message);
            MessageBox.Show(
                assigned
                    ? $"Customer #{window.CreatedCustomer.Id} is ready to use on this sale."
                    : message,
                "Create Loyalty Card",
                MessageBoxButton.OK,
                assigned ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }

        private void CashPayment_Click(object sender, RoutedEventArgs e)
        {
            CompleteSale("Cash");
        }

        private void CardPayment_Click(object sender, RoutedEventArgs e)
        {
            CompleteSale("Card");
        }

        private void SyncOffline_Click(object sender, RoutedEventArgs e)
        {
            string message = ViewModel?.SyncOfflineData() ?? "The sales screen is not ready yet.";
            MessageBox.Show(message, "Offline Sync", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearPendingOffline_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                MessageBox.Show("The sales screen is not ready yet.", "Clear Pending Records", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult confirmation = MessageBox.Show(
                "This will permanently remove all pending offline transactions and cash drawer entries saved on this machine. Continue?",
                "Clear Pending Records",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            string message = ViewModel.ClearPendingOfflineData();
            MessageBox.Show(message, "Clear Pending Records", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CashIn_Click(object sender, RoutedEventArgs e)
        {
            RecordCashMovement(CashDrawerEntryTypes.CashIn);
        }

        private void CashOut_Click(object sender, RoutedEventArgs e)
        {
            RecordCashMovement(CashDrawerEntryTypes.CashOut);
        }

        private void CompleteSale(string paymentMethod)
        {
            if (ViewModel == null)
            {
                MessageBox.Show("The sales screen is not ready yet.", "Checkout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool completed = ViewModel.CompleteSale(paymentMethod, out string message, out var transaction);

            if (!completed || transaction == null)
            {
                MessageBox.Show(message, "Checkout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var receiptWindow = new ReceiptWindow(transaction)
            {
                Owner = this
            };
            receiptWindow.ShowDialog();
        }

        private void RecordCashMovement(string movementType)
        {
            if (ViewModel == null)
            {
                MessageBox.Show("The sales screen is not ready yet.", "Cash Drawer", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new CashDrawerMovementWindow(movementType)
            {
                Owner = this
            };

            if (window.ShowDialog() != true)
            {
                return;
            }

            bool saved = ViewModel.RecordCashMovement(movementType, window.Amount, window.Note, out string message);
            if (!saved)
            {
                MessageBox.Show(
                    message,
                    "Cash Drawer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            FocusScanBox();
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F1:
                    ItemSearch_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    break;
                case Key.F2:
                    VoidLine_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    break;
                case Key.F3:
                    AddCustomer_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    break;
                case Key.F4:
                    ClearSale_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    break;
                case Key.F5:
                    CashPayment_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    break;
                case Key.F6:
                    CardPayment_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    break;
                case Key.F7:
                    TransactionHistory_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    break;
                case Key.F8:
                    EndOfDayReconciliation_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    break;
            }
        }

        private void FocusScanBox()
        {
            ScanTextBox.Focus();
            ScanTextBox.SelectAll();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Session.CurrentUser = null;

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}

using System;
using System.Windows;
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
            MessageBox.Show(
                message,
                "Cash Drawer",
                MessageBoxButton.OK,
                saved ? MessageBoxImage.Information : MessageBoxImage.Warning);
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
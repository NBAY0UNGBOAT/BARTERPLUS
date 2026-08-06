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

            // #region agent log
            try
            {
                var logLine = System.Text.Json.JsonSerializer.Serialize(new
                {
                    sessionId = "15cb7f",
                    runId = "pre-fix",
                    hypothesisId = "D",
                    location = "MainWindow.xaml.cs:ctor",
                    message = "Session state before SalesViewModel DataContext",
                    data = new
                    {
                        hasCurrentUser = Session.CurrentUser != null,
                        username = Session.CurrentUser?.Username,
                        role = Session.CurrentUser?.Role
                    },
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                }) + Environment.NewLine;
                System.IO.File.AppendAllText(@"E:\Github Projects\BARTERPLUS-main\debug-15cb7f.log", logLine);
            }
            catch { }
            // #endregion

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

            bool completed = ViewModel.CompleteSale(paymentMethod, out string message);
            MessageBox.Show(
                message,
                completed ? "Receipt" : "Checkout",
                MessageBoxButton.OK,
                completed ? MessageBoxImage.Information : MessageBoxImage.Warning);
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

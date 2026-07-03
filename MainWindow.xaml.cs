using System;
using System.Windows;
using BarterPOS.Services;
using BarterPOS.ViewModels;

namespace BarterPOS
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            try
            {
                this.DataContext = new SalesViewModel();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting DataContext: {ex.Message}\n{ex.StackTrace}");
                // Continue anyway - UI will still display, just no data binding
            }

            // Only Admins can manage account activation/deactivation.
            ManageUsersButton.Visibility = Session.CurrentUser?.Role == "Admin"
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void ManageUsers_Click(object sender, RoutedEventArgs e)
        {
            var window = new UserManagementWindow();
            window.ShowDialog();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Session.CurrentUser = null;

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}

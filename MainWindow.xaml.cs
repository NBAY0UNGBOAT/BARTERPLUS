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
                this.DataContext = new SalesViewModel();
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

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Session.CurrentUser = null;

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}

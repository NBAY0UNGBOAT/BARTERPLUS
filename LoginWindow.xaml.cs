using System.Windows;
using BarterPOS.Models;
using BarterPOS.Services;

namespace BarterPOS
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            string username = LoginUsername.Text.Trim();
            string password = LoginPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = UserStore.Repository.ValidateCredentials(username, password, out User? user, out string error);

            if (!success || user == null)
            {
                MessageBox.Show(error, "Sign In Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Session.CurrentUser = user;

            if (user.Role == "Admin")
            {
                var adminWindow = new UserManagementWindow();
                adminWindow.Show();
            }
            else
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }

            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

using System.Windows;
using BarterPOS.Models;
using BarterPOS.Services;

namespace BarterPOS
{
    public partial class AdminAuthorizationWindow : Window
    {
        public User? AuthorizedUser { get; private set; }

        public AdminAuthorizationWindow()
        {
            InitializeComponent();
        }

        private void Authorize_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            bool success = UserStore.Repository.ValidateCredentials(
                username,
                password,
                out User? user,
                out string error);

            if (!success || user == null)
            {
                MessageBox.Show(
                    error,
                    "Authorization Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (user.Role != "Admin")
            {
                MessageBox.Show(
                    "Only administrators can authorize this action.",
                    "Authorization Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            AuthorizedUser = user;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
using System.Windows;
using System.Windows.Controls;
using BarterPOS.Models;
using BarterPOS.Services;

namespace BarterPOS
{
    public partial class CreateUserWindow : Window
    {
        public CreateUserWindow()
        {
            InitializeComponent();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (RoleComboBox.SelectedItem is not ComboBoxItem selectedRole)
            {
                MessageBox.Show("Please select a permission.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string username = UsernameText.Text.Trim();
            string password = PasswordText.Password;
            string role = selectedRole.Content.ToString() ?? "Employee";

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Please enter a username.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                UsernameText.Focus();
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Temporary password must be at least 6 characters long.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordText.Focus();
                return;
            }

            var newUser = new User
            {
                EmployeeID = EmployeeIdText.Text.Trim(),
                FullName = FullNameText.Text.Trim(),
                Email = EmailText.Text.Trim(),
                ContactNumber = ContactNumberText.Text.Trim(),
                Username = username,
                Role = role,
                LastActivity = $"Account Created by {Session.CurrentUser?.Username ?? "Admin"}"
            };

            if (!UserStore.Repository.Register(newUser, password, out string error))
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("User created successfully.", "Create User", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

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
            EmployeeIdText.Text = UserStore.Repository.GetNextEmployeeId();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (RoleComboBox.SelectedItem is not ComboBoxItem selectedRole)
            {
                MessageBox.Show("Please select a permission.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string employeeId = EmployeeIdText.Text.Trim();
            string fullName = FullNameText.Text.Trim();
            string email = EmailText.Text.Trim();
            string contactNumber = ContactNumberText.Text.Trim();
            string username = UsernameText.Text.Trim();
            string password = PasswordText.Password;
            string role = selectedRole.Content.ToString() ?? "Employee";

            if (!InputValidator.IsValidEmployeeId(employeeId))
            {
                MessageBox.Show("Enter a valid employee ID using 3-20 letters, numbers, dashes, or underscores.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                EmployeeIdText.Focus();
                return;
            }

            if (!InputValidator.IsValidPersonName(fullName))
            {
                MessageBox.Show("Please enter the employee name using at least 2 characters.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                FullNameText.Focus();
                return;
            }

            if (!InputValidator.IsValidEmail(email))
            {
                MessageBox.Show("Please enter a valid email address or leave it blank.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailText.Focus();
                return;
            }

            if (!InputValidator.IsValidContactNumber(contactNumber))
            {
                MessageBox.Show("Please enter a valid contact number or leave it blank.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                ContactNumberText.Focus();
                return;
            }

            if (!InputValidator.IsValidUsername(username))
            {
                MessageBox.Show("Enter a valid username using 3-30 letters, numbers, dots, dashes, or underscores.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                EmployeeID = employeeId,
                FullName = fullName,
                Email = email,
                ContactNumber = contactNumber,
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

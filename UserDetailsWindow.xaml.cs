using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BarterPOS.Models;
using BarterPOS.Services;

namespace BarterPOS
{
    public partial class UserDetailsWindow : Window
    {
        private readonly int _userId;

        public UserDetailsWindow(User user)
        {
            InitializeComponent();
            _userId = user.Id;
            LoadUser(user);
        }

        private void LoadUser(User user)
        {
            TitleText.Text = $"{user.FullName} ({user.Username})";
            UsernameText.Text = user.Username;
            EmployeeNameText.Text = user.FullName;
            EmailText.Text = user.Email;
            ContactNumberText.Text = user.ContactNumber;
            DateCreatedText.Text = user.CreatedAt.ToString("MMM dd, yyyy h:mm tt");
            LastLoginText.Text = user.LastLoginDisplay;
            StatusText.Text = user.StatusDisplay;
            ToggleStatusButton.Content = user.IsActive ? "Disable Account" : "Enable Account";
            DeleteAccountButton.IsEnabled = user.Id != Session.CurrentUser?.Id;
            RoleComboBox.SelectedItem = RoleComboBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == NormalizeRole(user.Role));
            ActivityGrid.ItemsSource = UserStore.Repository.GetAuditLogForUser(user.Id);
        }

        private void Refresh()
        {
            var user = UserStore.Repository.GetById(_userId);
            if (user != null)
            {
                LoadUser(user);
            }
        }

        private void SaveInfo_Click(object sender, RoutedEventArgs e)
        {
            var user = UserStore.Repository.GetById(_userId);

            if (user == null)
            {
                MessageBox.Show("User not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string fullName = EmployeeNameText.Text.Trim();
            string email = EmailText.Text.Trim();
            string contactNumber = ContactNumberText.Text.Trim();

            if (!InputValidator.IsValidPersonName(fullName))
            {
                MessageBox.Show("Please enter the employee name using at least 2 characters.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                EmployeeNameText.Focus();
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

            user.FullName = fullName;
            user.Email = email;
            user.ContactNumber = contactNumber;

            string performedBy = Session.CurrentUser?.Username ?? "Unknown";
            if (!UserStore.Repository.UpdateUser(user, performedBy, out string error))
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Refresh();
        }

        private void ChangeRole_Click(object sender, RoutedEventArgs e)
        {
            if (RoleComboBox.SelectedItem is not ComboBoxItem selectedRole)
            {
                MessageBox.Show("Please select a role.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string role = selectedRole.Content.ToString() ?? "Employee";
            string performedBy = Session.CurrentUser?.Username ?? "Unknown";

            if (!UserStore.Repository.ChangeRole(_userId, role, performedBy, out string error))
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Refresh();
        }

        private void ToggleStatus_Click(object sender, RoutedEventArgs e)
        {
            var user = UserStore.Repository.GetById(_userId);

            if (user == null)
            {
                MessageBox.Show("User not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool newStatus = !user.IsActive;
            string performedBy = Session.CurrentUser?.Username ?? "Unknown";

            if (user.Id == Session.CurrentUser?.Id && !newStatus)
            {
                MessageBox.Show("You cannot disable your own account while signed in.", "Action Blocked", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!UserStore.Repository.SetActiveStatus(_userId, newStatus, performedBy, out string error))
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Refresh();
        }

        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (NewPasswordBox.Password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                NewPasswordBox.Focus();
                return;
            }

            string performedBy = Session.CurrentUser?.Username ?? "Unknown";
            if (!UserStore.Repository.ResetPassword(_userId, NewPasswordBox.Password, performedBy, out string error))
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NewPasswordBox.Clear();
            Refresh();
        }

        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            var user = UserStore.Repository.GetById(_userId);

            if (user == null)
            {
                MessageBox.Show("User not found.", "Delete Account", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (user.Id == Session.CurrentUser?.Id)
            {
                MessageBox.Show("You cannot delete your own signed-in account.", "Delete Account", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult confirm = MessageBox.Show(
                $"Delete account '{user.Username}'?",
                "Delete Account",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
            {
                return;
            }

            string performedBy = Session.CurrentUser?.Username ?? "Unknown";
            if (!UserStore.Repository.DeleteUser(_userId, performedBy, out string error))
            {
                MessageBox.Show(error, "Delete Account", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            Close();
        }

        private static string NormalizeRole(string role) =>
            role == "Cashier" ? "Employee" : role;
    }
}

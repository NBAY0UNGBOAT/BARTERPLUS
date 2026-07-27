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
            RoleComboBox.SelectedItem = RoleComboBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == user.Role);
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

            user.FullName = EmployeeNameText.Text.Trim();
            user.Email = EmailText.Text.Trim();
            user.ContactNumber = ContactNumberText.Text.Trim();

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
    }
}

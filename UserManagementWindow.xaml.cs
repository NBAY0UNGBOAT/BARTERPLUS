using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using BarterPOS.Models;
using BarterPOS.Services;

namespace BarterPOS
{
    public partial class UserManagementWindow : Window
    {
        private ObservableCollection<User> _users = new();
        private ObservableCollection<AuditLogEntry> _auditLog = new();

        public UserManagementWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            _users = new ObservableCollection<User>(UserStore.Repository.GetAllUsers());
            UsersGrid.ItemsSource = _users;

            _auditLog = new ObservableCollection<AuditLogEntry>(UserStore.Repository.GetAuditLog());
            AuditLogList.ItemsSource = _auditLog;
        }

        private void ToggleStatus_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int userId = (int)button.Tag;

            var user = UserStore.Repository.GetById(userId);
            if (user == null) return;

            bool newStatus = !user.IsActive;
            string performedBy = Session.CurrentUser?.Username ?? "Unknown";

            // Guard: don't let an admin lock themselves out.
            if (user.Id == Session.CurrentUser?.Id && !newStatus)
            {
                MessageBox.Show("You cannot deactivate your own account while signed in.", "Action Blocked", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = UserStore.Repository.SetActiveStatus(userId, newStatus, performedBy, out string error);

            if (!success)
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Refresh immediately so the grid and audit log reflect the change right away.
            LoadData();
        }
    }
}

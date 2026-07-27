using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BarterPOS.Models;
using BarterPOS.Services;

namespace BarterPOS
{
    public partial class UserManagementWindow : Window
    {
        private ObservableCollection<User> _users = new();

        public UserManagementWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            _users = new ObservableCollection<User>(UserStore.Repository.GetAllUsers());
            UsersGrid.ItemsSource = _users;

            TotalUsersText.Text = _users.Count.ToString();
            ActiveUsersText.Text = _users.Count(u => u.IsActive).ToString();
            DisabledUsersText.Text = _users.Count(u => !u.IsActive).ToString();
            AdminUsersText.Text = _users.Count(u => u.Role == "Admin").ToString();
        }

        private void ViewUser_Click(object sender, RoutedEventArgs e)
        {
            int userId = (int)((Button)sender).Tag;
            OpenUserDetails(userId);
        }

        private void CreateUser_Click(object sender, RoutedEventArgs e)
        {
            var window = new CreateUserWindow
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void UsersGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (UsersGrid.SelectedItem is User user)
            {
                OpenUserDetails(user.Id);
            }
        }

        private void OpenUserDetails(int userId)
        {
            var user = UserStore.Repository.GetById(userId);

            if (user == null)
            {
                MessageBox.Show("User not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var window = new UserDetailsWindow(user)
            {
                Owner = this
            };
            window.ShowDialog();
            LoadData();
        }

        private void ToggleStatus_Click(object sender, RoutedEventArgs e)
        {
            int userId = (int)((Button)sender).Tag;
            var user = UserStore.Repository.GetById(userId);

            if (user == null)
            {
                return;
            }

            bool newStatus = !user.IsActive;
            string performedBy = Session.CurrentUser?.Username ?? "Unknown";

            if (user.Id == Session.CurrentUser?.Id && !newStatus)
            {
                MessageBox.Show("You cannot disable your own account while signed in.", "Action Blocked", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!UserStore.Repository.SetActiveStatus(userId, newStatus, performedBy, out string error))
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            LoadData();
        }

        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            int userId = (int)((Button)sender).Tag;
            OpenUserDetails(userId);
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

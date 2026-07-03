using System.Windows;
using System.Windows.Controls;
using BarterPOS.Models;
using BarterPOS.Services;

namespace BarterPOS
{
    public partial class LoginWindow : Window
    {
        private bool isLoginMode = true;

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

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string validationError = ValidateRegistrationForm();

            if (!string.IsNullOrEmpty(validationError))
            {
                MessageBox.Show(validationError, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string role = ((ComboBoxItem)RegisterRole.SelectedItem).Content.ToString() ?? "Employee";

            var newUser = new User
            {
                EmployeeID = RegisterEmployeeID.Text.Trim(),
                FullName = RegisterFullName.Text.Trim(),
                Email = RegisterEmail.Text.Trim(),
                Username = RegisterUsername.Text.Trim(),
                Role = role
            };

            bool created = UserStore.Repository.Register(newUser, RegisterPassword.Password, out string error);

            if (!created)
            {
                MessageBox.Show(error, "Registration Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Account created successfully! You can now sign in.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            ClearRegistrationForm();
            SwitchToLogin();
        }

        private string ValidateRegistrationForm()
        {
            if (string.IsNullOrWhiteSpace(RegisterEmployeeID.Text))
            {
                return "Employee ID is required.";
            }

            if (string.IsNullOrWhiteSpace(RegisterFullName.Text))
            {
                return "Full Name is required.";
            }

            if (string.IsNullOrWhiteSpace(RegisterEmail.Text))
            {
                return "Email is required.";
            }

            if (!IsValidEmail(RegisterEmail.Text))
            {
                return "Please enter a valid email address.";
            }

            if (RegisterRole.SelectedIndex == 0 || RegisterRole.SelectedValue == null)
            {
                return "Please select a valid role.";
            }

            if (string.IsNullOrWhiteSpace(RegisterUsername.Text))
            {
                return "Username is required.";
            }

            if (RegisterUsername.Text.Length < 3)
            {
                return "Username must be at least 3 characters long.";
            }

            if (string.IsNullOrWhiteSpace(RegisterPassword.Password))
            {
                return "Password is required.";
            }

            if (RegisterPassword.Password.Length < 6)
            {
                return "Password must be at least 6 characters long.";
            }

            if (RegisterConfirmPassword.Password != RegisterPassword.Password)
            {
                return "Passwords do not match.";
            }

            if (!RegisterTermsCheckbox.IsChecked.HasValue || !RegisterTermsCheckbox.IsChecked.Value)
            {
                return "You must agree to the Terms and Conditions.";
            }

            return string.Empty;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void ClearRegistrationForm()
        {
            RegisterEmployeeID.Text = string.Empty;
            RegisterFullName.Text = string.Empty;
            RegisterEmail.Text = string.Empty;
            RegisterRole.SelectedIndex = 0;
            RegisterUsername.Text = string.Empty;
            RegisterPassword.Clear();
            RegisterConfirmPassword.Clear();
            RegisterTermsCheckbox.IsChecked = false;
        }

        private void Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (isLoginMode)
            {
                SwitchToRegister();
            }
            else
            {
                SwitchToLogin();
            }
        }

        private void SwitchToRegister()
        {
            isLoginMode = false;
            FormTitle.Text = "Create Account";
            FormSubtitle.Text = "Fill in your details";
            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;
            ToggleText.Text = "Already have an account?";
            ToggleButton.Content = "Sign In";
        }

        private void SwitchToLogin()
        {
            isLoginMode = true;
            FormTitle.Text = "Sign In";
            FormSubtitle.Text = "Enter your credentials";
            LoginPanel.Visibility = Visibility.Visible;
            RegisterPanel.Visibility = Visibility.Collapsed;
            ToggleText.Text = "Don't have an account?";
            ToggleButton.Content = "Sign Up";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

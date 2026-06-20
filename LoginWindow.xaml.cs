using System.Windows;

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
            // Demo mode - no authentication required
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            // Demo mode - account creation doesn't actually do anything
            MessageBox.Show("Account created successfully! You can now sign in.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            
            // Switch back to login
            SwitchToLogin();
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

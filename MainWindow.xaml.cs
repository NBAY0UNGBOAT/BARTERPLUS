using System;
using System.Windows;
using BarterPOS.ViewModels;

namespace BarterPOS
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            try
            {
                this.DataContext = new SalesViewModel();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting DataContext: {ex.Message}\n{ex.StackTrace}");
                // Continue anyway - UI will still display, just no data binding
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}

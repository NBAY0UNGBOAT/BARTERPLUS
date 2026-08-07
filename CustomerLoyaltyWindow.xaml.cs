using System.Globalization;
using System.Windows;
using BarterPOS.Models;
using BarterPOS.Services;

namespace BarterPOS
{
    public partial class CustomerLoyaltyWindow : Window
    {
        public Customer? SelectedCustomer { get; private set; }

        public CustomerLoyaltyWindow()
        {
            InitializeComponent();
            CustomerIdTextBox.Focus();
        }

        private void Validate_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(CustomerIdTextBox.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int customerId) || customerId <= 0)
            {
                ShowValidationError("Enter a valid numeric loyalty ID.");
                return;
            }

            try
            {
                Customer? customer = CustomerStore.Repository.GetById(customerId);
                if (!CustomerLoyaltyValidator.ValidateCustomer(customer, out string message))
                {
                    SelectedCustomer = null;
                    CustomerDetailsBorder.Visibility = Visibility.Collapsed;
                    ShowValidationError(message);
                    return;
                }

                SelectedCustomer = customer;
                CustomerNameText.Text = customer!.Name;
                CustomerTypeText.Text = $"Customer type: {customer.Type.Trim().ToUpperInvariant()}";
                CustomerPointsText.Text = $"Available loyalty points: {customer.Points:N0}";
                CustomerDetailsBorder.Visibility = Visibility.Visible;
            }
            catch (System.Exception ex)
            {
                SelectedCustomer = null;
                CustomerDetailsBorder.Visibility = Visibility.Collapsed;
                ShowValidationError($"Customer validation is unavailable: {ex.Message}");
            }
        }

        private void UseCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCustomer == null)
            {
                ShowValidationError("Validate a customer loyalty ID before continuing.");
                return;
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private static void ShowValidationError(string message)
        {
            MessageBox.Show(message, "Customer Loyalty", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}

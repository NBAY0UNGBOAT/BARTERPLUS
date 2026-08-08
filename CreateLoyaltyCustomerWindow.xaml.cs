using System.Windows;
using System.Windows.Controls;
using BarterPOS.Models;
using BarterPOS.Services;

namespace BarterPOS
{
    public partial class CreateLoyaltyCustomerWindow : Window
    {
        public Customer? CreatedCustomer { get; private set; }

        public CreateLoyaltyCustomerWindow()
        {
            InitializeComponent();
            CustomerNameTextBox.Focus();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            string name = CustomerNameTextBox.Text.Trim();
            string type = (CustomerTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "REGULAR";

            if (!InputValidator.IsValidPersonName(name))
            {
                MessageBox.Show(
                    "Please enter the customer name using at least 2 characters.",
                    "Create Loyalty Card",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                CustomerNameTextBox.Focus();
                return;
            }

            if (CustomerTypeComboBox.SelectedItem is not ComboBoxItem)
            {
                MessageBox.Show(
                    "Please select a customer type.",
                    "Create Loyalty Card",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                CustomerTypeComboBox.Focus();
                return;
            }

            var customer = new Customer
            {
                Name = name,
                Type = type,
                Points = 0m,
                CreditLimit = 0m,
                IsActive = true
            };

            if (!CustomerStore.Repository.Create(customer, out Customer? createdCustomer, out string error) || createdCustomer == null)
            {
                MessageBox.Show(
                    error,
                    "Create Loyalty Card",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            CreatedCustomer = createdCustomer;

            MessageBox.Show(
                $"Loyalty card created successfully.\nCustomer ID: {createdCustomer.Id}",
                "Create Loyalty Card",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

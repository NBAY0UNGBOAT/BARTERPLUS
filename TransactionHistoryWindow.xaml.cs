using System.Windows;
using BarterPOS.Services;
using BarterPOS.Models;

namespace BarterPOS
{
    public partial class TransactionHistoryWindow : Window
    {
        public TransactionHistoryWindow()
        {
            InitializeComponent();
            LoadTransactions();
        }

        private void LoadTransactions()
        {
            var transactions = TransactionRecordStore.GetTransactions();
            TransactionsGrid.ItemsSource = transactions;
        }

        private void VoidTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionsGrid.SelectedItem is not SaleTransaction transaction)
            {
                MessageBox.Show(
                    "Please select a transaction first.",
                    "Void Transaction",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var authWindow = new AdminAuthorizationWindow
            {
                Owner = this
            };

            if (authWindow.ShowDialog() != true)
            {
                return;
            }

            if (transaction.Status == TransactionStatus.Voided)
            {
                MessageBox.Show(
                    "This transaction has already been voided.",
                    "Void Transaction",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Void Transaction #{transaction.TransactionId}?",
                "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            transaction.Status = TransactionStatus.Voided;

            TransactionRecordStore.UpdateTransaction(transaction);

            LoadTransactions();

            MessageBox.Show(
                "Transaction successfully voided.",
                "Void Transaction",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ProcessRefund_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionsGrid.SelectedItem is not SaleTransaction transaction)
            {
                MessageBox.Show(
                    "Please select a transaction first.",
                    "Refund",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (transaction.Status == TransactionStatus.Refunded)
            {
                MessageBox.Show(
                    "This transaction has already been fully refunded.",
                    "Refund",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            var authWindow = new AdminAuthorizationWindow
            {
                Owner = this
            };

            if (authWindow.ShowDialog() != true)
            {
                return;
            }

            var refundWindow = new RefundWindow(transaction)
            {
                Owner = this
            };

            if (refundWindow.ShowDialog() == true)
            {
                LoadTransactions();

                MessageBox.Show(
                    "Refund processed successfully.",
                    "Refund",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

    }
}
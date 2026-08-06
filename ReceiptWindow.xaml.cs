using System.Windows;
using BarterPOS.Models;
using BarterPOS.Services;

namespace BarterPOS
{
    public partial class ReceiptWindow : Window
    {
        private readonly SaleTransaction _transaction;
        private readonly string _receiptText;

        public ReceiptWindow(SaleTransaction transaction)
        {
            InitializeComponent();

            _transaction = transaction;
            _receiptText = ReceiptService.BuildReceiptText(transaction);
            ReceiptTextBox.Text = _receiptText;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            bool saved = ReceiptService.TrySaveToFile(_receiptText, _transaction.TransactionId, out string message);
            MessageBox.Show(
                message,
                "Save Receipt",
                MessageBoxButton.OK,
                saved ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
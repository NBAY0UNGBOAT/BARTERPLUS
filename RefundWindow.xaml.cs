using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BarterPOS.Models;
using BarterPOS.Services;

namespace BarterPOS
{
    public partial class RefundWindow : Window
    {
        private readonly SaleTransaction _transaction;

        public List<RefundItem> RefundItems { get; } = new();

        public RefundWindow(SaleTransaction transaction)
        {
            InitializeComponent();

            _transaction = transaction;

            foreach (var item in transaction.Items)
            {
                bool alreadyRefunded = transaction.RefundedItems
                    .Any(r => r.Code == item.Code);

                if (alreadyRefunded)
                {
                    continue;
                }

                RefundItems.Add(new RefundItem
                {
                    SaleItem = item
                });
            }

            RefundGrid.ItemsSource = RefundItems;

            if (RefundItems.Count == 0)
            {
                MessageBox.Show(
                    "All items in this transaction have already been refunded.",
                    "Refund",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = false;
            }
        }

        private void Refund_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = RefundItems
                .Where(r => r.IsSelected)
                .Select(r => r.SaleItem)
                .ToList();

            if (selectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Please select at least one item to refund.",
                    "Refund",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            bool success = TransactionRecordStore.RefundItems(
                _transaction,
                selectedItems);

            if (!success)
            {
                MessageBox.Show(
                    "Unable to process the refund.",
                    "Refund",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
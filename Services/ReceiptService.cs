using System;
using System.Text;
using BarterPOS.Models;
using Microsoft.Win32;

namespace BarterPOS.Services
{
    public static class ReceiptService
    {
        private const int ReceiptWidth = 42;

        public static string BuildReceiptText(SaleTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            StoreInfo store = AppConfig.GetStoreInfo();
            var sb = new StringBuilder();

            AppendCentered(sb, store.Name);
            if (!string.IsNullOrWhiteSpace(store.AddressLine1))
            {
                AppendCentered(sb, store.AddressLine1);
            }
            if (!string.IsNullOrWhiteSpace(store.AddressLine2))
            {
                AppendCentered(sb, store.AddressLine2);
            }
            if (!string.IsNullOrWhiteSpace(store.Phone))
            {
                AppendCentered(sb, store.Phone);
            }

            AppendDivider(sb);

            sb.AppendLine($"Receipt #:  {transaction.TransactionId}");
            sb.AppendLine($"Date:       {transaction.CompletedAt:MMM dd, yyyy hh:mm tt}");
            sb.AppendLine($"Terminal:   {transaction.TerminalId}");
            sb.AppendLine($"Cashier:    {transaction.Cashier}");
            if (!string.IsNullOrWhiteSpace(transaction.CustomerName))
            {
                sb.AppendLine($"Customer:   {transaction.CustomerName}");
                sb.AppendLine($"Member ID:  {transaction.CustomerId}");
                sb.AppendLine($"Type:       {transaction.CustomerType}");
            }

            AppendDivider(sb);

            foreach (SaleLineItem item in transaction.Items)
            {
                AppendLineItem(sb, item);
            }

            AppendDivider(sb);

            AppendTotalLine(sb, "Items", transaction.TotalItems.ToString());
            AppendTotalLine(sb, "Subtotal", transaction.GrossAmount.ToString("C"));

            if (transaction.PercentageDiscount > 0)
            {
                AppendTotalLine(sb, "Discount", $"-{transaction.PercentageDiscount:C}");
            }

            if (transaction.ManualDeduction > 0)
            {
                AppendTotalLine(sb, "Deduction", $"-{transaction.ManualDeduction:C}");
            }

            AppendTotalLine(sb, "TOTAL", transaction.NetAmount.ToString("C"));
            AppendTotalLine(sb, "Payment", transaction.PaymentMethod);
            AppendTotalLine(sb, "Amount Paid", transaction.AmountPaid.ToString("C"));

            if (transaction.ChangeDue > 0)
            {
                AppendTotalLine(sb, "Change", transaction.ChangeDue.ToString("C"));
            }

            AppendDivider(sb);

            if (!string.IsNullOrWhiteSpace(store.ReceiptFooter))
            {
                AppendCentered(sb, store.ReceiptFooter);
            }

            AppendCentered(sb, transaction.IsSynced ? "Synced" : "Saved offline - pending sync");

            return sb.ToString();
        }

        public static bool TrySaveToFile(string receiptText, int transactionId, out string message)
        {
            var dialog = new SaveFileDialog
            {
                FileName = $"Receipt_{transactionId}.txt",
                Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = ".txt"
            };

            if (dialog.ShowDialog() != true)
            {
                message = "Save cancelled.";
                return false;
            }

            try
            {
                System.IO.File.WriteAllText(dialog.FileName, receiptText);
                message = $"Receipt saved to {dialog.FileName}";
                return true;
            }
            catch (Exception ex)
            {
                message = $"Could not save receipt: {ex.Message}";
                return false;
            }
        }

        private static void AppendLineItem(StringBuilder sb, SaleLineItem item)
        {
            sb.AppendLine(Truncate(item.Name, ReceiptWidth));

            string qtyPrice = $"  {item.Quantity} x {item.UnitPrice:C}";
            string subtotal = item.Subtotal.ToString("C");
            sb.AppendLine(PadLeftRight(qtyPrice, subtotal));
        }

        private static void AppendTotalLine(StringBuilder sb, string label, string value)
        {
            sb.AppendLine(PadLeftRight(label, value));
        }

        private static void AppendDivider(StringBuilder sb)
        {
            sb.AppendLine(new string('-', ReceiptWidth));
        }

        private static void AppendCentered(StringBuilder sb, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            text = Truncate(text, ReceiptWidth);
            int padding = Math.Max(0, (ReceiptWidth - text.Length) / 2);
            sb.AppendLine(new string(' ', padding) + text);
        }

        private static string PadLeftRight(string left, string right)
        {
            int spaceForRight = Math.Max(1, ReceiptWidth - left.Length);
            return left + right.PadLeft(spaceForRight);
        }

        private static string Truncate(string text, int maxLength)
        {
            return text.Length <= maxLength ? text : text.Substring(0, maxLength);
        }
    }
}

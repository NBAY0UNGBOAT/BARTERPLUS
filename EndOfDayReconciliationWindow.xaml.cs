using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using BarterPOS.Models;
using BarterPOS.Services;

namespace BarterPOS
{
    public partial class EndOfDayReconciliationWindow : Window
    {
        private static readonly Brush BalancedBrush = new SolidColorBrush(Color.FromRgb(0x05, 0x96, 0x69));
        private static readonly Brush VarianceBrush = new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26));

        private EndOfDayReconciliation _report = null!;

        public EndOfDayReconciliationWindow()
        {
            InitializeComponent();
            BusinessDatePicker.SelectedDate = DateTime.Today;
            RefreshReport();
        }

        private void BusinessDatePicker_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RefreshReport();
        }

        private void CountedCashText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            RefreshReport();
        }

        private void RefreshReport()
        {
            DateTime businessDate = BusinessDatePicker.SelectedDate ?? DateTime.Today;
            decimal countedCash = ParseCountedCash();

            _report = EndOfDayReconciliationService.BuildReport(businessDate, countedCash);

            var existing = EndOfDayReconciliationStore.GetReportForDate(businessDate);
            AlreadyReconciledText.Visibility = existing != null ? Visibility.Visible : Visibility.Collapsed;

            RenderReport();
        }

        private decimal ParseCountedCash()
        {
            return decimal.TryParse(CountedCashText.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal amount)
                ? Math.Max(0m, amount)
                : 0m;
        }

        private void RenderReport()
        {
            TransactionCountText.Text = _report.TransactionCount.ToString();
            VoidedCountText.Text = _report.VoidedCount.ToString();
            RefundedCountText.Text = _report.RefundedCount.ToString();
            GrossSalesText.Text = _report.GrossSales.ToString("C");
            DiscountsText.Text = _report.TotalDiscounts.ToString("C");
            NetSalesText.Text = _report.NetSales.ToString("C");

            PaymentMethodGrid.ItemsSource = _report.PaymentMethodBreakdown;

            CashSalesText.Text = _report.CashSalesTotal.ToString("C");
            CashInText.Text = _report.CashInTotal.ToString("C");
            CashOutText.Text = _report.CashOutTotal.ToString("C");
            CashRefundsText.Text = _report.CashRefundsTotal.ToString("C");
            ExpectedCashText.Text = _report.ExpectedCash.ToString("C");

            VarianceText.Text = $"{_report.Variance:C} ({_report.VarianceLabel})";
            VarianceText.Foreground = _report.IsBalanced ? BalancedBrush : VarianceBrush;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var existing = EndOfDayReconciliationStore.GetReportForDate(_report.BusinessDate);

            if (existing != null)
            {
                MessageBoxResult confirm = MessageBox.Show(
                    $"A reconciliation for {_report.BusinessDate:MMMM dd, yyyy} already exists. Overwrite it?",
                    "Confirm",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes)
                {
                    return;
                }

                _report.Id = existing.Id;
            }

            _report.Notes = NotesText.Text.Trim();

            if (!_report.IsBalanced)
            {
                MessageBoxResult proceed = MessageBox.Show(
                    $"The drawer is {_report.VarianceLabel.ToLower()} by {Math.Abs(_report.Variance):C}. Save anyway?",
                    "Cash Variance Detected",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            SaveSyncResult result = EndOfDayReconciliationStore.Save(_report);

            MessageBox.Show(
                result.Message,
                "End-of-Day Reconciliation",
                MessageBoxButton.OK,
                result.IsSynced ? MessageBoxImage.Information : MessageBoxImage.Warning);

            RefreshReport();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
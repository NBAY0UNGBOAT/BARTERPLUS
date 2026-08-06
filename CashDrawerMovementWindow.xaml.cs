using System.Globalization;
using System.Windows;

namespace BarterPOS
{
    public partial class CashDrawerMovementWindow : Window
    {
        public decimal Amount { get; private set; }
        public string Note { get; private set; } = string.Empty;

        public CashDrawerMovementWindow(string movementType)
        {
            InitializeComponent();
            MovementTitle.Text = movementType;
            AmountText.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(AmountText.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal amount) || amount <= 0m)
            {
                MessageBox.Show("Enter a valid amount greater than zero.", "Cash Drawer", MessageBoxButton.OK, MessageBoxImage.Warning);
                AmountText.Focus();
                return;
            }

            Amount = amount;
            Note = NoteText.Text.Trim();
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

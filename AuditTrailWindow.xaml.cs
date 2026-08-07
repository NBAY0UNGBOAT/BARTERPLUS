using System.Windows;
using BarterPOS.Services;

namespace BarterPOS
{
    public partial class AuditTrailWindow : Window
    {
        public AuditTrailWindow()
        {
            InitializeComponent();
            LoadEntries();
        }

        private void LoadEntries()
        {
            AuditGrid.ItemsSource = AuditTrailStore.GetEntries();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadEntries();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
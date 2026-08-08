using System.Windows;
using System.Windows.Input;
using BarterPOS.Models;
using BarterPOS.Services;

namespace BarterPOS
{
    public partial class ProductSearchWindow : Window
    {
        public Product? SelectedProduct { get; private set; }

        public ProductSearchWindow()
        {
            InitializeComponent();
            LoadProducts(string.Empty);
            SearchTextBox.Focus();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts(SearchTextBox.Text);
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            UseSelectedProduct();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ProductsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            UseSelectedProduct();
        }

        private void LoadProducts(string query)
        {
            ProductsGrid.ItemsSource = ProductStore.Repository.Search(query);
        }

        private void UseSelectedProduct()
        {
            if (ProductsGrid.SelectedItem is not Product product)
            {
                MessageBox.Show(
                    "Select an item first.",
                    "Item Search",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            SelectedProduct = product;
            DialogResult = true;
        }
    }
}

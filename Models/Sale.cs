using System;
using System.Collections.ObjectModel;

namespace BarterPOS.Models
{
    public class Sale
    {
        public int TransactionId { get; set; }
        public string TerminalId { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public Customer? Customer { get; set; }
        public string Cashier { get; set; } = string.Empty;
        public string Bagger { get; set; } = string.Empty;
        public ObservableCollection<Product> Products { get; set; } = new();
        public decimal TotalAmount => CalculateTotal();
        public decimal TotalSavings { get; set; }

        private decimal CalculateTotal()
        {
            decimal total = 0;
            foreach (var product in Products)
            {
                total += product.Subtotal;
            }
            return total;
        }
    }
}

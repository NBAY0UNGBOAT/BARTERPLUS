using BarterPOS.Models;

namespace BarterPOS.Models
{
    public class RefundItem
    {
        public bool IsSelected { get; set; }

        public SaleLineItem SaleItem { get; set; } = new();

        public string Code => SaleItem.Code;

        public string Name => SaleItem.Name;

        public int Quantity => SaleItem.Quantity;

        public decimal UnitPrice => SaleItem.UnitPrice;

        public decimal Subtotal => SaleItem.Subtotal;
    }
}
using BarterPOS.Models;

namespace BarterPOS.Services
{
    public class NullProductRepository : IProductRepository
    {
        public Product? GetByBarcode(string barcode) => null;
    }
}

using BarterPOS.Models;
using System.Collections.Generic;

namespace BarterPOS.Services
{
    public class NullProductRepository : IProductRepository
    {
        public Product? GetByBarcode(string barcode) => null;
        public List<Product> Search(string query) => new();
    }
}

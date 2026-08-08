using BarterPOS.Models;
using System.Collections.Generic;

namespace BarterPOS.Services
{
    public interface IProductRepository
    {
        Product? GetByBarcode(string barcode);
        List<Product> Search(string query);
    }
}

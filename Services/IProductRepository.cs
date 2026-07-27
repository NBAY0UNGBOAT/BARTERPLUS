using BarterPOS.Models;

namespace BarterPOS.Services
{
    public interface IProductRepository
    {
        Product? GetByBarcode(string barcode);
    }
}

using BarterPOS.Models;

namespace BarterPOS.Services
{
    public interface ICustomerRepository
    {
        Customer? GetById(int customerId);
    }
}

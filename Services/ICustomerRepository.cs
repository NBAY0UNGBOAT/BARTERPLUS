using BarterPOS.Models;

namespace BarterPOS.Services
{
    public interface ICustomerRepository
    {
        Customer? GetById(int customerId);
        bool Create(Customer customer, out Customer? createdCustomer, out string error);
        bool Update(Customer customer, out string error);
    }
}

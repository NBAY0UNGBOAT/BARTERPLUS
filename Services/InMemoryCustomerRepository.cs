using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BarterPOS.Models;

namespace BarterPOS.Services
{
    public class InMemoryCustomerRepository : ICustomerRepository
    {
        private readonly List<Customer> _customers;

        public InMemoryCustomerRepository()
        {
            _customers = LoadSeedCustomers();
        }

        public Customer? GetById(int customerId)
        {
            Customer? customer = _customers.FirstOrDefault(c => c.Id == customerId);
            return customer == null ? null : Copy(customer);
        }

        private static List<Customer> LoadSeedCustomers()
        {
            try
            {
                string seedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "customers.seed.json");
                if (!File.Exists(seedPath))
                {
                    return new List<Customer>();
                }

                string json = File.ReadAllText(seedPath);
                return JsonSerializer.Deserialize<List<Customer>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Customer>();
            }
            catch
            {
                return new List<Customer>();
            }
        }

        internal static Customer Copy(Customer customer) => new()
        {
            Id = customer.Id,
            Name = customer.Name,
            Type = customer.Type,
            Points = customer.Points,
            CreditLimit = customer.CreditLimit,
            IsActive = customer.IsActive
        };
    }
}

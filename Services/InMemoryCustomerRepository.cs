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
        private const int FirstCustomerId = 100001;
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

        public bool Create(Customer customer, out Customer? createdCustomer, out string error)
        {
            error = string.Empty;
            createdCustomer = null;

            if (customer == null)
            {
                error = "Customer details are required.";
                return false;
            }

            string name = customer.Name.Trim();
            string type = customer.Type.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(name))
            {
                error = "Customer name is required.";
                return false;
            }

            if (type is not "REGULAR" and not "PWD" and not "SENIOR")
            {
                error = "Customer type must be Regular, PWD, or Senior.";
                return false;
            }

            var newCustomer = new Customer
            {
                Id = GetNextCustomerId(),
                Name = name,
                Type = type,
                Points = Math.Max(0m, customer.Points),
                CreditLimit = Math.Max(0m, customer.CreditLimit),
                IsActive = true
            };

            _customers.Add(newCustomer);
            createdCustomer = Copy(newCustomer);
            return true;
        }

        public bool Update(Customer customer, out string error)
        {
            error = string.Empty;

            if (customer == null)
            {
                error = "Customer details are required.";
                return false;
            }

            int index = _customers.FindIndex(c => c.Id == customer.Id);
            if (index < 0)
            {
                error = "Customer not found.";
                return false;
            }

            _customers[index] = Copy(customer);
            return true;
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

        private int GetNextCustomerId()
        {
            int currentMax = _customers
                .Select(c => c.Id)
                .DefaultIfEmpty(FirstCustomerId - 1)
                .Max();

            return Math.Max(FirstCustomerId, currentMax + 1);
        }
    }
}

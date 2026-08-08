using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BarterPOS.Models;
using MongoDB.Driver;

namespace BarterPOS.Services
{
    public class MongoCustomerRepository : ICustomerRepository
    {
        private const int FirstCustomerId = 100001;
        private readonly IMongoCollection<Customer> _customers;
        private readonly IMongoCollection<MongoCounter> _counters;

        public MongoCustomerRepository(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _customers = database.GetCollection<Customer>("customers");
            _counters = database.GetCollection<MongoCounter>("counters");

            EnsureIndexes();
            SeedCustomersIfEmpty();
            EnsureCounters();
        }

        public Customer? GetById(int customerId)
        {
            Customer? customer = _customers.Find(c => c.Id == customerId).FirstOrDefault();
            return customer == null ? null : InMemoryCustomerRepository.Copy(customer);
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
                Id = GetNextId("customers"),
                Name = name,
                Type = type,
                Points = Math.Max(0m, customer.Points),
                CreditLimit = Math.Max(0m, customer.CreditLimit),
                IsActive = true
            };

            try
            {
                _customers.InsertOne(newCustomer);
                createdCustomer = InMemoryCustomerRepository.Copy(newCustomer);
                return true;
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                error = "A customer with that loyalty ID already exists.";
                return false;
            }
        }

        public bool Update(Customer customer, out string error)
        {
            error = string.Empty;

            if (customer == null)
            {
                error = "Customer details are required.";
                return false;
            }

            try
            {
                ReplaceOneResult result = _customers.ReplaceOne(
                    c => c.Id == customer.Id,
                    InMemoryCustomerRepository.Copy(customer));

                if (result.MatchedCount == 0)
                {
                    error = "Customer not found.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private void EnsureIndexes()
        {
            var idIndex = new CreateIndexModel<Customer>(
                Builders<Customer>.IndexKeys.Ascending(c => c.Id),
                new CreateIndexOptions { Unique = true });

            _customers.Indexes.CreateOne(idIndex);
        }

        private void SeedCustomersIfEmpty()
        {
            if (_customers.EstimatedDocumentCount() > 0)
            {
                return;
            }

            string seedPath = Path.Combine(AppContext.BaseDirectory, "Data", "customers.seed.json");
            if (!File.Exists(seedPath))
            {
                return;
            }

            string json = File.ReadAllText(seedPath);
            List<Customer>? customers = JsonSerializer.Deserialize<List<Customer>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (customers is { Count: > 0 })
            {
                _customers.InsertMany(customers.Select(InMemoryCustomerRepository.Copy));
            }
        }

        private void EnsureCounters()
        {
            EnsureCounterAtLeast("customers", GetMaxCustomerId());
        }

        private int GetNextId(string name)
        {
            var updatedCounter = _counters.FindOneAndUpdate(
                c => c.Id == name,
                Builders<MongoCounter>.Update.Inc(c => c.Value, 1),
                new FindOneAndUpdateOptions<MongoCounter>
                {
                    IsUpsert = true,
                    ReturnDocument = ReturnDocument.After
                });

            return updatedCounter.Value;
        }

        private void EnsureCounterAtLeast(string name, int minimumValue)
        {
            _counters.UpdateOne(
                c => c.Id == name,
                Builders<MongoCounter>.Update.Max(c => c.Value, minimumValue),
                new UpdateOptions { IsUpsert = true });
        }

        private int GetMaxCustomerId()
        {
            return _customers.Find(FilterDefinition<Customer>.Empty)
                .SortByDescending(c => c.Id)
                .Limit(1)
                .ToList()
                .FirstOrDefault()?.Id ?? (FirstCustomerId - 1);
        }
    }
}

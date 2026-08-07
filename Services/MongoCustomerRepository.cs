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
        private readonly IMongoCollection<Customer> _customers;

        public MongoCustomerRepository(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _customers = database.GetCollection<Customer>("customers");

            EnsureIndexes();
            SeedCustomersIfEmpty();
        }

        public Customer? GetById(int customerId)
        {
            Customer? customer = _customers.Find(c => c.Id == customerId).FirstOrDefault();
            return customer == null ? null : InMemoryCustomerRepository.Copy(customer);
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
    }
}

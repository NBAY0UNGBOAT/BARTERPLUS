using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BarterPOS.Models;
using MongoDB.Driver;

namespace BarterPOS.Services
{
    public class MongoProductRepository : IProductRepository
    {
        private readonly IMongoCollection<Product> _products;

        public MongoProductRepository(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);

            _products = database.GetCollection<Product>("products");

            EnsureIndexes();
            SeedProductsIfEmpty();
        }

        public Product? GetByBarcode(string barcode)
        {
            string normalizedBarcode = barcode.Trim();
            var product = _products.Find(p => p.Code == normalizedBarcode).FirstOrDefault();

            if (product == null)
            {
                return null;
            }

            return new Product
            {
                Id = product.Id,
                Code = product.Code,
                Name = product.Name,
                Price = product.Price,
                Quantity = 1
            };
        }

        private void EnsureIndexes()
        {
            var barcodeIndex = new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Code),
                new CreateIndexOptions { Unique = true });

            _products.Indexes.CreateOne(barcodeIndex);
        }

        private void SeedProductsIfEmpty()
        {
            if (_products.EstimatedDocumentCount() > 0)
            {
                return;
            }

            string seedPath = GetSeedPath();

            if (!File.Exists(seedPath))
            {
                return;
            }

            string json = File.ReadAllText(seedPath);
            var products = JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (products == null || products.Count == 0)
            {
                return;
            }

            _products.InsertMany(products.Select(p => new Product
            {
                Id = p.Id,
                Code = p.Code.Trim(),
                Name = p.Name.Trim(),
                Price = p.Price,
                Quantity = 1
            }));
        }

        private static string GetSeedPath()
        {
            string outputPath = Path.Combine(AppContext.BaseDirectory, "Data", "products.seed.json");

            if (File.Exists(outputPath))
            {
                return outputPath;
            }

            return Path.Combine(Environment.CurrentDirectory, "Data", "products.seed.json");
        }
    }
}

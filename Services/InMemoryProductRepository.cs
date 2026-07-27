using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BarterPOS.Models;

namespace BarterPOS.Services
{
    public class InMemoryProductRepository : IProductRepository
    {
        private readonly List<Product> _products;

        public InMemoryProductRepository()
        {
            _products = LoadSeedProducts();
        }

        public Product? GetByBarcode(string barcode)
        {
            var match = _products.FirstOrDefault(p => p.Code == barcode.Trim());
            if (match == null)
            {
                return null;
            }

            return new Product
            {
                Id = match.Id,
                Code = match.Code,
                Name = match.Name,
                Price = match.Price,
                Quantity = 1
            };
        }

        private static List<Product> LoadSeedProducts()
        {
            try
            {
                string seedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "products.seed.json");
                if (!File.Exists(seedPath))
                {
                    return new List<Product>();
                }

                string json = File.ReadAllText(seedPath);
                var products = JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return products ?? new List<Product>();
            }
            catch
            {
                return new List<Product>();
            }
        }
    }
}

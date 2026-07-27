using System;

namespace BarterPOS.Services
{
    public static class ProductStore
    {
        public static IProductRepository Repository { get; } = CreateRepository();

        private static IProductRepository CreateRepository()
        {
            string? connectionString = Environment.GetEnvironmentVariable("BARTERPLUS_MONGO_CONNECTION");
            string databaseName = Environment.GetEnvironmentVariable("BARTERPLUS_MONGO_DATABASE") ?? "BARTERPLUS";

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return new NullProductRepository();
            }

            return new MongoProductRepository(connectionString, databaseName);
        }
    }
}

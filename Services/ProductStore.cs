namespace BarterPOS.Services
{
    public static class ProductStore
    {
        public static IProductRepository Repository { get; } = CreateRepository();

        private static IProductRepository CreateRepository()
        {
            MongoSettings mongoSettings = AppConfig.GetMongoSettings();

            if (string.IsNullOrWhiteSpace(mongoSettings.ConnectionString))
            {
                return new InMemoryProductRepository();
            }

            return new MongoProductRepository(mongoSettings.ConnectionString, mongoSettings.DatabaseName);
        }
    }
}

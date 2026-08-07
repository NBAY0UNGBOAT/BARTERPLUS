namespace BarterPOS.Services
{
    public static class CustomerStore
    {
        public static ICustomerRepository Repository { get; } = CreateRepository();

        private static ICustomerRepository CreateRepository()
        {
            MongoSettings mongoSettings = AppConfig.GetMongoSettings();

            if (string.IsNullOrWhiteSpace(mongoSettings.ConnectionString))
            {
                return new InMemoryCustomerRepository();
            }

            return new MongoCustomerRepository(mongoSettings.ConnectionString, mongoSettings.DatabaseName);
        }
    }
}

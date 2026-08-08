namespace BarterPOS.Services
{
    public static class CustomerStore
    {
        public static ICustomerRepository Repository { get; } = CreateRepository();

        private static ICustomerRepository CreateRepository()
        {
            try
            {
                MongoSettings mongoSettings = AppConfig.GetMongoSettings();

                if (string.IsNullOrWhiteSpace(mongoSettings.ConnectionString))
                {
                    return new InMemoryCustomerRepository();
                }

                return new MongoCustomerRepository(mongoSettings.ConnectionString, mongoSettings.DatabaseName);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Customer repository fallback: {ex.Message}");
                return new InMemoryCustomerRepository();
            }
        }
    }
}

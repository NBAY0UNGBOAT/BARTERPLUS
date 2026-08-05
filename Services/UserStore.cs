namespace BarterPOS.Services
{
    // Uses MongoDB when configured; otherwise keeps local dev easy.
    public static class UserStore
    {
        public static IUserRepository Repository { get; } = CreateRepository();

        private static IUserRepository CreateRepository()
        {
            MongoSettings mongoSettings = AppConfig.GetMongoSettings();

            if (string.IsNullOrWhiteSpace(mongoSettings.ConnectionString))
            {
                return new InMemoryUserRepository();
            }

            return new MongoUserRepository(mongoSettings.ConnectionString, mongoSettings.DatabaseName);
        }
    }
}

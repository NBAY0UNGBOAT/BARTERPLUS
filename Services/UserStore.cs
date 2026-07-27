using System;

namespace BarterPOS.Services
{
    // Uses MongoDB when BARTERPLUS_MONGO_CONNECTION is set; otherwise keeps local dev easy.
    public static class UserStore
    {
        public static IUserRepository Repository { get; } = CreateRepository();

        private static IUserRepository CreateRepository()
        {
            string? connectionString = Environment.GetEnvironmentVariable("BARTERPLUS_MONGO_CONNECTION");
            string databaseName = Environment.GetEnvironmentVariable("BARTERPLUS_MONGO_DATABASE") ?? "BARTERPLUS";

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return new InMemoryUserRepository();
            }

            return new MongoUserRepository(connectionString, databaseName);
        }
    }
}

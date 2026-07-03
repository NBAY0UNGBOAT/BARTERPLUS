namespace BarterPOS.Services
{
    // Single shared instance for the whole app.
    // When SCRUM-51 (DB) is done, change this one line to a SqlUserRepository
    // and every screen that uses UserStore.Repository keeps working unchanged.
    public static class UserStore
    {
        public static IUserRepository Repository { get; } = new InMemoryUserRepository();
    }
}

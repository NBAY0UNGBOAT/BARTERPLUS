using BarterPOS.Models;

namespace BarterPOS.Services
{
    public static class Session
    {
        public static User? CurrentUser { get; set; }
    }
}

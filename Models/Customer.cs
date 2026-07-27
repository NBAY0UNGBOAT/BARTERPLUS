namespace BarterPOS.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // SENIOR, PWD, REGULAR
        public decimal Points { get; set; }
        public decimal CreditLimit { get; set; }
    }
}

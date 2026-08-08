using System.ComponentModel;
using System.Runtime.CompilerServices;
using MongoDB.Bson.Serialization.Attributes;

namespace BarterPOS.Models
{
    public class Product : INotifyPropertyChanged
    {
        private int _quantity;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity == value)
                {
                    return;
                }

                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        [BsonIgnore]
        public decimal Subtotal => Price * Quantity;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

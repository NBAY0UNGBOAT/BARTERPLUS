using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using BarterPOS.Models;

namespace BarterPOS.ViewModels
{
    public class SalesViewModel : INotifyPropertyChanged
    {
        private Sale _currentSale = null!;
        private string _barcodeInput = string.Empty;
        private string _currentDateTime = string.Empty;
        private DispatcherTimer? _dateTimeTimer;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Sale CurrentSale
        {
            get => _currentSale;
            set
            {
                if (_currentSale != value)
                {
                    _currentSale = value;
                    OnPropertyChanged();
                }
            }
        }

        public string BarcodeInput
        {
            get => _barcodeInput;
            set
            {
                if (_barcodeInput != value)
                {
                    _barcodeInput = value;
                    OnPropertyChanged();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        ProcessBarcode(value);
                    }
                }
            }
        }

        public string CurrentDateTime
        {
            get => _currentDateTime;
            set
            {
                if (_currentDateTime != value)
                {
                    _currentDateTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public SalesViewModel()
        {
            try
            {
                // Initialize with sample data
                _currentSale = new Sale
                {
                    TransactionId = 42,
                    TerminalId = "000C09",
                    TransactionDate = DateTime.Now,
                    Cashier = "Julius Tarrayo",
                    Bagger = string.Empty,
                    TotalSavings = 0.00m
                };

                // Add sample product
                _currentSale.Products.Add(new Product
                {
                    Id = 1,
                    Code = "BON001",
                    Name = "BON INFANT MILK BOX 2KG N",
                    Price = 1047.50m,
                    Quantity = 1
                });

                // Initialize date/time timer
                InitializeDateTimeTimer();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SalesViewModel constructor: {ex.Message}");
                // Provide a fallback
                _currentSale = new Sale();
                CurrentDateTime = DateTime.Now.ToString("MMMM dd, yyyy | hh:mm:ss tt");
            }
        }

        private void InitializeDateTimeTimer()
        {
            try
            {
                _dateTimeTimer = new DispatcherTimer();
                _dateTimeTimer.Interval = TimeSpan.FromSeconds(1);
                _dateTimeTimer.Tick += (s, e) =>
                {
                    CurrentDateTime = DateTime.Now.ToString("MMMM dd, yyyy | hh:mm:ss tt");
                };
                _dateTimeTimer.Start();
                // Set initial value
                CurrentDateTime = DateTime.Now.ToString("MMMM dd, yyyy | hh:mm:ss tt");
            }
            catch (Exception ex)
            {
                CurrentDateTime = DateTime.Now.ToString("MMMM dd, yyyy | hh:mm:ss tt");
                System.Diagnostics.Debug.WriteLine($"Error initializing timer: {ex.Message}");
            }
        }

        private void ProcessBarcode(string barcode)
        {
            // TODO: Implement barcode processing logic
            // This would typically:
            // 1. Look up the product in the database
            // 2. Add it to the current sale
            // 3. Clear the barcode input
            
            BarcodeInput = string.Empty;
        }

        public void AddProduct(Product product)
        {
            var existingProduct = _currentSale.Products.FirstOrDefault(p => p.Code == product.Code);
            if (existingProduct != null)
            {
                existingProduct.Quantity += product.Quantity;
            }
            else
            {
                _currentSale.Products.Add(product);
            }
            OnPropertyChanged(nameof(CurrentSale));
        }

        public void RemoveProduct(Product product)
        {
            _currentSale.Products.Remove(product);
            OnPropertyChanged(nameof(CurrentSale));
        }

        public void ClearSale()
        {
            _currentSale = new Sale
            {
                TransactionId = _currentSale.TransactionId + 1,
                TerminalId = _currentSale.TerminalId,
                TransactionDate = DateTime.Now,
            };
            OnPropertyChanged(nameof(CurrentSale));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

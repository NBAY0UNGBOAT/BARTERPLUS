using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using BarterPOS.Models;
using BarterPOS.Services;

namespace BarterPOS.ViewModels
{
    public class SalesViewModel : INotifyPropertyChanged
    {
        private const decimal SeniorPwdDiscountRate = 0.20m;

        private Sale _currentSale = null!;
        private string _barcodeInput = string.Empty;
        private string _currentDateTime = string.Empty;
        private string _deductionInput = string.Empty;
        private bool _isPwdDiscount;
        private bool _isSeniorDiscount;
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
                    RefreshTotals();
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

        public string CashierName =>
            Session.CurrentUser?.FullName is { Length: > 0 } fullName
                ? fullName
                : Session.CurrentUser?.Username ?? "Cashier";

        public string CashierRole => Session.CurrentUser?.Role ?? "Cashier";

        public bool IsPwdDiscount
        {
            get => _isPwdDiscount;
            set
            {
                if (_isPwdDiscount == value)
                {
                    return;
                }

                _isPwdDiscount = value;
                if (value)
                {
                    _isSeniorDiscount = false;
                    OnPropertyChanged(nameof(IsSeniorDiscount));
                }

                OnPropertyChanged();
                RefreshTotals();
            }
        }

        public bool IsSeniorDiscount
        {
            get => _isSeniorDiscount;
            set
            {
                if (_isSeniorDiscount == value)
                {
                    return;
                }

                _isSeniorDiscount = value;
                if (value)
                {
                    _isPwdDiscount = false;
                    OnPropertyChanged(nameof(IsPwdDiscount));
                }

                OnPropertyChanged();
                RefreshTotals();
            }
        }

        public string DeductionInput
        {
            get => _deductionInput;
            set
            {
                if (_deductionInput != value)
                {
                    _deductionInput = value;
                    OnPropertyChanged();
                    RefreshTotals();
                }
            }
        }

        public decimal TotalAmount => _currentSale?.TotalAmount ?? 0m;

        public int TotalItems => _currentSale?.Products.Sum(p => p.Quantity) ?? 0;

        public decimal PercentageDiscount =>
            (IsPwdDiscount || IsSeniorDiscount) ? TotalAmount * SeniorPwdDiscountRate : 0m;

        public decimal ManualDeduction
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_deductionInput))
                {
                    return 0m;
                }

                return decimal.TryParse(_deductionInput, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal amount)
                    ? Math.Max(0m, amount)
                    : 0m;
            }
        }

        public decimal AmountDue => Math.Max(0m, TotalAmount - PercentageDiscount - ManualDeduction);

        public ObservableCollection<Product> LineItems => _currentSale.Products;

        public SalesViewModel()
        {
            try
            {
                _currentSale = new Sale
                {
                    TransactionId = 1001,
                    TerminalId = "POS-01",
                    TransactionDate = DateTime.Now,
                    Cashier = CashierName,
                    Bagger = string.Empty
                };

                InitializeDateTimeTimer();

                // #region agent log
                try
                {
                    var cashierNameProp = typeof(SalesViewModel).GetProperty(nameof(CashierName));
                    var cashierRoleProp = typeof(SalesViewModel).GetProperty(nameof(CashierRole));
                    var logLine = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        sessionId = "15cb7f",
                        runId = "pre-fix",
                        hypothesisId = "A,B",
                        location = "SalesViewModel.cs:ctor",
                        message = "Cashier property read-only and values at VM init",
                        data = new
                        {
                            cashierNameCanWrite = cashierNameProp?.CanWrite,
                            cashierRoleCanWrite = cashierRoleProp?.CanWrite,
                            cashierName = CashierName,
                            cashierRole = CashierRole,
                            hasCurrentUser = Session.CurrentUser != null
                        },
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    }) + Environment.NewLine;
                    System.IO.File.AppendAllText(@"E:\Github Projects\BARTERPLUS-main\debug-15cb7f.log", logLine);
                }
                catch { }
                // #endregion
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SalesViewModel constructor: {ex.Message}");
                _currentSale = new Sale();
                CurrentDateTime = DateTime.Now.ToString("MMMM dd, yyyy | hh:mm:ss tt");
            }
        }

        private void InitializeDateTimeTimer()
        {
            try
            {
                _dateTimeTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                _dateTimeTimer.Tick += (_, _) =>
                {
                    CurrentDateTime = DateTime.Now.ToString("MMMM dd, yyyy | hh:mm:ss tt");
                };
                _dateTimeTimer.Start();
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
            var product = ProductStore.Repository.GetByBarcode(barcode);
            if (product != null)
            {
                AddProduct(product);
            }

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

            RefreshTotals();
        }

        public void RemoveProduct(Product product)
        {
            _currentSale.Products.Remove(product);
            RefreshTotals();
        }

        public void ClearSale()
        {
            _currentSale = new Sale
            {
                TransactionId = _currentSale.TransactionId + 1,
                TerminalId = _currentSale.TerminalId,
                TransactionDate = DateTime.Now,
                Cashier = CashierName,
            };

            IsPwdDiscount = false;
            IsSeniorDiscount = false;
            DeductionInput = string.Empty;

            OnPropertyChanged(nameof(CurrentSale));
            RefreshTotals();
        }

        private void RefreshTotals()
        {
            OnPropertyChanged(nameof(TotalAmount));
            OnPropertyChanged(nameof(TotalItems));
            OnPropertyChanged(nameof(PercentageDiscount));
            OnPropertyChanged(nameof(ManualDeduction));
            OnPropertyChanged(nameof(AmountDue));
            OnPropertyChanged(nameof(LineItems));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

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
        private const decimal LoyaltyEarnRate = 0.001m;

        private Sale _currentSale = null!;
        private string _barcodeInput = string.Empty;
        private string _currentDateTime = string.Empty;
        private string _deductionInput = string.Empty;
        private string _syncStatusText = string.Empty;
        private string _cashDrawerLastActivity = string.Empty;
        private int _pendingSyncCount;
        private decimal _cashDrawerBalance;
        private bool _isPwdDiscount;
        private bool _isSeniorDiscount;
        private Product? _selectedLineItem;
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
                : Session.CurrentUser?.Username ?? "Employee";

        public string CashierRole =>
            Session.CurrentUser?.Role == "Cashier"
                ? "Employee"
                : Session.CurrentUser?.Role ?? "Employee";

        public Customer? CurrentCustomer => _currentSale?.Customer;

        public string CustomerDisplayName
        {
            get
            {
                if (CurrentCustomer == null)
                {
                    return "No customer selected";
                }

                return $"{CurrentCustomer.Name} · {CurrentCustomer.Type}";
            }
        }

        public string CustomerPointsDisplay => CurrentCustomer == null
            ? string.Empty
            : $"{CurrentCustomer.Points:N2} pts | {CurrentCustomer.CreditLimit:C} credit";

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

        public Product? SelectedLineItem
        {
            get => _selectedLineItem;
            set
            {
                if (_selectedLineItem != value)
                {
                    _selectedLineItem = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SyncStatusText
        {
            get => _syncStatusText;
            private set
            {
                if (_syncStatusText != value)
                {
                    _syncStatusText = value;
                    OnPropertyChanged();
                }
            }
        }

        public int PendingSyncCount
        {
            get => _pendingSyncCount;
            private set
            {
                if (_pendingSyncCount != value)
                {
                    _pendingSyncCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal CashDrawerBalance
        {
            get => _cashDrawerBalance;
            private set
            {
                if (_cashDrawerBalance != value)
                {
                    _cashDrawerBalance = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CashDrawerLastActivity
        {
            get => _cashDrawerLastActivity;
            private set
            {
                if (_cashDrawerLastActivity != value)
                {
                    _cashDrawerLastActivity = value;
                    OnPropertyChanged();
                }
            }
        }

        public SalesViewModel()
        {
            try
            {
                _currentSale = CreateSale(TransactionRecordStore.GetNextTransactionId());
                RefreshOperationalState();

                InitializeDateTimeTimer();
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
            if (SelectedLineItem == product)
            {
                SelectedLineItem = null;
            }
            RefreshTotals();
        }

        public bool RemoveSelectedLineItem(out string message)
        {
            if (SelectedLineItem == null)
            {
                message = "Select a line item to void.";
                return false;
            }

            RemoveProduct(SelectedLineItem);
            message = "Selected line item removed.";
            return true;
        }

        public void ClearSale()
        {
            StartNewSale(_currentSale.TransactionId + 1);
        }

        public bool SetCustomer(Customer customer, out string message)
        {
            if (!CustomerLoyaltyValidator.ValidateCustomer(customer, out message))
            {
                return false;
            }

            _currentSale.Customer = customer;
            OnPropertyChanged(nameof(CurrentCustomer));
            OnPropertyChanged(nameof(CustomerDisplayName));
            OnPropertyChanged(nameof(CustomerPointsDisplay));
            message = $"Loyalty customer {customer.Name} has been verified.";
            return true;
        }

        public void ClearCustomer()
        {
            _currentSale.Customer = null;
            OnPropertyChanged(nameof(CurrentCustomer));
            OnPropertyChanged(nameof(CustomerDisplayName));
            OnPropertyChanged(nameof(CustomerPointsDisplay));
        }

        public bool CompleteSale(string paymentMethod, out string message, out SaleTransaction? transaction)
        {
            message = string.Empty;
            transaction = null;

            if (!_currentSale.Products.Any())
            {
                message = "Add at least one product before completing the transaction.";
                return false;
            }

            if (!CustomerLoyaltyValidator.ValidateDiscountEligibility(
                    _currentSale.Customer,
                    IsPwdDiscount,
                    IsSeniorDiscount,
                    out message))
            {
                return false;
            }

            decimal amountDue = AmountDue;
            decimal loyaltyCreditRedeemed = CurrentCustomer == null
                ? 0m
                : Math.Min(ManualDeduction, Math.Max(0m, CurrentCustomer.CreditLimit));
            decimal loyaltyPointsEarned = CurrentCustomer == null
                ? 0m
                : Math.Round(amountDue * LoyaltyEarnRate, 2, MidpointRounding.AwayFromZero);
            var transactionItems = _currentSale.Products
                .GroupBy(product => new { product.Code, product.Name, product.Price })
                .Select(group => new SaleLineItem
                {
                    Code = group.Key.Code,
                    Name = group.Key.Name,
                    UnitPrice = group.Key.Price,
                    Quantity = group.Sum(product => product.Quantity),
                    Subtotal = group.Sum(product => product.Subtotal)
                })
                .ToList();

            var builtTransaction = new SaleTransaction
            {
                TransactionId = _currentSale.TransactionId,
                TerminalId = _currentSale.TerminalId,
                TransactionDate = _currentSale.TransactionDate,
                CompletedAt = DateTime.Now,
                Cashier = CashierName,
                CashierUsername = Session.CurrentUser?.Username ?? string.Empty,
                CustomerId = _currentSale.Customer?.Id,
                CustomerName = _currentSale.Customer?.Name ?? string.Empty,
                CustomerType = _currentSale.Customer?.Type ?? string.Empty,
                PaymentMethod = paymentMethod,
                Items = transactionItems,
                TotalItems = TotalItems,
                GrossAmount = TotalAmount,
                PercentageDiscount = PercentageDiscount,
                ManualDeduction = ManualDeduction,
                LoyaltyCreditRedeemed = loyaltyCreditRedeemed,
                LoyaltyPointsEarned = loyaltyPointsEarned,
                NetAmount = amountDue,
                AmountPaid = amountDue,
                ChangeDue = 0m
            };

            SaveSyncResult transactionResult = TransactionRecordStore.Save(builtTransaction);
            string cashDrawerMessage = string.Empty;
            string loyaltyMessage = string.Empty;

            if (_currentSale.Customer != null)
            {
                Customer updatedCustomer = InMemoryCustomerRepository.Copy(_currentSale.Customer);
                updatedCustomer.Points = Math.Max(0m, updatedCustomer.Points + loyaltyPointsEarned);
                updatedCustomer.CreditLimit = Math.Max(0m, updatedCustomer.CreditLimit - loyaltyCreditRedeemed + loyaltyPointsEarned);

                if (CustomerStore.Repository.Update(updatedCustomer, out string customerUpdateError))
                {
                    _currentSale.Customer = updatedCustomer;
                    OnPropertyChanged(nameof(CurrentCustomer));
                    OnPropertyChanged(nameof(CustomerDisplayName));
                    OnPropertyChanged(nameof(CustomerPointsDisplay));

                    if (loyaltyCreditRedeemed > 0m || loyaltyPointsEarned > 0m)
                    {
                        loyaltyMessage = Environment.NewLine
                            + $"Loyalty: used {loyaltyCreditRedeemed:C} credit, earned {loyaltyPointsEarned:N2} point(s).";
                    }
                }
                else if (!string.IsNullOrWhiteSpace(customerUpdateError))
                {
                    loyaltyMessage = Environment.NewLine + $"Loyalty: {customerUpdateError}";
                }
            }

            if (paymentMethod.Equals("Cash", StringComparison.OrdinalIgnoreCase) && amountDue > 0m)
            {
                var drawerEntry = new CashDrawerEntry
                {
                    TerminalId = _currentSale.TerminalId,
                    Cashier = CashierName,
                    CashierUsername = Session.CurrentUser?.Username ?? string.Empty,
                    Type = CashDrawerEntryTypes.CashSale,
                    Amount = amountDue,
                    Note = $"Cash payment for transaction #{_currentSale.TransactionId}",
                    RelatedTransactionId = _currentSale.TransactionId
                };

                SaveSyncResult drawerResult = CashDrawerStore.AddEntry(drawerEntry);
                cashDrawerMessage = Environment.NewLine + "Cash Drawer: " + drawerResult.Message;
            }

            transaction = builtTransaction;
            message = BuildReceiptSummary(builtTransaction, transactionResult.Message + cashDrawerMessage + loyaltyMessage);
            StartNewSale(TransactionRecordStore.GetNextTransactionId());
            RefreshOperationalState();
            return true;
        }

        public bool RecordCashMovement(string type, decimal amount, string note, out string message)
        {
            message = string.Empty;

            if (amount <= 0m)
            {
                message = "Enter an amount greater than zero.";
                return false;
            }

            var entry = new CashDrawerEntry
            {
                TerminalId = _currentSale.TerminalId,
                Cashier = CashierName,
                CashierUsername = Session.CurrentUser?.Username ?? string.Empty,
                Type = type,
                Amount = amount,
                Note = note.Trim()
            };

            SaveSyncResult result = CashDrawerStore.AddEntry(entry);
            RefreshOperationalState();
            message = result.Message;
            return true;
        }

        public string SyncOfflineData()
        {
            MongoDatabaseFactory.ResetSyncCooldown();

            SyncResult transactionResult = TransactionRecordStore.SyncPending();
            SyncResult drawerResult = CashDrawerStore.SyncPending();

            RefreshOperationalState();

            int totalPending = transactionResult.PendingBeforeSync + drawerResult.PendingBeforeSync;
            int totalSynced = transactionResult.SyncedCount + drawerResult.SyncedCount;
            int totalFailed = transactionResult.FailedCount + drawerResult.FailedCount;
            string lastError = !string.IsNullOrWhiteSpace(transactionResult.LastError)
                ? transactionResult.LastError
                : drawerResult.LastError;

            if (totalPending == 0)
            {
                return "No pending records.";
            }

            if (totalFailed > 0)
            {
                return $"Synced {totalSynced} record(s). {totalFailed} record(s) are still pending. Remote sync will be done after closing hours.";
            }

            return $"Synced {totalSynced} record(s).";
        }

        public string ClearPendingOfflineData()
        {
            int removedTransactions = TransactionRecordStore.ClearPending();
            int removedDrawerEntries = CashDrawerStore.ClearPending();
            int totalRemoved = removedTransactions + removedDrawerEntries;

            RefreshOperationalState();

            if (totalRemoved == 0)
            {
                return "No pending records found.";
            }

            return $"Cleared {totalRemoved} pending record(s).";
        }

        private Sale CreateSale(int transactionId)
        {
            return new Sale
            {
                TransactionId = transactionId,
                TerminalId = "POS-01",
                TransactionDate = DateTime.Now,
                Cashier = CashierName,
                Bagger = string.Empty
            };
        }

        private void StartNewSale(int transactionId)
        {
            _currentSale = CreateSale(transactionId);
            _isPwdDiscount = false;
            _isSeniorDiscount = false;
            _deductionInput = string.Empty;
            _selectedLineItem = null;

            OnPropertyChanged(nameof(CurrentSale));
            OnPropertyChanged(nameof(IsPwdDiscount));
            OnPropertyChanged(nameof(IsSeniorDiscount));
            OnPropertyChanged(nameof(DeductionInput));
            OnPropertyChanged(nameof(SelectedLineItem));
            OnPropertyChanged(nameof(CurrentCustomer));
            OnPropertyChanged(nameof(CustomerDisplayName));
            OnPropertyChanged(nameof(CustomerPointsDisplay));
            RefreshTotals();
        }

        private void RefreshOperationalState()
        {
            int pendingTransactions = TransactionRecordStore.GetPendingCount();
            int pendingDrawerEntries = CashDrawerStore.GetPendingCount();

            PendingSyncCount = pendingTransactions + pendingDrawerEntries;
            SyncStatusText = MongoDatabaseFactory.IsConfigured
                ? PendingSyncCount == 0
                    ? "Ready"
                    : $"{PendingSyncCount} record(s) pending"
                : $"{PendingSyncCount} record(s) pending";
            CashDrawerBalance = CashDrawerStore.GetCurrentBalance();
            CashDrawerLastActivity = CashDrawerStore.GetLastActivityText();
        }

        private string BuildReceiptSummary(SaleTransaction transaction, string syncMessage)
        {
            return $"Receipt #{transaction.TransactionId}" + Environment.NewLine
                + $"Payment: {transaction.PaymentMethod}" + Environment.NewLine
                + $"Items: {transaction.TotalItems}" + Environment.NewLine
                + $"Subtotal: {transaction.GrossAmount:C}" + Environment.NewLine
                + $"Discounts: {(transaction.PercentageDiscount + transaction.ManualDeduction):C}" + Environment.NewLine
                + (transaction.LoyaltyCreditRedeemed > 0m ? $"Loyalty Credit Used: {transaction.LoyaltyCreditRedeemed:C}" + Environment.NewLine : string.Empty)
                + (transaction.LoyaltyPointsEarned > 0m ? $"Loyalty Earned: {transaction.LoyaltyPointsEarned:N2}" + Environment.NewLine : string.Empty)
                + $"Total Paid: {transaction.NetAmount:C}" + Environment.NewLine
                + Environment.NewLine
                + syncMessage;
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

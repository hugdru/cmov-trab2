using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Wallet.Models
{
    public class Account : IEnumerable
    {
        private static object syncLock = new object();
        private ConcurrentDictionary<string, CurrencyAmount> wallet = new ConcurrentDictionary<string, CurrencyAmount>();
        private ObservableCollection<CurrencyAmount> UICollection = null;

        public bool Deposit(string currency, double amount)
        {

            if (amount < 0 || currency == null)
            {
                return false;
            }

            lock (syncLock)
            {
                CurrencyAmount currencyAmount = null;
                if (wallet.TryGetValue(currency, out currencyAmount))
                {
                    currencyAmount.Add(amount);
                }
                else
                {
                    currencyAmount = new CurrencyAmount(currency, amount);
                    wallet[currency] = currencyAmount;
                    UICollection?.Add(currencyAmount);
                }
                return true;
            }
        }

        public bool Withdraw(string currency, double amount)
        {
            if (amount < 0)
            {
                return false;
            }

            lock (syncLock)
            {
                CurrencyAmount currencyAmount = null;
                if (wallet.TryGetValue(currency, out currencyAmount))
                {
                    currencyAmount.Add(-amount);
                }
                else
                {
                    currencyAmount = new CurrencyAmount(currency, -amount);
                    wallet[currency] = currencyAmount;
                    UICollection?.Add(currencyAmount);
                }
                return true;
            }
        }

        public bool Exchange(double fromAmount, string currencyFrom, double quoteValue, string currencyTo)
        {
            if (currencyFrom == null || currencyTo == null)
            {
                return false;
            }

            double toAmount = fromAmount * quoteValue;

            lock (syncLock)
            {
                if (!Withdraw(currencyFrom, fromAmount))
                {
                    return false;
                }
                if (!Deposit(currencyTo, toAmount))
                {
                    Deposit(currencyFrom, fromAmount);
                    return false;
                };
                return true;
            }
        }

        public void RegisterUICollection(ObservableCollection<CurrencyAmount> bindableCollection)
        {
            lock (syncLock)
            {
                UICollection = bindableCollection;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)wallet).GetEnumerator();
        }

        public class CurrencyAmount : INotifyPropertyChanged
        {
            private static object syncLock = new object();

            private string currency;
            public string Currency
            {
                set
                {
                    lock (syncLock)
                    {
                        SetProperty(ref currency, value);
                    }
                }
                get
                {
                    lock (syncLock)
                    {
                        return currency;
                    }
                }
            }

            private double amount;
            public double Amount
            {
                set
                {
                    lock (syncLock)
                    {
                        SetProperty(ref amount, value);
                    }
                }
                get
                {
                    lock (syncLock)
                    {
                        return amount;
                    }
                }
            }

            private void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
            public event PropertyChangedEventHandler PropertyChanged;

            public CurrencyAmount(string currency = "", double amount = 0)
            {
                Currency = currency;
                Amount = amount;
            }

            public override int GetHashCode()
            {
                return Currency.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                var otherCurrencyAmount = obj as CurrencyAmount;
                return otherCurrencyAmount != null && currency.Equals(otherCurrencyAmount.currency);
            }


            public void Add(double add)
            {
                lock (syncLock)
                {
                    Amount = amount + add;
                }
            }
        }
    }
}
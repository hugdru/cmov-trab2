using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Xamarin.Forms;
using Wallet.Services;
using static Wallet.Models.Account;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using static Wallet.Models.QuotesGraph;
using Wallet.Models;
using System.Collections.Generic;

namespace Wallet.Pages
{
    public partial class WalletPage : ContentPage
    {
        public class WalletPageModel : INotifyPropertyChanged
        {
            private string totalCurrency;
            public string TotalCurrency
            {
                set
                {
                    SetProperty(ref totalCurrency, value);
                }
                get
                {
                    return totalCurrency;
                }
            }
            private double totalAmount;
            public double TotalAmount
            {
                set
                {
                    SetProperty(ref totalAmount, value);
                }
                get
                {
                    return totalAmount;
                }
            }
            private string operationDepositCurrency;
            public string OperationDepositCurrency
            {
                set
                {
                    SetProperty(ref operationDepositCurrency, value);
                }
                get
                {
                    return operationDepositCurrency;
                }
            }
            private double operationDepositAmount;
            public double OperationDepositAmount
            {
                set
                {
                    SetProperty(ref operationDepositAmount, value);
                }
                get
                {
                    return operationDepositAmount;
                }
            }
            private string operationWithdrawCurrency;
            public string OperationWithdrawCurrency
            {
                set
                {
                    SetProperty(ref operationWithdrawCurrency, value);
                }
                get
                {
                    return operationWithdrawCurrency;
                }
            }
            private double operationWithdrawAmount;
            public double OperationWithdrawAmount
            {
                set
                {
                    SetProperty(ref operationWithdrawAmount, value);
                }
                get
                {
                    return operationWithdrawAmount;
                }
            }
            private string operationExchangeFromCurrency;
            public string OperationExchangeFromCurrency
            {
                set
                {
                    SetProperty(ref operationExchangeFromCurrency, value);
                }
                get
                {
                    return operationExchangeFromCurrency;
                }
            }
            private string operationExchangeToCurrency;
            public string OperationExchangeToCurrency
            {
                set
                {
                    SetProperty(ref operationExchangeToCurrency, value);
                }
                get
                {
                    return operationExchangeToCurrency;
                }
            }
            private double operationExchangeAmount;
            public double OperationExchangeAmount
            {
                set
                {
                    SetProperty(ref operationExchangeAmount, value);
                }
                get
                {
                    return operationExchangeAmount;
                }
            }
            public ObservableCollection<CurrencyAmount> AccountWallet { set; get; }

            public WalletPageModel()
            {
                TotalCurrency = "";
                TotalAmount = 0;
                OperationDepositCurrency = "";
                OperationDepositAmount = 0;
                OperationWithdrawCurrency = "";
                OperationWithdrawAmount = 0;
                OperationExchangeFromCurrency = "";
                OperationExchangeToCurrency = "";
                OperationExchangeAmount = 0;
                AccountWallet = new ObservableCollection<CurrencyAmount>();
            }

            private void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }

        private WalletPageModel Model;

        private bool depositSuccessful = false;
        private bool withdrawSuccessful = false;
        private bool exchangeSuccessful = false;

        public WalletPage()
        {
            InitializeComponent();
            Model = new WalletPageModel();
            App.Account.RegisterUICollection(Model.AccountWallet);
            Model.AccountWallet.CollectionChanged += CurrencyAmountChangesListener;
            BindingContext = Model;
        }

        private void DepositCliked(object sender, EventArgs e)
        {
            if (!Models.Currencies.Contains(Model.OperationDepositCurrency))
            {
                if (Model.OperationDepositCurrency != "")
                {
                    OperationDepositCurrencySearchBar.TextColor = Color.Red;
                }
                return;
            }
            if (!App.Account.Deposit(Model.OperationDepositCurrency, Model.OperationDepositAmount))
            {
                OperationDepositAmountEntry.TextColor = Color.Red;
                return;
            }
            OperationDepositCurrencySearchBar.TextColor = Color.Green;
            OperationDepositAmountEntry.TextColor = Color.Green;
            depositSuccessful = true;
        }

        private void WithdrawCliked(object sender, EventArgs e)
        {
            if (!Models.Currencies.Contains(Model.OperationWithdrawCurrency))
            {
                if (Model.OperationWithdrawCurrency != "")
                {
                    OperationWithdrawCurrencySearchBar.TextColor = Color.Red;
                }
                return;
            }
            if (!App.Account.Withdraw(Model.OperationWithdrawCurrency, Model.OperationWithdrawAmount))
            {
                OperationWithdrawAmountEntry.TextColor = Color.Red;
                return;
            }
            OperationWithdrawCurrencySearchBar.TextColor = Color.Green;
            OperationWithdrawAmountEntry.TextColor = Color.Green;
            withdrawSuccessful = true;
        }

        private async void ExchangeCliked(object sender, EventArgs e)
        {
            string currencyFrom = Model.OperationExchangeFromCurrency;
            string currencyTo = Model.OperationExchangeToCurrency;
            double amount = Model.OperationExchangeAmount;

            if (!Models.Currencies.Contains(currencyFrom))
            {
                if (currencyFrom != "")
                {
                    OperationExchangeFromCurrencySearchBar.TextColor = Color.Red;
                }
                return;
            }

            if (!Models.Currencies.Contains(currencyTo))
            {
                if (currencyTo != "")
                {
                    OperationExchangeToCurrencySearchBar.TextColor = Color.Red;
                }
                return;
            }

            Task<Quote> getQuoteTask = QuoteService.FetchQuoteAsync(currencyFrom, currencyTo);
            Quote quote = await getQuoteTask;

            if (quote != null)
            {
                Result result = App.QuotesGraph.UpsertQuote(quote);
                if (result != Result.Inserted && result != Result.Updated)
                {
                    return;
                }
            }
            else
            {
                quote = new Quote(currencyFrom, currencyTo);
                Result result = App.QuotesGraph.GetQuote(quote);
                if (result != Result.Found)
                {
                    return;
                }
            }
            if (!App.Account.Exchange(amount, currencyFrom, quote.Value, currencyTo))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    OperationExchangeAmountEntry.TextColor = Color.Red;
                });
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                OperationExchangeFromCurrencySearchBar.TextColor = Color.Green;
                OperationExchangeToCurrencySearchBar.TextColor = Color.Green;
                OperationExchangeAmountEntry.TextColor = Color.Green;
                exchangeSuccessful = true;
            });
        }

        private void OperationDepositTextChanged(object sender, TextChangedEventArgs e)
        {
            if (depositSuccessful)
            {
                OperationDepositCurrencySearchBar.TextColor = Color.Black;
                OperationDepositAmountEntry.TextColor = Color.Black;
                depositSuccessful = false;
            }
            else
            {
                if (sender is Entry)
                {
                    ((Entry)sender).TextColor = Color.Black;
                }
            }
            if (sender is SearchBar)
            {
                ValidateCurrencySearchBar(sender);
            }
        }

        private void OperationWithdrawTextChanged(object sender, TextChangedEventArgs e)
        {
            if (withdrawSuccessful)
            {
                OperationWithdrawCurrencySearchBar.TextColor = Color.Black;
                OperationWithdrawAmountEntry.TextColor = Color.Black;
                withdrawSuccessful = false;
            }
            else
            {
                if (sender is Entry)
                {
                    ((Entry)sender).TextColor = Color.Black;
                }
            }
            if (sender is SearchBar)
            {
                ValidateCurrencySearchBar(sender);
            }
        }

        private void OperationExchangeTextChanged(object sender, TextChangedEventArgs e)
        {
            if (exchangeSuccessful)
            {
                OperationExchangeFromCurrencySearchBar.TextColor = Color.Black;
                OperationExchangeToCurrencySearchBar.TextColor = Color.Black;
                OperationExchangeAmountEntry.TextColor = Color.Black;
                exchangeSuccessful = false;
            }
            else
            {
                if (sender is Entry)
                {
                    ((Entry)sender).TextColor = Color.Black;
                }
            }
            if (sender is SearchBar)
            {
                ValidateCurrencySearchBar(sender);
            }
        }

        private void CheckCurrencyUnfocused(object sender, FocusEventArgs e)
        {
            ValidateCurrencySearchBar(sender);
        }

        private void ValidateCurrencySearchBar(object sender)
        {
            var searchBar = (SearchBar)sender;
            searchBar.Text = searchBar.Text.ToUpper();
            if (!Currencies.Contains(searchBar.Text))
            {
                if (searchBar.Text != "")
                {
                    searchBar.TextColor = Color.Red;
                }
            }
            else
            {
                searchBar.TextColor = Color.Black;
            }
        }

        private void CurrencyAmountChangesListener(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CurrencyAmount item in e.NewItems)
                {
                    item.PropertyChanged += CurrencyAmountChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (CurrencyAmount item in e.OldItems)
                {
                    item.PropertyChanged -= CurrencyAmountChanged;
                }
            }

            CalculateTotal();
        }

        private void CurrencyAmountChanged(object sender, PropertyChangedEventArgs e)
        {
            CalculateTotal();
        }

        private async void CalculateTotal()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                TotalAmountLabel.TextColor = Color.Black;
            });

            var targetCurrency = Model.TotalCurrency;
            if (!Currencies.Contains(targetCurrency))
            {
                if (targetCurrency != "")
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        TotalCurrencySearchBar.TextColor = Color.Red;
                    });
                }
                return;
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                TotalCurrencySearchBar.TextColor = Color.Black;
            });

            Task<List<Quote>> getQuotesTask = QuoteService.FetchQuotesAsync(targetCurrency, App.Account);
            List<Quote> quotes = await getQuotesTask;
            if (quotes != null)
            {
                foreach (Quote quote in quotes)
                {
                    App.QuotesGraph.UpsertQuote(quote);
                }
            }

            double totalAmount = 0.0;
            foreach (KeyValuePair<string, CurrencyAmount> tuple in App.Account)
            {
                double quoteValue = 0.0;
                if (App.QuotesGraph.GetQuote(tuple.Key, targetCurrency, out quoteValue) != Result.Found)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        TotalAmountLabel.TextColor = Color.Red;
                        Model.TotalAmount = 0;
                    });
                    return;
                }
                totalAmount += tuple.Value.Amount * quoteValue;
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                TotalCurrencySearchBar.TextColor = Color.Navy;
                TotalAmountLabel.TextColor = Color.Navy;
                Model.TotalAmount = totalAmount;
            });
            return;
        }

        private void TotalCurrencyTextChanged(object sender, TextChangedEventArgs e)
        {
            TotalCurrencySearchBar.Text = TotalCurrencySearchBar.Text.ToUpper();
            CalculateTotal();
        }
    }
}
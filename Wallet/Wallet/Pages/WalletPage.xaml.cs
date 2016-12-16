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
            private Currency totalCurrency;
            public Currency TotalCurrency
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
            private decimal totalAmount;
            public decimal TotalAmount
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
            private Currency operationDepositCurrency;
            public Currency OperationDepositCurrency
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
            private decimal operationDepositAmount;
            public decimal OperationDepositAmount
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
            private Currency operationWithdrawCurrency;
            public Currency OperationWithdrawCurrency
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
            private decimal operationWithdrawAmount;
            public decimal OperationWithdrawAmount
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
            private Currency operationExchangeFromCurrency;
            public Currency OperationExchangeFromCurrency
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
            private Currency operationExchangeToCurrency;
            public Currency OperationExchangeToCurrency
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
            private decimal operationExchangeAmount;
            public decimal OperationExchangeAmount
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
            public ObservableCollection<Currency> Currencies { set; get; }
            private String message;
            public String Message
            {
                set
                {
                    SetProperty(ref message, value);
                }
                get
                {
                    return message;
                }
            }

            public WalletPageModel()
            {
                TotalCurrency = null;
                TotalAmount = 0.0M;
                OperationDepositCurrency = null;
                OperationDepositAmount = 0.0M;
                OperationWithdrawCurrency = null;
                OperationWithdrawAmount = 0.0M;
                OperationExchangeFromCurrency = null;
                OperationExchangeToCurrency = null;
                OperationExchangeAmount = 0.0M;
                AccountWallet = new ObservableCollection<CurrencyAmount>();
                Currencies = new ObservableCollection<Currency>();
            }

            private void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }

        private WalletPageModel Model;

        public WalletPage()
        {
            InitializeComponent();
            Model = new WalletPageModel();
            App.Account.RegisterUICollection(Model.AccountWallet);
            Model.AccountWallet.CollectionChanged += CurrencyAmountChangesListener;
            BindingContext = Model;
        }

        public ObservableCollection<Currency> GetBindableCurrencies()
        {
            return Model.Currencies;
        }

        public void RegisterCurrencies(ObservableCollection<Currency> currencies)
        {
            Model.Currencies = currencies;
        }

        private void DepositCliked(object sender, EventArgs e)
        {
            if (!App.Account.Deposit(Model.OperationDepositCurrency, Model.OperationDepositAmount))
            {
                SetMessage(false, "DEPOSIT FAILED");
                return;
            }
            SetMessage(true, "DEPOSIT SUCCESSFUL");
        }

        private void WithdrawCliked(object sender, EventArgs e)
        {
            if (!App.Account.Withdraw(Model.OperationWithdrawCurrency, Model.OperationWithdrawAmount))
            {
                SetMessage(false, "WITHDRAW FAILED");
                return;
            }
            SetMessage(true, "WITHDRAW SUCCESSFUL");
        }

        private async void ExchangeCliked(object sender, EventArgs e)
        {
            Currency currencyFrom = Model.OperationExchangeFromCurrency;
            Currency currencyTo = Model.OperationExchangeToCurrency;
            decimal amount = Model.OperationExchangeAmount;

            Task<Quote> getQuoteTask = QuoteService.FetchQuoteAsync(currencyFrom, currencyTo);
            Quote quote = await getQuoteTask;

            if (quote != null)
            {
                Result result = App.QuotesGraph.UpsertQuote(quote);
                if (result != Result.Inserted && result != Result.Updated)
                {
                    SetMessage(false, "EXCHANGE QUOTE UPSERT FAILED");
                    return;
                }
            }
            else
            {
                quote = new Quote(currencyFrom, currencyTo);
                Result result = App.QuotesGraph.GetQuote(quote);
                if (result != Result.Found)
                {
                    SetMessage(false, "EXCHANGE QUOTE NOT FOUND LOCALLY");
                    return;
                }
            }
            if (!App.Account.Exchange(amount, currencyFrom, quote.Value, currencyTo))
            {

                SetMessage(false, "EXCHANGE FAILED");
                return;
            }
            SetMessage(true, "EXCHANGE SUCCESSFUL");
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

        private void TotalCurrencyChanged(object sender, EventArgs e)
        {
            CalculateTotal();
        }

        private async void CalculateTotal()
        {
            var targetCurrency = Model.TotalCurrency;
            if (Currency.IsNullOrEmpty(targetCurrency))
            {
                return;
            }

            Task<List<Quote>> getQuotesTask = QuoteService.FetchQuotesAsync(targetCurrency, App.Account);
            List<Quote> quotes = await getQuotesTask;
            if (quotes != null)
            {
                foreach (Quote quote in quotes)
                {
                    App.QuotesGraph.UpsertQuote(quote);
                }
            }

            decimal totalAmount = 0.0M;
            foreach (KeyValuePair<Currency, CurrencyAmount> tuple in App.Account)
            {
                decimal quoteValue = 0.0M;
                if (App.QuotesGraph.GetQuote(tuple.Key, targetCurrency, out quoteValue) != Result.Found)
                {
                    SetMessage(false, "MISSING QUOTE CANT CALCULATE TOTAL");
                    Model.TotalAmount = 0;
                    return;
                }
                totalAmount += tuple.Value.Amount * quoteValue;
            }
            Model.TotalAmount = totalAmount;
            return;
        }

        public ObservableCollection<CurrencyAmount> GetAccountWallet()
        {
            return Model.AccountWallet;
        }

        private void SetMessage(bool success, string message)
        {
            if (String.IsNullOrEmpty(message) || String.IsNullOrWhiteSpace(message))
            {
                return;
            }
            Device.BeginInvokeOnMainThread(() =>
            {
                if (success)
                {
                    MessageLabel.TextColor = Color.Green;
                }
                else
                {
                    MessageLabel.TextColor = Color.Red;
                }
                Model.Message = message;
                MessageLabel.IsVisible = true;
            });
        }

        private void DisableMessage()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                //MessageLabel.IsVisible = false;
                Model.Message = "";
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (Currency.IsNullOrEmpty(Model.TotalCurrency))
            {
                Model.TotalAmount = 0.0M;
            }
        }

        private async void AccountWalletListViewItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;
            var currencyAmount = (CurrencyAmount)e.SelectedItem;
            var answer = await DisplayAlert("Question?", "Do you want to delete " + currencyAmount.Currency + " from your Account", "Yes", "No");
            if (answer)
            {
                if (!App.Account.RemoveCurrency(new Currency(currencyAmount.Currency)))
                {
                    SetMessage(false, "FAILED TO REMOVE CURRENCY FROM ACCOUNT");
                    return;
                }
                SetMessage(true, "SUCCESSFULLY REMOVED CURRENCY FROM ACCOUNT");
            }
            ((ListView)sender).SelectedItem = null;
        }
    }
}
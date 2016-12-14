using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Models;
using Xamarin.Forms;

namespace Wallet.Pages
{
    public partial class MainPage : TabbedPage
    {
        public MainPage()
        {
            var walletPage = new WalletPage();
            var currenciesPage = new CurrenciesPage(App.Currencies, walletPage.GetBindableCurrencies());

            Children.Add(walletPage);
            Children.Add(currenciesPage);
        }
    }
}
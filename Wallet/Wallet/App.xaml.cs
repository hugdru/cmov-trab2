using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

using Wallet.Pages;

namespace Wallet
{
    public partial class App : Application
    {

        public static Wallet.Models.Account Account = new Wallet.Models.Account();
        public static Wallet.Models.QuotesGraph QuotesGraph = new Wallet.Models.QuotesGraph();
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
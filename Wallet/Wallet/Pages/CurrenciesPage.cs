using System;

using Xamarin.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

using Wallet.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

/*
 * Based on https://github.com/xamarin/recipes/blob/master/cross-platform/xamarin-forms/Controls/multiselect/Multiselect/SelectMultipleBasePage.cs
 */

namespace Wallet.Pages
{
    public class CurrenciesPage : ContentPage
    {
        public class WrappedCurrency : INotifyPropertyChanged
        {
            public Currency Currency { get; set; }
            bool isSelected = false;
            public bool IsSelected
            {
                get
                {
                    return isSelected;
                }
                set
                {
                    if (isSelected != value)
                    {
                        isSelected = value;
                        PropertyChanged(this, new PropertyChangedEventArgs("IsSelected"));
                    }
                }
            }
            public event PropertyChangedEventHandler PropertyChanged = delegate { };
        }
        public class WrappedCurrencySelectionTemplate : ViewCell
        {
            public WrappedCurrencySelectionTemplate() : base()
            {
                Label name = new Label();
                name.SetBinding(Label.TextProperty, new Binding("Currency.Name"));
                Switch mainSwitch = new Switch();
                mainSwitch.SetBinding(Switch.IsToggledProperty, new Binding("IsSelected"));
                RelativeLayout layout = new RelativeLayout();
                layout.Children.Add(name,
                    Constraint.Constant(5),
                    Constraint.Constant(5),
                    Constraint.RelativeToParent(p => p.Width - 60),
                    Constraint.RelativeToParent(p => p.Height - 10)
                );
                layout.Children.Add(mainSwitch,
                    Constraint.RelativeToParent(p => p.Width - 55),
                    Constraint.Constant(5),
                    Constraint.Constant(50),
                    Constraint.RelativeToParent(p => p.Height - 10)
                );
                View = layout;
            }
        }
        public List<WrappedCurrency> WrappedCurrencies = new List<WrappedCurrency>();
        public class CurrenciesViewModel
        {
            public string SearchBarText { set; get; }
            public CurrenciesViewModel()
            {
                SearchBarText = "";
            }
        }

        public CurrenciesViewModel ViewModel;
        public ObservableCollection<Currency> BindableCurrencies;

        public CurrenciesPage(Currencies currencies, ObservableCollection<Currency> bindableCurrencies)
        {
            Title = "Currencies";
            WrappedCurrencies = new List<WrappedCurrency>();
            BindableCurrencies = bindableCurrencies;
            bindableCurrencies.Clear();

            foreach (var currency in currencies.currencies)
            {
                bool isSelected = false;
                if (currencies.defaultCurrencies.Contains(currency.Name))
                {
                    isSelected = true;
                    bindableCurrencies.Add(currency);
                }
                WrappedCurrencies.Add(new WrappedCurrency() { Currency = currency, IsSelected = isSelected });
            }

            var stackLayout = new StackLayout();

            ListView mainList = new ListView()
            {
                ItemsSource = WrappedCurrencies,
                ItemTemplate = new DataTemplate(typeof(WrappedCurrencySelectionTemplate)),
            };

            mainList.ItemSelected += (sender, e) =>
            {
                if (e.SelectedItem == null) return;
                var o = (WrappedCurrency)e.SelectedItem;
                o.IsSelected = !o.IsSelected;
                ((ListView)sender).SelectedItem = null; //de-select
            };
            var searchBar = new SearchBar
            {
                Placeholder = "Filter by",
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BindingContext = ViewModel
            };
            searchBar.SetBinding<CurrenciesViewModel>(SearchBar.TextProperty, vm => vm.SearchBarText, BindingMode.TwoWay);
            searchBar.TextChanged += (sender, e) =>
            {
                if (string.IsNullOrEmpty(searchBar.Text) || string.IsNullOrWhiteSpace(searchBar.Text))
                {
                    mainList.ItemsSource = WrappedCurrencies;
                }
                else
                {
                    searchBar.Text = searchBar.Text.ToUpper();
                    mainList.ItemsSource = WrappedCurrencies.
                        Where(wrappedCurrency => wrappedCurrency.Currency.Name.Contains(searchBar.Text)).ToList();
                }
            };
            stackLayout.Children.Add(mainList);
            stackLayout.Children.Add(searchBar);
            Content = stackLayout;
            if (Device.OS == TargetPlatform.Windows)
            {   // fix issue where rows are badly sized (as tall as the screen) on WinPhone8.1
                mainList.RowHeight = 40;
                // also need icons for Windows app bar (other platforms can just use text)
                ToolbarItems.Add(new ToolbarItem("All", "check.png", SelectAll, ToolbarItemOrder.Primary));
                ToolbarItems.Add(new ToolbarItem("None", "cancel.png", SelectNone, ToolbarItemOrder.Primary));
            }
            else
            {
                ToolbarItems.Add(new ToolbarItem("All", null, SelectAll, ToolbarItemOrder.Primary));
                ToolbarItems.Add(new ToolbarItem("None", null, SelectNone, ToolbarItemOrder.Primary));
            }
        }
        void SelectAll()
        {
            foreach (var wrappedCurrency in WrappedCurrencies)
            {
                wrappedCurrency.IsSelected = true;
            }
        }
        void SelectNone()
        {
            foreach (var wrappedCurrency in WrappedCurrencies)
            {
                wrappedCurrency.IsSelected = false;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            BindableCurrencies.Clear();
            foreach (var WrappedCurrency in WrappedCurrencies)
            {
                if (WrappedCurrency.IsSelected)
                {
                    BindableCurrencies.Add(WrappedCurrency.Currency);
                }
            }
        }
    }
}
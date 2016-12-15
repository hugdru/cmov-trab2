using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

using Xamarin.Forms;
using static Wallet.Models.Account;

namespace Wallet.Pages
{
    public class GraphPage : ContentPage
    {
        public ObservableCollection<CurrencyAmount> AccountWallet;
        public GraphPage(ObservableCollection<CurrencyAmount> accountWallet)
        {
            Title = "Graph";
            AccountWallet = accountWallet;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            StackLayout graph = new StackLayout();
            graph.Orientation = StackOrientation.Vertical;
            graph.HorizontalOptions = LayoutOptions.FillAndExpand;
            ColourGenerator generator = new ColourGenerator();

            decimal total = 0.0M;
            foreach (var currencyAmount in AccountWallet)
            {
                total += currencyAmount.Amount;
            }

            foreach (var currencyAmount in AccountWallet)
            {
                var grid = new Grid();
                var row = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
                grid.RowDefinitions.Add(row);
                var labelColumn = new ColumnDefinition { Width = GridLength.Auto };
                var barColumn = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
                grid.ColumnDefinitions.Add(labelColumn);
                grid.ColumnDefinitions.Add(barColumn);


                Label label = new Label();
                label.Text = currencyAmount.Currency;
                grid.Children.Add(label, 1, 0);

                StackLayout boxViewWrapper = new StackLayout();
                boxViewWrapper.Orientation = StackOrientation.Horizontal;
                boxViewWrapper.HorizontalOptions = LayoutOptions.FillAndExpand;
                boxViewWrapper.VerticalOptions = LayoutOptions.Center;
                RelativeLayout relativeLayout = new RelativeLayout();
                BoxView bv = new BoxView();
                bv.VerticalOptions = LayoutOptions.StartAndExpand;
                bv.BackgroundColor = Color.FromHex(generator.NextColour());
                relativeLayout.Children.Add(bv, Constraint.RelativeToParent((parent) =>
                {
                    return 0;
                }), Constraint.RelativeToParent((parent) =>
                {
                    return 0;
                }), Constraint.RelativeToParent((parent) =>
                {
                    return parent.Width * (double)(currencyAmount.Amount / total);
                }), Constraint.RelativeToParent((parent) =>
                {
                    return parent.Height;
                }));
                boxViewWrapper.Children.Add(relativeLayout);

                grid.Children.Add(boxViewWrapper, 1, 1);

                graph.Children.Add(grid);
            }
            Content = graph;
        }
    }
}

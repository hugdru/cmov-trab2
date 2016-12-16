using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Models
{
    public class Currency : INotifyPropertyChanged
    {
        private string name;
        public string Name
        {
            set
            {
                SetProperty(ref name, value);
            }
            get
            {
                return name;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Currency(string name)
        {
            Name = name;
        }
        public override bool Equals(object obj)
        {
            var otherCurrency = obj as Currency;
            return otherCurrency != null && Name.Equals(otherCurrency.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool IsNullOrEmpty(Currency currency)
        {
            return currency == null || String.IsNullOrEmpty(currency.Name);
        }
    }
}

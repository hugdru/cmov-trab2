using System;

namespace Wallet.Models
{
    public class Quote
    {
        public string From { set; get; }
        public string To { set; get; }
        public decimal Value { set; get; }

        public Quote(Currency F, Currency T, decimal V = 0.0M)
        {
            if (Currency.IsNullOrEmpty(F) || Currency.IsNullOrEmpty(T))
            {
                throw new System.ArgumentNullException();
            }
            From = F.Name;
            To = T.Name;
            Value = V;
        }

        public static bool IsNullOrEmpty(Quote quote)
        {
            return quote == null || String.IsNullOrEmpty(quote.From) || String.IsNullOrEmpty(quote.To);
        }
    }
}
namespace Wallet.Models
{
    public class Quote
    {
        public string From { set; get; }
        public string To { set; get; }
        public decimal Value { set; get; }

        public Quote(Currency F, Currency T, decimal V = 0.0M)
        {
            From = F.Name;
            To = T.Name;
            Value = V;
        }
    }
}
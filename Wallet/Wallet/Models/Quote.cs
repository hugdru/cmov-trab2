namespace Wallet.Models
{
    public class Quote
    {
        public string From { set; get; }
        public string To { set; get; }
        public double Value { set; get; }

        public Quote(string F, string T, double V = 0.0)
        {
            From = F;
            To = T;
            Value = V;
        }
    }
}
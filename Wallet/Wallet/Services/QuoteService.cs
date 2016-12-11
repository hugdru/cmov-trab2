using System;
using System.IO;
using System.Net;
using Wallet.Models;

namespace Wallet.Services
{
    public static class QuoteService
    {
        private const string unformatedUri = "http://download.finance.yahoo.com/d/quotes?f=sl1d1t1&s={0}{1}=X";

        public static void FetchQuote(string currencyFrom, string currencyTo, AsyncCallback callback)
        {
            var uri = string.Format(unformatedUri, currencyFrom, currencyTo);
            var request = HttpWebRequest.Create(uri);
            request.Method = "GET";
            var quote = new Quote(currencyFrom, currencyTo);
            var state = new Tuple<Quote, WebRequest>(quote, request);
            request.BeginGetResponse(callback, state);
        }

        public static Quote ParseQuote(IAsyncResult asyncResult)
        {
            try
            {
                var state = (Tuple<Quote, WebRequest>)asyncResult.AsyncState;
                var quote = state.Item1;
                var request = state.Item2;
                using (HttpWebResponse response = request.EndGetResponse(asyncResult) as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            var content = reader.ReadToEnd();
                            var values = content.Split(',');
                            if (values == null || values.Length != 4)
                            {
                                return null;
                            }
                            quote.Value = double.Parse(values[1]);
                            return quote;
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
            return null;
        }
    }
}
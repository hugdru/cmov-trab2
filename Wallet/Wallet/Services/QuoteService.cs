using System;
using System.IO;
using System.Net;
using Wallet.Models;
using System.Threading.Tasks;
using static Wallet.Models.Account;
using System.Collections.Generic;
using System.Net.Http;

namespace Wallet.Services
{
    public static class QuoteService
    {
        private const string unformatedUri = "http://download.finance.yahoo.com/d/quotes?f=sl1d1t1&s={0}{1}=X";

        private static string BuildUri(string currencyFrom, string currencyTo)
        {
            return string.Format(unformatedUri, currencyFrom, currencyTo);
        }

        public static bool FetchQuoteByCallback(Currency currencyFrom, Currency currencyTo, AsyncCallback callback)
        {
            if (currencyFrom == null || currencyTo == null || callback == null)
            {
                return false;
            }
            var request = HttpWebRequest.Create(BuildUri(currencyFrom.Name, currencyTo.Name));
            request.Method = "GET";
            var quote = new Quote(currencyFrom, currencyTo);
            var state = new Tuple<Quote, WebRequest>(quote, request);
            request.BeginGetResponse(callback, state);
            return true;
        }

        private static bool ParseQuote(string content, Quote quote)
        {
            var values = content.Split(',');
            if (values == null || values.Length != 4)
            {
                return false;
            }
            try
            {
                quote.Value = decimal.Parse(values[1]);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public static Quote ParseQuoteByCallbackResult(IAsyncResult asyncResult)
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
                            if (!ParseQuote(content, quote))
                            {
                                return null;
                            }
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

        public static async Task<List<Quote>> FetchQuotesAsync(Currency targetCurrency, Account account)
        {
            List<Quote> quotes = new List<Quote>();
            foreach (KeyValuePair<Currency, CurrencyAmount> tuple in account)
            {
                Task<Quote> getQuoteTask = FetchQuoteAsync(tuple.Key, targetCurrency);
                Quote quote = await getQuoteTask;
                if (quote == null)
                {
                    return null;
                }
                quotes.Add(quote);
            }
            return quotes;
        }

        public static async Task<Quote> FetchQuoteAsync(Currency currencyFrom, Currency currencyTo)
        {
            try
            {
                if (currencyFrom == null || currencyTo == null)
                {
                    return null;
                }

                Quote quote = new Quote(currencyFrom, currencyTo);

                if (currencyFrom.Equals(currencyTo))
                {
                    quote.Value = 1.0M;
                    return quote;
                }

                HttpClient httpClient = new HttpClient();
                Task<HttpResponseMessage> getTask = httpClient.GetAsync(BuildUri(currencyFrom.Name, currencyTo.Name));
                HttpResponseMessage response = await getTask;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    if (!ParseQuote(content, quote))
                    {
                        return null;
                    }
                    return quote;
                }
            }
            catch (Exception e)
            {
                return null;
            }
            return null;
        }
    }
}
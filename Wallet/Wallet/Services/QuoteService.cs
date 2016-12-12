﻿using System;
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

        public static bool FetchQuoteByCallback(string currencyFrom, string currencyTo, AsyncCallback callback)
        {
            if (String.IsNullOrEmpty(currencyFrom) || String.IsNullOrEmpty(currencyTo) || callback == null)
            {
                return false;
            }
            var request = HttpWebRequest.Create(BuildUri(currencyFrom, currencyTo));
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
                quote.Value = double.Parse(values[1]);
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

        public static async Task<List<Quote>> FetchQuotesAsync(string targetCurrency, Account account)
        {
            List<Quote> quotes = new List<Quote>();
            foreach (KeyValuePair<string, CurrencyAmount> tuple in account)
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

        public static async Task<Quote> FetchQuoteAsync(string currencyFrom, string currencyTo)
        {
            try
            {
                if (String.IsNullOrEmpty(currencyFrom) || String.IsNullOrEmpty(currencyTo))
                {
                    return null;
                }

                Quote quote = new Quote(currencyFrom, currencyTo);

                if (currencyFrom.Equals(currencyTo))
                {
                    quote.Value = 1.0;
                    return quote;
                }

                HttpClient httpClient = new HttpClient();
                Task<HttpResponseMessage> getTask = httpClient.GetAsync(BuildUri(currencyFrom, currencyTo));
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
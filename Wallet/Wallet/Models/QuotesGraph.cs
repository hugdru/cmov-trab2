using System.Collections.Generic;

namespace Wallet.Models
{
    public class QuotesGraph
    {
        public enum Result
        {
            Error = -1,
            Inserted = 0,
            Updated = 1,
            Found = 2,
            NotFound = 3
        }

        private readonly object syncLock = new object();
        private Dictionary<string, Dictionary<string, decimal>> graph = new Dictionary<string, Dictionary<string, decimal>>();

        public Result UpsertQuote(Quote quote)
        {
            if (quote.Value <= 0 || quote.From == null || quote.To == null)
            {
                return Result.Error;
            }

            lock (syncLock)
            {
                Result result = Result.Updated;
                Dictionary<string, decimal> edgesFromTo;
                if (!graph.TryGetValue(quote.From, out edgesFromTo))
                {
                    edgesFromTo = new Dictionary<string, decimal>();
                    graph[quote.From] = edgesFromTo;
                    result = Result.Inserted;
                }
                edgesFromTo[quote.To] = quote.Value;

                Dictionary<string, decimal> edgesToFrom;
                if (!graph.TryGetValue(quote.To, out edgesToFrom))
                {
                    edgesToFrom = new Dictionary<string, decimal>();
                    graph[quote.To] = edgesToFrom;
                }
                edgesToFrom[quote.From] = 1 / quote.Value;
                return result;
            }
        }

        public Result GetQuote(Currency from, Currency to, out decimal value)
        {
            value = 0;
            if (from == null || to == null)
            {
                return Result.Error;
            }

            lock (syncLock)
            {
                Dictionary<string, decimal> edgesFromTo;
                if (!graph.TryGetValue(from.Name, out edgesFromTo))
                {
                    return Result.NotFound;
                }
                if (!edgesFromTo.TryGetValue(to.Name, out value))
                {
                    return Result.NotFound;
                }
                return Result.Found;
            }
        }

        public Result GetQuote(string from, string to, out decimal value)
        {
            value = 0;
            if (from == null || to == null)
            {
                return Result.Error;
            }

            lock (syncLock)
            {
                Dictionary<string, decimal> edgesFromTo;
                if (!graph.TryGetValue(from, out edgesFromTo))
                {
                    return Result.NotFound;
                }
                if (!edgesFromTo.TryGetValue(to, out value))
                {
                    return Result.NotFound;
                }
                return Result.Found;
            }
        }

        public Result GetQuote(Quote quote)
        {
            decimal value;
            var result = GetQuote(quote.From, quote.To, out value);
            if (result == Result.Found)
            {
                quote.Value = value;
            }
            return result;
        }
    }
}
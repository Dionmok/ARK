using System.Collections.Generic;
using ARK.Core;

namespace ARK.App
{
    public class WatchlistQuote
    {
        public string Symbol { get; set; } = "";
        public double Last { get; set; }
        public double Change { get; set; }
        public double PercentChange { get; set; }
        public long Volume { get; set; }
        public double High { get; set; }
        public double Low { get; set; }

        public static List<WatchlistQuote> GetSampleData()
        {
            var quotes = new List<WatchlistQuote>();

            foreach (var symbol in SampleData.Symbols)
            {
                double basePrice = SampleData.RandomDouble(20, 800);
                double change = SampleData.RandomDouble(-15, 15);
                double last = basePrice + change;
                double percentChange = basePrice != 0 ? (change / basePrice) * 100 : 0;

                double high = last + SampleData.RandomDouble(0, 10);
                double low = last - SampleData.RandomDouble(0, 10);

                quotes.Add(new WatchlistQuote
                {
                    Symbol = symbol,
                    Last = System.Math.Round(last, 2),
                    Change = System.Math.Round(change, 2),
                    PercentChange = System.Math.Round(percentChange, 2),
                    Volume = SampleData.RandomInt(10000, 5000000),
                    High = System.Math.Round(high, 2),
                    Low = System.Math.Round(low, 2)
                });
            }

            return quotes;
        }
    }
}
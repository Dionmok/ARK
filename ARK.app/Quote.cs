using System.Collections.Generic;
using ARK.Core;

namespace ARK.App
{
    public class Quote
    {
        public string Symbol { get; set; } = "";
        public double Last { get; set; }
        public double Change { get; set; }
        public double PercentChange { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
        public long ContractVolume { get; set; }

        public static readonly List<string> FuturesSymbols = new()
        {
            "ES", "NQ", "YM", "RTY", "CL", "GC", "SI"
        };

        public static List<Quote> GetSampleData()
        {
            var quotes = new List<Quote>();

            foreach (var symbol in FuturesSymbols)
            {
                double basePrice = SampleData.RandomDouble(50, 5000);
                double change = SampleData.RandomDouble(-50, 50);
                double last = basePrice + change;
                double percentChange = basePrice != 0 ? (change / basePrice) * 100 : 0;
                double spread = SampleData.RandomDouble(0.25, 2);

                quotes.Add(new Quote
                {
                    Symbol = symbol,
                    Last = System.Math.Round(last, 2),
                    Change = System.Math.Round(change, 2),
                    PercentChange = System.Math.Round(percentChange, 2),
                    Bid = System.Math.Round(last - spread / 2, 2),
                    Ask = System.Math.Round(last + spread / 2, 2),
                    ContractVolume = SampleData.RandomInt(100000, 3000000)
                });
            }

            return quotes;
        }
    }
}
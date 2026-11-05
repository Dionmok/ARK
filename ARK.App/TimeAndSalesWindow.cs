using System;
using System.Collections.Generic;
using ARK.Core;

namespace ARK.App
{
    public class TimeAndSalesEntry
    {
        public string MonthDay { get; set; } = "";
        public string Time { get; set; } = "";
        public double Price { get; set; }
        public int Volume { get; set; }
        public int NumberOfTrades { get; set; }
        public string Type { get; set; } = "";
        public double Bid { get; set; }
        public int BidSize { get; set; }
        public double Ask { get; set; }
        public int AskSize { get; set; }

        public static List<TimeAndSalesEntry> GetSampleData(int count = 100)
        {
            var entries = new List<TimeAndSalesEntry>();
            var now = DateTime.Now;
            double lastPrice = SampleData.RandomDouble(50, 500);

            for (int i = 0; i < count; i++)
            {
                var timestamp = now.AddSeconds(-i * SampleData.RandomInt(1, 8));

                double move = SampleData.RandomDouble(-0.5, 0.5);
                lastPrice = Math.Max(1, lastPrice + move);

                double spread = SampleData.RandomDouble(0.01, 0.1);
                double bid = lastPrice - spread / 2;
                double ask = lastPrice + spread / 2;

                bool atAsk = SampleData.RandomInt(0, 1) == 1;

                entries.Add(new TimeAndSalesEntry
                {
                    MonthDay = timestamp.ToString("MM/dd"),
                    Time = timestamp.ToString("HH:mm:ss"),
                    Price = Math.Round(lastPrice, 2),
                    Volume = SampleData.RandomInt(1, 500),
                    NumberOfTrades = SampleData.RandomInt(1, 20),
                    Type = atAsk ? "Buy" : "Sell",
                    Bid = Math.Round(bid, 2),
                    BidSize = SampleData.RandomInt(1, 200),
                    Ask = Math.Round(ask, 2),
                    AskSize = SampleData.RandomInt(1, 200)
                });
            }

            return entries;
        }
    }
}
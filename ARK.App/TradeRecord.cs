using System;
using System.Collections.Generic;
using ARK.Core;

namespace ARK.App
{
    public class TradeRecord
    {
        public string Symbol { get; set; } = "";
        public string Account { get; set; } = "";
        public DateTime Date { get; set; }
        public double EntryPrice { get; set; }
        public double ExitPrice { get; set; }
        public int Size { get; set; }
        public double ProfitLoss { get; set; }
        public double MaxFavorableExcursion { get; set; }
        public double MaxAdverseExcursion { get; set; }

        public static readonly List<string> SampleAccounts = new()
        {
            "SIM-1001", "SIM-1002", "SIM-1003"
        };

        public static List<TradeRecord> GetSampleData(int months)
        {
            var trades = new List<TradeRecord>();
            int count = months * 20;

            for (int i = 0; i < count; i++)
            {
                double entry = SampleData.RandomPrice(100, 20);
                double pl = SampleData.RandomDouble(-150, 200);
                double exit = entry + pl;

                double mfe = Math.Abs(pl) + SampleData.RandomDouble(0, 50);
                double mae = SampleData.RandomDouble(0, 40);

                trades.Add(new TradeRecord
                {
                    Symbol = SampleData.RandomSymbol(),
                    Account = SampleAccounts[SampleData.RandomInt(0, SampleAccounts.Count - 1)],
                    Date = SampleData.RandomDateWithinLastMonths(months),
                    EntryPrice = Math.Round(entry, 2),
                    ExitPrice = Math.Round(exit, 2),
                    Size = SampleData.RandomInt(1, 5),
                    ProfitLoss = Math.Round(pl, 2),
                    MaxFavorableExcursion = Math.Round(mfe, 2),
                    MaxAdverseExcursion = Math.Round(mae, 2)
                });
            }

            trades.Sort((a, b) => a.Date.CompareTo(b.Date));
            return trades;
        }
    }
}
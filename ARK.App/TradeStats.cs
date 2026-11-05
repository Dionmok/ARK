using System.Collections.Generic;
using System.Linq;

namespace ARK.App
{
    public class TradeStats
    {
        public double WinRate { get; set; }
        public double AverageWin { get; set; }
        public double AverageLoss { get; set; }
        public double BiggestWin { get; set; }
        public double BiggestLoss { get; set; }
        public double MaxFavorableExcursion { get; set; }
        public double MaxAdverseExcursion { get; set; }

        public static TradeStats Calculate(List<TradeRecord> trades)
        {
            var stats = new TradeStats();

            if (trades.Count == 0)
                return stats;

            var wins = trades.Where(t => t.ProfitLoss > 0).ToList();
            var losses = trades.Where(t => t.ProfitLoss < 0).ToList();

            stats.WinRate = (double)wins.Count / trades.Count * 100;

            stats.AverageWin = wins.Count > 0 ? wins.Average(t => t.ProfitLoss) : 0;
            stats.AverageLoss = losses.Count > 0 ? losses.Average(t => t.ProfitLoss) : 0;

            stats.BiggestWin = wins.Count > 0 ? wins.Max(t => t.ProfitLoss) : 0;
            stats.BiggestLoss = losses.Count > 0 ? losses.Min(t => t.ProfitLoss) : 0;

            stats.MaxFavorableExcursion = trades.Max(t => t.MaxFavorableExcursion);
            stats.MaxAdverseExcursion = trades.Max(t => t.MaxAdverseExcursion);

            return stats;
        }
    }
}
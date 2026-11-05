using System;
using System.Collections.Generic;

namespace ARK.Core
{
    public static class SampleData
    {
        public static readonly List<string> Symbols = new()
        {
            "ES", "NQ", "YM", "RTY", "CL", "GC", "SI",
            "AAPL", "MSFT", "GOOGL", "AMZN", "TSLA", "NVDA", "META"
        };

        private static readonly Random Rng = new(42);

        public static string RandomSymbol()
        {
            return Symbols[Rng.Next(Symbols.Count)];
        }

        public static double RandomPrice(double basePrice, double spread)
        {
            return basePrice + (Rng.NextDouble() * 2 - 1) * spread;
        }

        public static DateTime RandomDateWithinLastMonths(int months)
        {
            var start = DateTime.Today.AddMonths(-months);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(Rng.Next(range + 1));
        }

        public static int RandomInt(int minInclusive, int maxInclusive)
        {
            return Rng.Next(minInclusive, maxInclusive + 1);
        }

        public static double RandomDouble(double min, double max)
        {
            return min + Rng.NextDouble() * (max - min);
        }
    }
}
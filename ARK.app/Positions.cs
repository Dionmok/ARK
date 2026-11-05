using System.Collections.Generic;

namespace ARK.App
{
    public class Position
    {
        public string Symbol { get; set; } = "";
        public double Size { get; set; }
        public string AccountNumber { get; set; } = "";
        public string OrderType { get; set; } = "";
        public string Status { get; set; } = "";

        public static List<Position> GetSampleData()
        {
            return new List<Position>
            {
                new Position { Symbol = "ES", Size = 2, AccountNumber = "SIM-1001", OrderType = "Market", Status = "Filled" },
                new Position { Symbol = "NQ", Size = -1, AccountNumber = "SIM-1001", OrderType = "Limit", Status = "Filled" },
                new Position { Symbol = "CL", Size = 3, AccountNumber = "SIM-1002", OrderType = "Stop", Status = "Working" },
            };
        }
    }
}
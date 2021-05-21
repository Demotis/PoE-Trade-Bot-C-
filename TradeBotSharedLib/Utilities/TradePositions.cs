using TradeBotSharedLib.Enums;
using TradeBotSharedLib.Models;
using System;
using System.Collections.Generic;

namespace TradeBotSharedLib.Utilities
{
    public static class TradePositions
    {
        static TradePositions()
        {
            TradePositionData = new Dictionary<int, List<Position>>();

            // ToDo: Convert this to a foreach of every resolution in the enum
            // Create the positions for 800600
            Position zeroBlock = new Position { Width = 29, Height = 30, Left = 42, Top = 115 };
            TradePositionData.Add(800600, BuildTradePositions(zeroBlock));

        }

        private static List<Position> BuildTradePositions(Position zeroBlock)
        {
            List<Position> returnList = new List<Position>();
            for (int left = 0; left < 12; left++)
            {
                for (int top = 0; top < 5; top++)
                {
                    Position position = new Position();
                    position.Left = (zeroBlock.Left + (zeroBlock.Width * left));
                    position.Top = (zeroBlock.Top + (zeroBlock.Height * top));
                    position.Width = zeroBlock.Width;
                    position.Height = zeroBlock.Height;
                    returnList.Add(position);
                }
            }
            return returnList;
        }

        public static List<Position> GetPositions(Resolution res)
        {
            int resolutionKey = (int)res;
            if (!TradePositionData.ContainsKey(resolutionKey))
                throw new Exception($"Trade Positions for resolution key {resolutionKey} not found.");
            return TradePositionData[resolutionKey];
        }

        public static Dictionary<int, List<Position>> TradePositionData { get; private set; }
    }
}

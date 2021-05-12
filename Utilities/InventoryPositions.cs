using PoETradeBot.Enums;
using PoETradeBot.Models;
using System;
using System.Collections.Generic;

namespace PoETradeBot.Utilities
{
    public static class InventoryPositions
    {
        static InventoryPositions()
        {
            InventoryPositionData = new Dictionary<int, List<Position>>();

            // ToDo: Convert this to a foreach of every resolution in the enum
            // Create the positions for 800600
            Position zeroBlock = new Position { Width = 27, Height = 27, Left = 441, Top = 328 };
            InventoryPositionData.Add(800600, BuildinventoryPositions(zeroBlock, 2, 2));

        }

        private static List<Position> BuildinventoryPositions(Position zeroBlock, int leftOffset, int topOffset)
        {
            List<Position> returnList = new List<Position>();
            for (int left = 0; left < 12; left++)
            {
                for (int top = 0; top < 5; top++)
                {
                    Position position = new Position();
                    position.Left = (zeroBlock.Left + (zeroBlock.Width * left)) + (leftOffset * left);
                    position.Top = (zeroBlock.Top + (zeroBlock.Height * top)) + (topOffset * top);
                    position.Width = zeroBlock.Width;
                    position.Height = zeroBlock.Height;
                    returnList.Add(position);
                }
            }
            return returnList;
        }

        public static List<Position> GetInvenoryPositions(Resolution res)
        {
            int resolutionKey = (int)res;
            if (!InventoryPositionData.ContainsKey(resolutionKey))
                throw new Exception($"Inventory Positions for resolution key {resolutionKey} not found.");
            return InventoryPositionData[resolutionKey];
        }

        public static Dictionary<int, List<Position>> InventoryPositionData { get; private set; }
    }
}

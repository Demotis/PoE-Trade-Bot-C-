using PoE_Trade_Bot.Enums;
using PoE_Trade_Bot.Models;
using System;
using System.Collections.Generic;

namespace PoE_Trade_Bot.Utilities
{
    public static class StashPositions
    {
        static StashPositions()
        {
            StashPositionData = new Dictionary<int, List<Position>>();

            // ToDo: Convert this to a foreach of every resolution in the enum
            // Create the positions for 800600
            Position zeroBlock = new Position { Width = 29, Height = 29, Left = 9, Top = 75 };
            StashPositionData.Add(800600, BuildStashPositions(zeroBlock, 1, 1));

        }

        private static List<Position> BuildStashPositions(Position zeroBlock, int leftOffset, int topOffset)
        {
            List<Position> returnList = new List<Position>();
            for (int left = 0; left < 12; left++)
            {
                for (int top = 0; top < 12; top++)
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

        public static List<Position> GetStashPositions(Resolution res)
        {
            int resolutionKey = (int)res;
            if (!StashPositionData.ContainsKey(resolutionKey))
                throw new Exception($"Stash Positions for resolution key {resolutionKey} not found.");
            return StashPositionData[resolutionKey];
        }

        public static Dictionary<int, List<Position>> StashPositionData { get; private set; }
    }
}

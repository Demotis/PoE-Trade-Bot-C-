using TradeBotSharedLib.Enums;
using TradeBotSharedLib.Models;
using System;
using System.Collections.Generic;

namespace TradeBotSharedLib.Utilities
{
    public static class StashPositions
    {
        static StashPositions()
        {
            StashPositionData = new Dictionary<int, Position[,]>();

            // ToDo: Convert this to a foreach of every resolution in the enum
            // Create the positions for 800600
            Position zeroBlock = new Position { Width = 29, Height = 29, Left = 9, Top = 75 };
            StashPositionData.Add(800600, BuildStashPositions(zeroBlock, 1, 1));

        }

        private static Position[,] BuildStashPositions(Position zeroBlock, int leftOffset, int topOffset)
        {
            Position[,] returnList = new Position[13, 13]; // position 0,0 will be null
            for (int left = 0; left < 12; left++)
            {
                for (int top = 0; top < 12; top++)
                {
                    Position position = new Position();
                    position.Left = (zeroBlock.Left + (zeroBlock.Width * left)) + (leftOffset * left);
                    position.Top = (zeroBlock.Top + (zeroBlock.Height * top)) + (topOffset * top);
                    position.Width = zeroBlock.Width;
                    position.Height = zeroBlock.Height;
                    returnList[left + 1, top + 1] = position;
                }
            }
            return returnList;
        }

        public static Position[,] GetStashPositions(Resolution res)
        {
            int resolutionKey = (int)res;
            if (!StashPositionData.ContainsKey(resolutionKey))
                throw new Exception($"Stash Positions for resolution key {resolutionKey} not found.");
            return StashPositionData[resolutionKey];
        }

        public static Dictionary<int, Position[,]> StashPositionData { get; private set; }
    }
}

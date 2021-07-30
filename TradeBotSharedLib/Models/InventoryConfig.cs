using TradeBotSharedLib.PoEClient;

namespace TradeBotSharedLib.Models
{
    public class InventoryConfig
    {
        public Position UpperLeft { get; set; }
        public Position LowerRight { get; set; }

        public int XCells { get; set; }
        public int YCells { get; set; }

        public InventoryConfig()
        {
            UpperLeft = new Position();
            LowerRight = new Position();
            XCells = 5;
            YCells = 12;
        }

        /// <summary>
        /// Dumps the inventory by CTRL Click each Cell.
        /// </summary>
        public void DumpInventory()
        {
            if (UpperLeft == null || LowerRight == null)
                return;
            if (!UpperLeft.IsVisible || !LowerRight.IsVisible)
                return;

            int pixelsLeft = (LowerRight.Left - UpperLeft.Left) / (YCells - 1);
            int pixelsTop = (LowerRight.Top - UpperLeft.Top) / (XCells - 1);

            ClientManager.Instance.BringToForeground();
            for (int left = UpperLeft.Left; left < LowerRight.Left; left += pixelsLeft)
                for (int top = UpperLeft.Top; top < LowerRight.Top; top += pixelsTop)
                {
                    Position tmppos = new Position() { Left = left, Top = top };
                    ClientManager.Instance.CtrlClickPosition(ClientManager.Instance.TranslatePosition(tmppos));
                }

        }

    }
}

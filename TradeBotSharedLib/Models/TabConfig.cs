using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using TradeBotSharedLib.PoEClient;
using TradeBotSharedLib.Utilities;

namespace TradeBotSharedLib.Models
{
    public class TabConfig
    {
        public List<Position> OpenSteps { get; set; }
        public string TabName { get; set; }

        [JsonIgnore]
        public Bitmap TabTitle { get; set; }
        public string TabTitleBase64
        {
            get { return TabTitle.ToBase64String(ImageFormat.Png); }
            set { TabTitle = value.Base64StringToBitmap(); }
        }

        public bool IsQuadTab { get; set; }
        public Position UpperLeft { get; set; }
        public Position LowerRight { get; set; }

        public int XCells { get; set; }
        public int YCells { get; set; }


        public TabConfig()
        {
            OpenSteps = new List<Position>();
            UpperLeft = new Position();
            LowerRight = new Position();
            XCells = 12;
            YCells = 12;

        }

        public void OpenTab()
        {
            if (!ClientManager.Instance.OpenStash())
                return;

            foreach (Position clickLocation in OpenSteps)
                ClientManager.Instance.ClickPosition(ClientManager.Instance.TranslatePosition(clickLocation));
        }

        public void GetItem(int left, int top)
        {
            if (UpperLeft == null || LowerRight == null)
                return;
            if (!UpperLeft.IsVisible || !LowerRight.IsVisible)
                return;

            int realCellsX = IsQuadTab ? XCells * 4 : XCells;
            int realCellsY = IsQuadTab ? YCells * 4 : YCells;

            int pixelsLeft = (LowerRight.Left - UpperLeft.Left) / (realCellsY - 1);
            int pixelsTop = (LowerRight.Top - UpperLeft.Top) / (realCellsX - 1);

            Position tmppos = new Position()
            {
                Left = UpperLeft.Left + (pixelsLeft * (left - 1)),
                Top = UpperLeft.Top + (pixelsTop * (top - 1))
            };
            ClientManager.Instance.CtrlClickPosition(ClientManager.Instance.TranslatePosition(tmppos));
        }
    }
}

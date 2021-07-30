using System;
using System.Drawing;
using System.Windows.Forms;
using TradeBotSharedLib.PoEClient;

namespace POETradeBotSyncfusionUI
{
    public partial class GetImage : Form
    {
        private int xDown;
        private int yDown;
        private int xUp;
        private int yUp;
        private Rectangle rectCropArea;

        public Bitmap Asset { get; set; }
        public GetImage()
        {
            InitializeComponent();
        }

        private void GetImage_Load(object sender, EventArgs e)
        {
            Asset = ClientManager.Instance.GetClientScreenShot();
            // Setup the image display size to be a relative of the screen size
            int wBorderPadding = this.Width - AssetDisplay.Width;
            int hBorderPadding = this.Height - AssetDisplay.Height;
            this.Width = Asset.Width / 2 + wBorderPadding;
            this.Height = Asset.Height / 2 + hBorderPadding;
            AssetDisplay.Image = Asset;
        }

        private void AssetDisplay_MouseDown(object sender, MouseEventArgs e)
        {
            AssetDisplay.Invalidate();

            xDown = e.X;
            yDown = e.Y;
        }

        private void AssetDisplay_MouseUp(object sender, MouseEventArgs e)
        {
            xUp = e.X;
            yUp = e.Y;

            Rectangle rec = new Rectangle(xDown, yDown, Math.Abs(xUp - xDown), Math.Abs(yUp - yDown));

            using (Pen pen = new Pen(Color.YellowGreen, 1))
            {

                AssetDisplay.CreateGraphics().DrawRectangle(pen, rec);
            }

            xDown = xDown * AssetDisplay.Image.Width / AssetDisplay.Width;
            yDown = yDown * AssetDisplay.Image.Height / AssetDisplay.Height;

            xUp = xUp * AssetDisplay.Image.Width / AssetDisplay.Width;
            yUp = yUp * AssetDisplay.Image.Height / AssetDisplay.Height;

            rectCropArea = new Rectangle(xDown, yDown, Math.Abs(xUp - xDown), Math.Abs(yUp - yDown));
            CropImage();
        }

        private void CropImage()
        {
            try
            {
                using (Bitmap bmp = new Bitmap(Asset))
                {
                    var newImg = bmp.Clone(
                        rectCropArea,
                        bmp.PixelFormat);
                    Asset = newImg;
                }

                // Setup the image display size to be a relative of the image size
                int wBorderPadding = this.Width - AssetDisplay.Width;
                int hBorderPadding = this.Height - AssetDisplay.Height;

                int scale = AssetDisplay.Width / Asset.Width;
                if (Asset.Height > Asset.Width)
                    scale = AssetDisplay.Height / Asset.Height;

                this.Width = Asset.Width * scale + wBorderPadding;
                this.Height = Asset.Height * scale + hBorderPadding;
                AssetDisplay.Image = Asset;
            }
            catch (Exception ex)
            {

            }
        }

        private void GetImage_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
    }
}

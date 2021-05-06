﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace PoE_Trade_Bot.Services
{
    /// <summary>
    /// Provides functions to capture the entire screen, or a particular window, and save it to a file.
    /// </summary>
    public class ScreenCapture
    {
        public static Bitmap CaptureRectangle(Rectangle rect)
        {
            return CaptureRectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static Bitmap CaptureRectangle(int x, int y, int rec_width, int rec_height)
        {
            Rectangle rect = new Rectangle(x, y, rec_width, rec_height);
            Bitmap bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppRgb);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);

            if (Convert.ToBoolean(ConfigManager.Instance.ApplicationConfig["SaveTestImages"]))
            {
                string guid = Guid.NewGuid().ToString();
                bmp.Save(ConfigManager.Instance.ApplicationConfig["TestImagePath"] + guid + ".png");
            }

            g.Dispose();
            return bmp;
        }

        public static Bitmap CaptureScreen()
        {
            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppRgb);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(0, 0, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);

            if (Convert.ToBoolean(ConfigManager.Instance.ApplicationConfig["SaveTestImages"]))
            {
                string guid = Guid.NewGuid().ToString();
                bmp.Save(ConfigManager.Instance.ApplicationConfig["TestImagePath"] + guid + ".png");
            }

            g.Dispose();
            return bmp;
        }
    }
}
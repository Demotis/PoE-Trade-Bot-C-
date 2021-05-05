using System;

namespace PoE_Trade_Bot.Models
{
    public class Position
    {
        public int Left { get; set; } = -1;

        public int Top { get; set; } = -1;

        public int Width { get; set; }

        public int Height { get; set; }

        public bool IsVisible
        {
            get
            {
                if (Left >= 0 && Top >= 0)
                    return true;
                else
                    return false;
            }
        }

        public int ClickTargetX
        {
            get
            {
                return (int)Math.Floor(Left + (double)(Width / 2));
            }
        }
        public int ClickTargetY
        {
            get
            {
                return (int)Math.Floor(Top + (double)(Height / 2));
            }
        }
    }
}

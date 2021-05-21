using System.Collections.Generic;

namespace TradeBotSharedLib.Models.Test
{
    public class Item
    {
        public List<Cell> Places { get; set; }

        public string Name { get; set; }

        public string RealName { get; set; }

        public Price Price { get; set; }

        public double ChaosValue { get; set; }

        public int SizeInStack { get; set; } = 0;

        public int StackSize { get; set; } = 0;

        public Item()
        {
            Places = new List<Cell>();
        }
    }

    public class Price
    {
        public double Cost { get; set; } = -1;

        public int ForNumberItems { get; set; }

        public Currency_ExRate CurrencyType { get; set; } = null;

        public bool IsSet
        {
            get
            {
                if (Cost != -1 && CurrencyType != null)
                    return true;
                else
                    return false;
            }
        }

    }
}

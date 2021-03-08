using System.Collections.Generic;

namespace PoE_Trade_Bot.PoEBotV2.Models
{
    public class Product
    {
        public List<Item> Items { get; set; }

        public ItemType Type { get; set; }
        
        public Price Price { get; set; }

        public int Available => Items.Count;
    }
}
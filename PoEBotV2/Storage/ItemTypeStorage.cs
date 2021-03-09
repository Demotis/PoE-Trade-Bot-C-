using System.Collections.Generic;
using PoE_Trade_Bot.PoEBotV2.Models;

namespace PoE_Trade_Bot.PoEBotV2.Storage
{
    public static class ItemTypeStorage
    {
        private static readonly List<ItemType> Types = new List<ItemType>();

        public static ItemType FindByName(string name)
        {
            return Types.Find(type => type.Name == name);
        }

        public static void Add(ItemType itemType)
        {
            Types.Add(itemType);
        }
        
    }
}
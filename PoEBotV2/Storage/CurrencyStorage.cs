using System.Collections.Generic;
using PoE_Trade_Bot.PoEBotV2.Models;

namespace PoE_Trade_Bot.PoEBotV2.Storage
{
    public static class CurrencyStorage
    {
        private static readonly List<Currency> Currencies = new List<Currency>();

        public static Currency GetCurrencyByTypeName(string typeName)
        {
            return Currencies.Find(currency => currency.ItemType.Name == typeName);
        }
    }
}
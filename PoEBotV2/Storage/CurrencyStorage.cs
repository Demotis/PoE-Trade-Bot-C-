﻿using System.Collections.Generic;
using PoE_Trade_Bot.PoEBotV2.Models;

namespace PoE_Trade_Bot.PoEBotV2.Storage
{
    public static class CurrencyStorage
    {
        private static readonly List<Currency> Currencies = new List<Currency>();

        public static Currency FindCurrencyByTypeName(string typeName)
        {
            return Currencies.Find(currency => currency.ItemType.Name == typeName);
        }
        
        public static Currency FindCurrencyByType(ItemType type)
        {
            return Currencies.Find(currency => currency.ItemType == type);
        }

        public static void Add(Currency currency)
        {
            Currencies.Add(currency);
        }
    }
}
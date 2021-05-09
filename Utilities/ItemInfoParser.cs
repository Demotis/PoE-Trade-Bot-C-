using PoE_Trade_Bot.Models.Test;
using System;
using System.Text.RegularExpressions;

namespace PoE_Trade_Bot.Utilities
{
    public class ItemInfoParser
    {
        public string RawInfo { get; private set; }
        public Item Item { get; private set; }

        public ItemInfoParser()
        {
            ProcessRawInfo("empty_string");
        }

        public ItemInfoParser(string rawInfo)
        {
            ProcessRawInfo(rawInfo);
        }

        private void ProcessRawInfo(string rawInfo)
        {
            RawInfo = rawInfo;
            Item = new Item();
            Item.Price = GetPrice();
            if (Item.Price.Cost == -1)
                Item.Name = "Not For Sell";
            else
                Item.Name = GetNameItem();
            Item.StackSize = GetStackSize();

            if (Item.StackSize != 1)
                Item.SizeInStack = (int)GetSizeInStack();
        }

        public void AddPlace(int ClickTargetX, int ClickTargetY)
        {
            Item.Places.Add(new Cell(ClickTargetX, ClickTargetY));
        }

        private double GetSizeInStack()
        {
            if (!string.IsNullOrEmpty(RawInfo) && RawInfo != "empty_string")
            {
                int begin = RawInfo.IndexOf("Stack Size: ") + 12;
                int length = RawInfo.IndexOf("/") - begin;

                return Convert.ToDouble(RawInfo.Substring(begin, length));
            }
            return 0;
        }


        private string GetNameItem()
        {
            if (RawInfo.Contains("Rarity: Currency"))
            {
                string str = Regex.Match(RawInfo, @"Rarity: Currency\s([\w ']+)").Groups[1].Value;
                return str;
            }

            if (RawInfo.Contains("Map Tier:"))
            {
                if (RawInfo.Contains("Rarity: Rare"))
                {
                    var match = Regex.Match(RawInfo, @"Rarity: Rare\s([\w ']*)\s([\w ']*)");

                    if (!string.IsNullOrEmpty(match.Groups[2].Value))
                    {
                        return match.Groups[2].Value.Replace(" Map", "");
                    }
                    else
                        return match.Groups[1].Value.Replace(" Map", "");
                }

                if (RawInfo.Contains("Rarity: Normal"))
                {
                    return Regex.Match(RawInfo, @"Rarity: Normal\s([\w ']*)").Groups[1].Value.Replace(" Map", "");
                }

                if (RawInfo.Contains("Rarity: Unique"))
                {
                    var match = Regex.Match(RawInfo, @"Rarity: Unique\s([\w ']*)\s([\w ']*)");

                    if (!string.IsNullOrEmpty(match.Groups[2].Value))
                    {
                        return $"{match.Groups[1].Value} {match.Groups[2].Value}".Replace(" Map", "");
                    }
                    else
                        return "Undefined item";
                }

            }

            if (RawInfo.Contains("Rarity: Divination Card"))
            {
                return Regex.Match(RawInfo, @"Rarity: Divination Card\s([\w ']*)").Groups[1].Value;
            }

            //I think that it for predicate fragments
            if (!RawInfo.Contains("Requirements:"))
            {
                if (RawInfo.Contains("Rarity: Normal"))
                {
                    return Regex.Match(RawInfo, @"Rarity: Normal\s([\w ']*)").Groups[1].Value;
                }
            }

            // Get Generic Name
            string[] infoParts = RawInfo.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string itemName = string.Empty;
            foreach (string line in infoParts)
            {
                if (line.Contains(":"))
                    continue;
                if (line.Contains("--------"))
                    break;
                itemName = $"{itemName} {line}";
            }
            return itemName.Trim();
        }

        private Price GetPrice()
        {
            Price price = new Price();

            if (!RawInfo.Contains("Note: ~price") && !RawInfo.Contains("Note: ~b/o"))
                return new Price();

            if (Regex.IsMatch(RawInfo, "~b/o [0-9.]+/[0-9.]+"))
            {
                price.Cost = Convert.ToDouble(Regex.Replace(RawInfo, @"([\w\s\W\n]+Note: ~b/o )|(/+[\w\s\W]*)|([^0-9.])", ""));
                price.ForNumberItems = Convert.ToInt32(Regex.Replace(RawInfo, @"([\w\s\W]+/)|([^0-9.])", ""));
                price.CurrencyType = PoECurrencyManager.Instance.Currencies.GetCurrencyByName(Regex.Replace(RawInfo, @"[\w\s\W]+\d+\s|\n", ""));
            }
            else if (Regex.IsMatch(RawInfo, @"~b/o +[0-9.]+\s\D*"))
            {
                price.Cost = Convert.ToDouble(Regex.Replace(RawInfo, @"[\w\W]*~b/o |[^0-9.]*", "").Replace('.', ','));
                price.ForNumberItems = GetStackSize();
                price.CurrencyType = PoECurrencyManager.Instance.Currencies.GetCurrencyByName(Regex.Replace(RawInfo, @"[\w\s\W]+\d+\s|\n", ""));
            }
            else if (Regex.IsMatch(RawInfo, "~price [0-9.]+/[0-9.]+"))
            {
                price.Cost = Convert.ToDouble(Regex.Replace(RawInfo, @"([\w\s\W\n]+Note: ~price )|(/+[\w\s\W]*)|([^0-9.])", ""));
                price.ForNumberItems = Convert.ToInt32(Regex.Replace(RawInfo, @"([\w\s\W]+/)|([^0-9.])", ""));
                price.CurrencyType = PoECurrencyManager.Instance.Currencies.GetCurrencyByName(Regex.Replace(RawInfo, @"[\w\s\W]+\d+\s|\n", ""));
            }
            else if (Regex.IsMatch(RawInfo, @"~price +[0-9.]+\s\D*"))
            {
                price.Cost = Convert.ToDouble(Regex.Replace(RawInfo, @"[\w\W]*~price |[^0-9.]*", "").Replace('.', ','));
                price.ForNumberItems = GetStackSize();
                price.CurrencyType = PoECurrencyManager.Instance.Currencies.GetCurrencyByName(Regex.Replace(RawInfo, @"[\w\s\W]+\d+\s|\n", ""));
            }

            if (!price.IsSet)
                return new Price();
            else
                return price;
        }

        private int GetStackSize()
        {
            if (!RawInfo.Contains("Stack Size:"))
                return 1;

            int res = Convert.ToInt32(Regex.Match(RawInfo, @"Stack Size: [0-9.]+/([0-9.]+)").Groups[1].Value);

            return res;
        }
    }
}

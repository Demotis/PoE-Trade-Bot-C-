using System;

namespace PoETradeBot.Models
{
    public class CustomerInfo
    {
        public enum TradeStatuses
        {
            STARTED, ACCEPTED, CANCELED
        }

        public enum OrderTypes
        {
            ITEM,
            CURRENCY,
            API
        }

        public enum ApiActions
        {
            NONE,
            GET,
            TAKE,
            PUT
        }

        public string Nickname { get; set; }

        public string Product { get; set; }

        public int NumberProducts { get; set; }

        public double Cost { get; set; }

        public Currency_ExRate CurrencyType { get; set; }

        public string StashTab { get; set; }

        public string StashTabNormal => $"{StashTab.Replace(" ", "")}_tab";

        public int Left { get; set; }

        public int Top { get; set; }

        public bool IsReady
        {
            get
            {
                if (OrderType == OrderTypes.ITEM)
                {
                    if (Nickname != null && Product != null && Cost > 0 && StashTab != null && CurrencyType != null)
                    {
                        return true;
                    }
                    else
                        return false;
                }
                else
                {
                    if (!string.IsNullOrEmpty(Nickname) && !String.IsNullOrEmpty(Product) && Cost > 0 && NumberProducts >= 0 && CurrencyType != null)
                    {
                        return true;
                    }
                    else
                        return false;
                }

            }
        }

        public TradeStatuses TradeStatus { get; set; }

        public double Chaos_Price { get; set; }

        public string Item_PoE_Info;

        public bool IsInArea { get; set; }

        public OrderTypes OrderType { get; set; }

        public ApiActions ApiAction { get; set; }

        public override string ToString()
        {
            if (OrderType == OrderTypes.ITEM)
            {
                return $"\nNickname: <{Nickname}> \n" +
                $"Order Typer: <{OrderType}> \n" +
                $"Prodcut: <{Product}> \n" +
                $"Price: <{Cost}> \n" +
                $"Currency: <{CurrencyType.Name}> \n" +
                $"Stash Tab: <{StashTab}> \n" +
                $"Left: <{Left}> \n" +
                $"Top: <{Top}>\n";
            }
            else
            {
                return $"\nNickname: <{Nickname}> \n" +
                $"Order Typer: <{OrderType}> \n" +
                $"Prodcut: <{Product}> \n" +
                $"Number Products: <{NumberProducts}> \n" +
                $"Cost: <{Cost}> \n" +
                $"Currency: <{CurrencyType.Name}> \n" +
                $"Left: <{Left}> \n" +
                $"Top: <{Top}>\n";
            }
        }
    }
}

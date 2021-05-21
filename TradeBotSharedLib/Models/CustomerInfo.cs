using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TradeBotSharedLib.Models
{
    public class CustomerInfo : INotifyPropertyChanged
    {
        public CustomerInfo()
        {
            CurrencyType = new Currency_ExRate();

        }

        public enum TradeStatuses
        {
            NONE, STARTED, ACCEPTED, CANCELED
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

        public string _stashTab { get; set; }
        public string StashTab { get { return string.IsNullOrWhiteSpace(_stashTab) ? "" : _stashTab; } set { _stashTab = value; } }

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

        private TradeStatuses _tradeStatus { get; set; }
        public TradeStatuses TradeStatus
        {
            get
            {
                return _tradeStatus;
            }
            set
            {
                if (value != this._tradeStatus)
                {
                    this._tradeStatus = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double Chaos_Price { get; set; }

        public string Item_PoE_Info;

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isInArea { get; set; }
        public bool IsInArea
        {
            get
            {
                return _isInArea;
            }
            set
            {
                if (value != this._isInArea)
                {
                    this._isInArea = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public OrderTypes OrderType { get; set; }

        public ApiActions ApiAction { get; set; }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            if (OrderType == OrderTypes.ITEM)
            {
                return $"Nickname: {Nickname}\r\n" +
                $"Product: {Product}\r\n" +
                $"Price: {Cost}\r\n" +
                $"Currency: {CurrencyType.Name}\r\n" +
                $"Stash Tab: {StashTab}\r\n" +
                $"Left: {Left}\r\n" +
                $"Top: {Top}\r\n";
            }
            else
            {
                return $"Nickname: {Nickname}\r\n" +
                $"Product: {Product}\r\n" +
                $"Quantity: {NumberProducts}\r\n" +
                $"Cost: {Cost}\r\n" +
                $"Currency: {CurrencyType.Name}\r\n";
            }
        }
    }
}

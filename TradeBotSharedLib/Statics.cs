using System.ComponentModel;
using TradeBotSharedLib.Models;

namespace TradeBotSharedLib
{
    public static class Statics
    {
        public static BindingList<CustomerInfo> CustomerQueue = new BindingList<CustomerInfo>();
        public static BindingList<CustomerInfo> CompletedTrades = new BindingList<CustomerInfo>();
    }
}

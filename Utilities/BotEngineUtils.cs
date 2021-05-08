using PoE_Trade_Bot.Models;
using PoE_Trade_Bot.PoEClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE_Trade_Bot.Utilities
{
    public static class BotEngineUtils
    {
        public static bool KickFromParty(CustomerInfo customer)
        {
            return ClientManager.Instance.ChatCommand($"/kick {customer.Nickname}");
        }
    }
}

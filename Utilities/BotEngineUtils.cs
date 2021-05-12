using PoETradeBot.Models;
using PoETradeBot.PoEClient;
using System.Threading;

namespace PoETradeBot.Utilities
{
    public static class BotEngineUtils
    {
        public static bool InviteCustomer(CustomerInfo customer, int secondsToWait = 30)
        {
            ClientManager.Instance.ChatCommand($"/invite {customer.Nickname}");
            return CheckAreaForCustomer(customer, secondsToWait);
        }

        public static bool CheckAreaForCustomer(CustomerInfo customer, int secondsToWait = 40)
        {
            int halfSeconds = secondsToWait * 2;
            for (int i = 0; i < halfSeconds; i++)
            {
                if (customer.IsInArea)
                {
                    Thread.Sleep(1000);
                    return true;
                }
                Thread.Sleep(500);
            }
            Logger.Console.Warn("Customer arrival timeout expired");
            return false;
        }

        public static bool KickFromParty(CustomerInfo customer)
        {
            return ClientManager.Instance.ChatCommand($"/kick {customer.Nickname}");
        }
    }
}

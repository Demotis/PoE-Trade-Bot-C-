using System.Threading.Tasks;
using PoE_Trade_Bot.PoEBotV2;
using PoE_Trade_Bot.PoEBotV2.Services;
using PoEBotV2;

namespace PoE_Trade_Bot
{

    class Program
    {
        public static void Main(string[] args)
        {
            LogReader logReader = new LogReader(@"D:/test/");

            PoELogManager logManager = new PoELogManager(logReader);
            
            Bot bot = new Bot(logManager);

            Task.WaitAll(bot.StartAsync());
        }
    }
}

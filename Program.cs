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
            Bot bot = new Bot(logReader);

            Task.WaitAll(bot.StartAsync());
        }
    }
}

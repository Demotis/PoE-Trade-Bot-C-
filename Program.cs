using System.Linq;
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
            var path = args.ElementAtOrDefault(0) ?? @"D:/test/";

            LogReader logReader = new LogReader(path);

            PoELogParser logParser = new PoELogParser();

            PoELogManager logManager = new PoELogManager(logReader, logParser);

            Bot bot = new Bot(logManager);

            Task.WaitAll(bot.StartAsync());
        }
    }
}
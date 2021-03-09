using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using PoE_Trade_Bot.PoEBotV2;
using PoE_Trade_Bot.PoEBotV2.Services;

namespace PoE_Trade_Bot
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var path = args.ElementAtOrDefault(0) ?? @"D:/test/";

            HttpClient client = new HttpClient();

            CurrencyManager currencyManager = new CurrencyManager(client);

            LogReader logReader = new LogReader(path);

            PoELogParser logParser = new PoELogParser();

            PoELogManager logManager = new PoELogManager(logReader, logParser);

            Bot bot = new Bot(logManager, currencyManager);

            await bot.StartAsync();
        }
    }
}
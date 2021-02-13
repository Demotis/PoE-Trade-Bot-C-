using System.Threading.Tasks;
using PoEBotV2;
using PoEBotV2.Services;

namespace PoE_Trade_Bot
{
    
    class Program
    {
        public static async Task Main(string[] args)
        {
            Bot bot = new Bot();

            LogReader logReader = new LogReader();
            await logReader.startAsync("D:\\test\\");
        }
    }
}

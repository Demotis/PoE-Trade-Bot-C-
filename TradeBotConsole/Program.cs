using System;

namespace PoETradeBot
{

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var BotEngine = new BotEngine();
            BotEngine.StartBot();


        }
    }
}

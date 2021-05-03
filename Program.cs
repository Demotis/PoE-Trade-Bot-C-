using System;

namespace PoE_Trade_Bot
{

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var Bot_Engine = new BotEngine();
            Bot_Engine.StartBot();

        }
    }
}

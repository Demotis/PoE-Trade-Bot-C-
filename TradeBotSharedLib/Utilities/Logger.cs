using log4net;

namespace TradeBotSharedLib.Utilities
{
    public static class Logger
    {
        public static ILog Application => Get("Application");
        public static ILog Console => Get("Console");


        public static ILog Get(string name = "")
        {
            return LogManager.GetLogger(name);
        }
    }
}

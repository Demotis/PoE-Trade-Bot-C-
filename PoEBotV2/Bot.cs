using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoE_Trade_Bot.PoEBotV2.Interfaces;

namespace PoE_Trade_Bot.PoEBotV2
{
    public class Bot
    {
        public ILogReader LogReader { get; }

        private List<string> logs;

        public Bot(ILogReader logReader)
        {
            LogReader = logReader;
            logs = new List<string>();
        }

        public async Task StartAsync()
        {
            await LogReader.StartAsync(OnEndRead);
        }

        private void OnEndRead(List<string> result)
        {
            Console.WriteLine(String.Join("\n", result));
            logs.AddRange(result);
        }
    }

}
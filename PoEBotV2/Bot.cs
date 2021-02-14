using PoEBotV2.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoEBotV2
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
            await LogReader.StartAsync("D:\\test\\", OnEndRead);
        }

        private void OnEndRead(List<string> result)
        {
            Console.WriteLine(String.Join("\n", result));
            logs.AddRange(result);
        }
    }

}
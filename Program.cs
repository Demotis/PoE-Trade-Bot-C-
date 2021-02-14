using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using PoEBotV2;
using PoEBotV2.Services;

namespace PoE_Trade_Bot
{

    class Program
    {
        public static void Main(string[] args)
        {
            Bot bot = new Bot();

            LogReader logReader = new LogReader();

            var logReaderTask = logReader.StartAsync("D:\\test\\", (List<string> results) =>
            {
                Console.WriteLine(String.Join("\n", results));
            });

            Console.WriteLine("Cont");

            Task.WaitAll(logReaderTask);
        }
    }
}

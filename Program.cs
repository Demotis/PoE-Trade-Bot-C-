﻿using System;
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
            LogReader logReader = new LogReader();
            Bot bot = new Bot(logReader);

            Task.WaitAll(bot.StartAsync());
        }
    }
}

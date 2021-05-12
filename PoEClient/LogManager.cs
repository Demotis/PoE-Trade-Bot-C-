using Microsoft.Win32;
using PoETradeBot.Models;
using PoETradeBot.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PoETradeBot.PoEClient
{
    public sealed class LogManager : IDisposable
    {
        private static readonly LogManager instance = new LogManager();
        private bool disposedValue;
        public static LogManager Instance => instance;
        private System.Timers.Timer _timer;
        private long _lastIndex { get; set; }
        private bool _processingLog { get; set; }

        private string _logsDir;
        private string _logFile;


        static LogManager()
        {
        }

        private LogManager()
        {
            var path = Registry.GetValue(@"HKEY_CURRENT_USER\Software\GrindingGearGames\Path of Exile", "InstallLocation", null);
            if (path != null)
            {
                path = path.ToString();
                _logsDir = path + @"logs\";
                _logFile = _logsDir + @"\Client.txt";
            }
        }

        public void StartService()
        {
            _lastIndex = GetLineCount();
            _timer = new System.Timers.Timer();
            _timer.Interval = 1000;
            _timer.Elapsed += ReadLogs;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private long GetLineCount()
        {
            using (var fs = new FileStream(_logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                return fs.CountLines();
        }

        private void ReadLogs(object source, System.Timers.ElapsedEventArgs e)
        {
            if (_processingLog)
                return;
            _processingLog = true;
            long currentLine = 0;

            using (var fs = new FileStream(_logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    currentLine++;
                    if (_lastIndex >= currentLine)
                        continue;
                    if (line.Contains("bad [INFO Client"))
                        ProcessLogLine(line);
                    _lastIndex++;
                }

            }
            _processingLog = false;
        }

        private void ProcessLogLine(string logLine)
        {
            Logger.Console.Debug(logLine);

            //GetFullInfoCustomer
            try
            {
                if (logLine.Contains(ConfigManager.Instance.ApplicationConfig["ApiKey"]) && logLine.Contains("@From"))
                {
                    // Do we have an action?
                    if (!logLine.Contains("A:") ||
                        !logLine.Contains(":A"))
                        return;

                    var cus_inf = new CustomerInfo();

                    cus_inf.OrderType = CustomerInfo.OrderTypes.API;

                    int length;
                    int begin;

                    //Nickname
                    if (!logLine.Contains("> "))
                    {
                        begin = logLine.IndexOf("@From ") + 6;
                        length = logLine.IndexOf(": ") - begin;
                        cus_inf.Nickname = logLine.Substring(begin, length);
                    }
                    else
                    {
                        begin = logLine.IndexOf("> ") + 2;
                        length = logLine.IndexOf(": ") - begin;
                        cus_inf.Nickname = logLine.Substring(begin, length);
                    }

                    // Action
                    begin = logLine.IndexOf("A:") + 2;
                    length = logLine.IndexOf(":A") - begin;
                    string action = logLine.Substring(begin, length);

                    switch (action)
                    {
                        case "take":
                            // Do we have an currency?
                            if (!logLine.Contains("C:") ||
                                !logLine.Contains(":C"))
                                return;
                            cus_inf.ApiAction = CustomerInfo.ApiActions.TAKE;
                            begin = logLine.IndexOf("C:") + 2;
                            length = logLine.IndexOf(":C") - begin;
                            cus_inf.Product = logLine.Substring(begin, length);
                            break;
                        case "get":
                            cus_inf.ApiAction = CustomerInfo.ApiActions.GET;
                            break;
                        case "put":
                            return;
                            break;
                    }
                    BotEngine.CustomerQueue.Add(cus_inf);
                    return;
                }

                if (logLine.Contains("Hi, I would like to buy your") && logLine.Contains("@From"))
                {
                    var cus_inf = new CustomerInfo();

                    cus_inf.OrderType = CustomerInfo.OrderTypes.ITEM;

                    int length;
                    int begin;
                    //Nickname

                    if (!logLine.Contains("> "))
                    {
                        begin = logLine.IndexOf("@From ") + 6;
                        length = logLine.IndexOf(": ") - begin;
                        cus_inf.Nickname = logLine.Substring(begin, length);
                    }
                    else
                    {
                        begin = logLine.IndexOf("> ") + 2;
                        length = logLine.IndexOf(": ") - begin;
                        cus_inf.Nickname = logLine.Substring(begin, length);
                    }


                    //Product
                    begin = logLine.IndexOf("your ") + 5;
                    length = logLine.IndexOf(" listed") - begin;
                    cus_inf.Product = logLine.Substring(begin, length);

                    //Currency
                    begin = logLine.IndexOf(" in") - 1;
                    for (int i = 0; i < 50; i++)
                    {
                        if (logLine[begin - i] == ' ')
                        {
                            begin = begin - i + 1;
                            break;
                        }
                    }
                    length = logLine.IndexOf(" in") - begin;
                    cus_inf.CurrencyType = PoECurrencyManager.Instance.Currencies.GetCurrencyByName(logLine.Substring(begin, length));

                    //Price
                    begin = logLine.IndexOf("for ") + 4;
                    cus_inf.Cost = GetNumber(begin, logLine);

                    //Stash Tab
                    begin = logLine.IndexOf("tab \"") + 5;
                    length = logLine.IndexOf("\"; position") - begin;
                    cus_inf.StashTab = logLine.Substring(begin, length);

                    //left
                    begin = logLine.IndexOf("left ") + 5;
                    cus_inf.Left = (int)GetNumber(begin, logLine);

                    //top
                    begin = logLine.IndexOf("top ") + 4;
                    cus_inf.Top = (int)GetNumber(begin, logLine);

                    //to chaos chaosequivalent
                    cus_inf.Chaos_Price = cus_inf.CurrencyType.ChaosEquivalent * cus_inf.Cost;

                    //trade accepted
                    cus_inf.TradeStatus = CustomerInfo.TradeStatuses.STARTED;

                    if (cus_inf.IsReady)
                    {
                        BotEngine.CustomerQueue.Add(cus_inf);
                        Logger.Console.Info(cus_inf.ToString());
                    }
                }

                if (logLine.Contains("I'd like to buy your") && logLine.Contains("@From"))
                {
                    var cus = new CustomerInfo();

                    cus.OrderType = CustomerInfo.OrderTypes.CURRENCY;

                    cus.Nickname = Regex.Replace(logLine, @"([\w\s\W]+@From )|(: [\w\W\s]*)|(<[\w\W\s]+> )", "");

                    cus.Product = Regex.Replace(logLine, @"([\w\W]+your +[\d,]* )|( for+[\w\s\W]*)|( Map [()\d\w]+)", "");

                    string test = Regex.Match(logLine, @"your ([\d]+)").Groups[1].Value;

                    cus.NumberProducts = Convert.ToInt32(test);

                    cus.Cost = Convert.ToDouble(Regex.Replace(logLine, @"([\s\w\W]+for my )|([\D])", "").Replace(".", ","));

                    cus.CurrencyType = PoECurrencyManager.Instance.Currencies.GetCurrencyByName(Regex.Replace(logLine, @"([\w\s\W]+my +[\d,.]* )|( in +[\w\W\s]*)", ""));

                    if (cus.IsReady)
                    {
                        BotEngine.CustomerQueue.Add(cus);
                        Logger.Console.Info(cus.ToString());
                    }

                }
            }
            catch (Exception e)
            {
                Logger.Application.Error(e.Message, e);
            }

            //check area
            if (BotEngine.CustomerQueue.Any())
            {
                if (logLine.Contains(BotEngine.CustomerQueue.First().Nickname) && logLine.Contains("has joined the area"))
                {
                    Logger.Console.Info($"Customer has arrived {BotEngine.CustomerQueue.First().Nickname}");
                    BotEngine.CustomerQueue.First().IsInArea = true;
                }

                if (logLine.Contains("Player not found in this area."))
                {
                    BotEngine.CustomerQueue.First().IsInArea = false;
                }

                if (logLine.Contains(": Trade accepted."))
                {
                    BotEngine.CustomerQueue.First().TradeStatus = CustomerInfo.TradeStatuses.ACCEPTED;
                }

                if (logLine.Contains(": Trade cancelled."))
                {
                    BotEngine.CustomerQueue.First().TradeStatus = CustomerInfo.TradeStatuses.CANCELED;
                }
            }

            if (logLine.Contains("AFK mode is now ON. Autoreply"))
                ClientManager.Instance.IsAFK = true;
            if (logLine.Contains("AFK mode is now OFF"))
                ClientManager.Instance.IsAFK = false;
        }

        private double GetNumber(int begin, string target)
        {
            double result = 0;
            string buf = string.Empty;

            for (int i = begin; i < begin + 5; i++)
            {
                if (target[i] != ' ' && target[i] != ')')
                {
                    if (target[i] != '.')
                        buf += target[i];
                    else buf += ',';
                }
                else
                {
                    begin = i + 1;
                    break;
                }
            }

            return result = Convert.ToDouble(buf);
        }



        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_timer != null)
                    {
                        if (_timer.Enabled)
                            _timer.Stop();
                        _timer.Elapsed -= ReadLogs;
                        _timer = null;
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

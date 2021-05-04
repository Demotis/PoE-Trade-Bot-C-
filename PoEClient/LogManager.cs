using Microsoft.Win32;
using PoE_Trade_Bot.Models;
using PoE_Trade_Bot.Services;
using PoE_Trade_Bot.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoE_Trade_Bot.PoEClient
{
    public sealed class LogManager : IDisposable
    {
        private static readonly LogManager instance = new LogManager();
        private bool disposedValue;
        public static LogManager Instance => instance;

        private Task ReadLogs;

        private string path;
        private string logsDir;
        private string logFile;
       

        static LogManager()
        {
        }

        private LogManager()
        {
            var path = Registry.GetValue(@"HKEY_CURRENT_USER\Software\GrindingGearGames\Path of Exile", "InstallLocation", null);
            if (path != null)
            {
                path = path.ToString();
                logsDir = path + @"logs\";
                logFile = logsDir + @"\Client.txt";
            }

            ReadLogs = new Task(ReadLogsInBack);
            ReadLogs.Start();
        }

        private void ReadLogsInBack()
        {
            Win32.FocusPoEWindow();

            int last_index = -1;
            bool not_first = false;

            while (true)
            {
                var fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (var sr = new StreamReader(fs))
                {
                    int li = 0;
                    string ll = string.Empty;
                    while (!sr.EndOfStream)
                    {
                        li++;
                        ll = sr.ReadLine();

                        if (not_first && li > last_index)
                        {
                            if (ll.Contains("bad [INFO Client"))
                            {
                                Logger.Console.Debug(ll);
                                ProcessLogLine(ll);
                            }
                        }
                    }

                    sr.Dispose();
                    fs.Dispose();

                    if (li > last_index)
                    {
                        last_index = li;

                        if (!not_first)
                            not_first = true;
                    }
                }
                Thread.Sleep(100);
            }
        }

        private void ProcessLogLine(string logLine)
        {
            //GetFullInfoCustomer
            try
            {
                if (logLine.Contains("Hi, I would like to buy your") && logLine.Contains("@From"))
                {
                    var cus_inf = new CustomerInfo();

                    cus_inf.OrderType = CustomerInfo.OrderTypes.SINGLE;

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
                    cus_inf.Currency = PoECurrencyManager.Instance.Currencies.GetCurrencyByName(logLine.Substring(begin, length));

                    //Price
                    begin = logLine.IndexOf("for ") + 4;
                    cus_inf.Cost = GetNumber(begin, logLine);

                    //Stash Tab
                    begin = logLine.IndexOf("tab \"") + 5;
                    length = logLine.IndexOf("\"; position") - begin;
                    cus_inf.Stash_Tab = logLine.Substring(begin, length);

                    //left
                    begin = logLine.IndexOf("left ") + 5;
                    cus_inf.Left = (int)GetNumber(begin, logLine);

                    //top
                    begin = logLine.IndexOf("top ") + 4;
                    cus_inf.Top = (int)GetNumber(begin, logLine);

                    //to chaos chaosequivalent
                    cus_inf.Chaos_Price = cus_inf.Currency.ChaosEquivalent * cus_inf.Cost;

                    //trade accepted
                    cus_inf.TradeStatus = CustomerInfo.TradeStatuses.STARTED;

                    if (cus_inf.IsReady)
                    {
                        BotEngine.Customer.Add(cus_inf);

                        Logger.Console.Info(cus_inf.ToString());
                    }
                }

                if (logLine.Contains("I'd like to buy your") && logLine.Contains("@From"))
                {
                    var cus = new CustomerInfo();

                    cus.OrderType = CustomerInfo.OrderTypes.MANY;

                    cus.Nickname = Regex.Replace(logLine, @"([\w\s\W]+@From )|(: [\w\W\s]*)|(<[\w\W\s]+> )", "");

                    cus.Product = Regex.Replace(logLine, @"([\w\W]+your +[\d,]* )|( for+[\w\s\W]*)|( Map [()\d\w]+)", "");

                    string test = Regex.Match(logLine, @"your ([\d]+)").Groups[1].Value;

                    cus.NumberProducts = Convert.ToInt32(test);

                    cus.Cost = Convert.ToDouble(Regex.Replace(logLine, @"([\s\w\W]+for my )|([\D])", "").Replace(".", ","));

                    cus.Currency = PoECurrencyManager.Instance.Currencies.GetCurrencyByName(Regex.Replace(logLine, @"([\w\s\W]+my +[\d,.]* )|( in +[\w\W\s]*)", ""));

                    if (cus.IsReady)
                    {
                        BotEngine.Customer.Add(cus);
                        Logger.Console.Info(cus.ToString());
                    }

                }
            }
            catch (Exception e)
            {
                Logger.Application.Error(e.Message, e);
            }

            //check area
            if (BotEngine.Customer.Any())
            {
                if (logLine.Contains(BotEngine.Customer.First().Nickname) && logLine.Contains("has joined the area"))
                {
                    Logger.Console.Info("He come for product");
                    BotEngine.Customer.First().IsInArea = true;
                }

                if (logLine.Contains("Player not found in this area."))
                {
                    BotEngine.Customer.First().IsInArea = false;
                }

                if (logLine.Contains(": Trade accepted."))
                {
                    BotEngine.Customer.First().TradeStatus = CustomerInfo.TradeStatuses.ACCEPTED;
                }

                if (logLine.Contains(": Trade cancelled."))
                {
                    BotEngine.Customer.First().TradeStatus = CustomerInfo.TradeStatuses.CANCELED;
                }
            }
            else
            {
                if (logLine.Contains("AFK mode is now ON. Autoreply"))
                {
                    ClientManager.Instance.IsAFK = true;
                }
                if (logLine.Contains("AFK mode is now OFF"))
                {
                    ClientManager.Instance.IsAFK = false;
                }
            }

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

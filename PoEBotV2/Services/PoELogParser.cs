using PoE_Trade_Bot.PoEBotV2.Interfaces;
using PoEBotV2.Types;

namespace PoE_Trade_Bot.PoEBotV2.Services
{
    class PoELogParser : IPoELogParser
    {
        public CustomerList ParseLogs(PoELogList logList)
        {
            return new CustomerList();
            //throw new System.NotImplementedException();
        }

        private void GetInfo(string log24)
        {
            ////GetFullInfoCustomer
            //try
            //{
            //    if (log24.Contains("Hi, I would like to buy your") && log24.Contains("@From"))
            //    {
            //        var cus_inf = new CustomerInfo();

            //        cus_inf.OrderType = CustomerInfo.OrderTypes.SINGLE;

            //        int length;
            //        int begin;
            //        //Nickname

            //        if (!log24.Contains("> "))
            //        {
            //            begin = log24.IndexOf("@From ") + 6;
            //            length = log24.IndexOf(": ") - begin;
            //            cus_inf.Nickname = log24.Substring(begin, length);
            //        }
            //        else
            //        {
            //            begin = log24.IndexOf("> ") + 2;
            //            length = log24.IndexOf(": ") - begin;
            //            cus_inf.Nickname = log24.Substring(begin, length);
            //        }


            //        //Product
            //        begin = log24.IndexOf("your ") + 5;
            //        length = log24.IndexOf(" listed") - begin;
            //        cus_inf.Product = log24.Substring(begin, length);

            //        //Currency
            //        begin = log24.IndexOf(" in") - 1;
            //        for (int i = 0; i < 50; i++)
            //        {
            //            if (log24[begin - i] == ' ')
            //            {
            //                begin = begin - i + 1;
            //                break;
            //            }
            //        }
            //        length = log24.IndexOf(" in") - begin;
            //        cus_inf.Currency = Currencies.GetCurrencyByName(log24.Substring(begin, length));

            //        //Price
            //        begin = log24.IndexOf("for ") + 4;
            //        cus_inf.Cost = GetNumber(begin, log24);

            //        //Stash Tab
            //        begin = log24.IndexOf("tab \"") + 5;
            //        length = log24.IndexOf("\"; position") - begin;
            //        cus_inf.Stash_Tab = log24.Substring(begin, length);

            //        //left
            //        begin = log24.IndexOf("left ") + 5;
            //        cus_inf.Left = (int)GetNumber(begin, log24);

            //        //top
            //        begin = log24.IndexOf("top ") + 4;
            //        cus_inf.Top = (int)GetNumber(begin, log24);

            //        //to chaos chaosequivalent
            //        cus_inf.Chaos_Price = cus_inf.Currency.ChaosEquivalent * cus_inf.Cost;

            //        //trade accepted
            //        cus_inf.TradeStatus = CustomerInfo.TradeStatuses.STARTED;

            //        if (cus_inf.IsReady)
            //        {
            //            Customer.Add(cus_inf);

            //            Console.WriteLine(cus_inf.ToString());
            //        }
            //    }

            //    if (log24.Contains("I'd like to buy your") && log24.Contains("@From"))
            //    {
            //        var cus = new CustomerInfo();

            //        cus.OrderType = CustomerInfo.OrderTypes.MANY;

            //        cus.Nickname = Regex.Replace(log24, @"([\w\s\W]+@From )|(: [\w\W\s]*)|(<[\w\W\s]+> )", "");

            //        cus.Product = Regex.Replace(log24, @"([\w\W]+your +[\d,]* )|( for+[\w\s\W]*)|( Map [()\d\w]+)", "");

            //        string test = Regex.Match(log24, @"your ([\d]+)").Groups[1].Value;

            //        cus.NumberProducts = Convert.ToInt32(test);

            //        cus.Cost = Convert.ToDouble(Regex.Replace(log24, @"([\s\w\W]+for my )|([\D])", "").Replace(".", ","));

            //        cus.Currency = Currencies.GetCurrencyByName(Regex.Replace(log24, @"([\w\s\W]+my +[\d,.]* )|( in +[\w\W\s]*)", ""));

            //        if (cus.IsReady)
            //        {
            //            Customer.Add(cus);

            //            Console.WriteLine(cus.ToString());
            //        }

            //    }
            //}
            //catch (Exception e)
            //{

            //    Console.WriteLine(e.Message);
            //}

            ////check area
            //if (Customer.Any())
            //{
            //    if (log24.Contains(Customer.First().Nickname) && log24.Contains("has joined the area"))
            //    {
            //        Console.WriteLine("He come for product");
            //        Customer.First().IsInArea = true;
            //    }

            //    if (log24.Contains("Player not found in this area."))
            //    {
            //        Customer.First().IsInArea = false;
            //    }

            //    if (log24.Contains(": Trade accepted."))
            //    {
            //        Customer.First().TradeStatus = CustomerInfo.TradeStatuses.ACCEPTED;
            //    }

            //    if (log24.Contains(": Trade cancelled."))
            //    {
            //        Customer.First().TradeStatus = CustomerInfo.TradeStatuses.CANCELED;
            //    }
            //}
            //else
            //{
            //    if (log24.Contains("AFK mode is now ON. Autoreply"))
            //    {
            //        IsAfk = true;
            //    }
            //    if (log24.Contains("AFK mode is now OFF"))
            //    {
            //        IsAfk = false;
            //    }
            //}

        }

    }
}

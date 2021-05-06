using Newtonsoft.Json;
using PoE_Trade_Bot.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace PoE_Trade_Bot.Models
{
    public class Currencies
    {
        private DateTime LastUpdate;

        private HttpClient Client;

        private List<Currency_ExRate> CurrenciesList;

        public Currencies()
        {
            Client = new HttpClient();
            CurrenciesList = new List<Currency_ExRate>();
        }

        public Currency_ExRate GetCurrencyByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            switch (name)
            {
                case "gcp":
                    name = "gemcuttersprism";
                    break;
                case "blessed":
                    name = "blessedorb";
                    break;
                case "chrome":
                    name = "chromaticorb";
                    break;
                case "divine":
                    name = "divineorb";
                    break;
                case "exalted":
                    name = "exaltedorb";
                    break;
                case "jewellers":
                    name = "jewellersorb";
                    break;
                case "mirror":
                    name = "mirrorofkalandra";
                    break;
                case "alch":
                    name = "orbofalchemy";
                    break;
                case "alt":
                    name = "orbofalteration";
                    break;
                case "chance":
                    name = "orbofchance";
                    break;
                case "fusing":
                    name = "orboffusing";
                    break;
                case "regret":
                    name = "orbofregret";
                    break;
                case "scour":
                    name = "orbofscouring";
                    break;
                case "transmute":
                    name = "orboftransmutation";
                    break;
                case "regal":
                    name = "regalorb";
                    break;
                case "vaal":
                    name = "vaalorb";
                    break;
                case "aug":
                    name = "orbofaugmentation";
                    break;
                case "chaos":
                    name = "chaosorb";
                    break;
                case "chisel":
                    name = "cartographerschisel";
                    break;
            }

            Currency_ExRate returnRate = CurrenciesList.Find((Currency_ExRate c) => c.NormalName.Equals(name.ToLower()));

            if (returnRate == null)
            {
                Logger.Application.Error($"Exchange Rate not found, {name}");
            }

            return returnRate;
        }

        public void Update()
        {
            var poeNinjaAPI = $"https://poe.ninja/api/data/CurrencyOverview?league={ConfigManager.Instance.ApplicationConfig["POENinja_Leage"]}&type=Currency&language=en";
            var response = Client.GetAsync(poeNinjaAPI).Result;
            var responseBody = response.Content.ReadAsStringAsync().Result;

            var ExchangeRatesJson = JsonConvert.DeserializeObject<CurrenciesJson>(responseBody);

            CurrenciesList.Clear();

            foreach (Line l in ExchangeRatesJson.Lines)
            {
                CurrenciesList.Add(new Currency_ExRate(l.CurrencyTypeName, l.ChaosEquivalent));
            }
            CurrenciesList.Add(new Currency_ExRate("Chaos Orb", 1));
            Logger.Console.Info("Curencies updated!");
        }
    }

    public class Currency_ExRate
    {
        public string Name { get; set; }
        public double ChaosEquivalent { get; set; }
        public string NormalName => Name.Replace(" ", "").Replace("'", "");

        public Currency_ExRate(string name, double chaosequivalent)
        {
            Name = name.ToLower();
            ChaosEquivalent = chaosequivalent;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

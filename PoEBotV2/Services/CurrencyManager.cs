using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PoE_Trade_Bot.PoEBotV2.Exceptions;
using PoE_Trade_Bot.PoEBotV2.Models;
using PoE_Trade_Bot.PoEBotV2.Storage;

namespace PoE_Trade_Bot.PoEBotV2.Services
{
    public class CurrencyManager
    {
        private readonly HttpClient _client;

        private string CachePath { get; set; } = @"D:/test/Cache/";

        public CurrencyManager(HttpClient client)
        {
            _client = client;
        }

        public async Task StartAsync(bool infinity = false)
        {
            do
            {
                await LoadCurrencies();

                if (infinity) await Task.Delay(1000 * 60 * 10);
                // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
            } while (infinity);
        }

        private async Task LoadCurrencies()
        {
            var response =
                await _client.GetAsync(
                    "https://poe.ninja/api/data/CurrencyOverview?league=Ritual&type=Currency&language=en");

            var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());

            var currencyDetails = jObject["currencyDetails"]?.Children().ToList();
            var lines = jObject["lines"]?.Children().ToList();

            if (lines == null) throw new NullLinesPoeNinja();

            var loadTasks = new List<Task>();
            
            foreach (var line in lines)
            {
                var typeName = (string) line["currencyTypeName"];

                var chaosEquivalent =
                    double.Parse((string) line["chaosEquivalent"] ?? "0", CultureInfo.InvariantCulture);

                var type = ItemTypeStorage.FindByName(typeName);

                if (type == null)
                {
                    type = ItemType.Create(typeName, true);

                    ItemTypeStorage.Add(type);
                }

                var currency = CurrencyStorage.FindCurrencyByType(type);

                if (currency == null)
                {
                    var imageUrl = (string) currencyDetails?.Find(token => (string) token["name"] == typeName)?["icon"];
                    
                    loadTasks.Add(SaveCurrencyImage(imageUrl, typeName));

                    currency = Currency.Create(chaosEquivalent, type, imageUrl);
                    CurrencyStorage.Add(currency);
                }
                else
                {
                    currency.ChaosEquivalent = chaosEquivalent;
                }
            }

            Task.WaitAll(loadTasks.ToArray());
        }

        private async Task SaveCurrencyImage(string url, string typeName)
        {
            var filePath = Path.Combine(CachePath, $"{typeName}.png");

            if (File.Exists(filePath)) return;

            using WebClient client = new WebClient();

            Directory.CreateDirectory(CachePath);

            await client.DownloadFileTaskAsync(url, filePath);
        }
    }
}
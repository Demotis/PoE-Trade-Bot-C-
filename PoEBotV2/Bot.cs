using System;
using System.Threading.Tasks;
using PoE_Trade_Bot.PoEBotV2.Models;
using PoE_Trade_Bot.PoEBotV2.Services;

namespace PoE_Trade_Bot.PoEBotV2
{
    public class Bot
    {
        private readonly PoELogManager _logManager;
        private readonly CurrencyManager _currencyManager;

        public async Task StartAsync()
        {
            Console.WriteLine("Bot launched. First load currencies. Please wait...");
            
            // First load, for correct work
            await _currencyManager.StartAsync();

            Console.WriteLine("Currencies loaded. We are monitoring PoE logs");

            Task.WaitAll(
                _currencyManager.StartAsync(true),
                _logManager.StartAsync()
            );
        }

        public Bot(PoELogManager logManager, CurrencyManager currencyManager)
        {
            _logManager = logManager;
            _currencyManager = currencyManager;

            _logManager.OnAfkOn += OnAfkOn;
            _logManager.OnAfkOff += OnAfkOff;
            _logManager.OnOffer += OnOffer;
            _logManager.OnTradeAccepted += OnTradeAccepted;
            _logManager.OnTradeCanceled += OnTradeCanceled;
            _logManager.OnUserJoinedAtArea += OnUserJoinedAtArea;
            _logManager.OnUserNotFoundAtArea += OnUserNotFoundAtArea;
        }

        private void OnAfkOff()
        {
            throw new NotImplementedException();
        }

        private void OnUserNotFoundAtArea()
        {
            throw new NotImplementedException();
        }

        private void OnUserJoinedAtArea(string username)
        {
            throw new NotImplementedException();
        }

        private void OnTradeCanceled()
        {
            throw new NotImplementedException();
        }

        private void OnTradeAccepted()
        {
            throw new NotImplementedException();
        }

        private void OnOffer(Offer offer)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(offer);

            Console.WriteLine(json);
        }

        private void OnAfkOn()
        {
            throw new NotImplementedException();
        }
    }
}
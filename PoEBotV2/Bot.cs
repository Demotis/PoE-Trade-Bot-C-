using System;
using System.Threading.Tasks;
using PoE_Trade_Bot.PoEBotV2.Models;
using PoE_Trade_Bot.PoEBotV2.Services;

namespace PoE_Trade_Bot.PoEBotV2
{
    public class Bot
    {
        private readonly PoELogManager _logManager;

        public async Task StartAsync()
        {
            await _logManager.StartAsync();
        }

        public Bot(PoELogManager logManager)
        {
            _logManager = logManager;

            _logManager.OnAfkOn += OnAfkOn;
            _logManager.OnOffer += OnOffer;
            _logManager.OnTradeAccepted += OnTradeAccepted;
            _logManager.OnTradeCanceled += OnTradeCanceled;
            _logManager.OnUserJoinedAtArea += OnUserJoinedAtArea;
            _logManager.OnUserNotFoundAtArea += OnUserNotFoundAtArea;
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
using System;
using System.Threading.Tasks;
using PoE_Trade_Bot.PoEBotV2.Interfaces;

namespace PoE_Trade_Bot.PoEBotV2.Services
{
    public delegate void OfferHandler();

    public delegate void AfkHandler();

    public delegate void UserNotFoundAtAreaHandler();

    public delegate void UserJoinedAtAreaHandler();

    public delegate void TradeAcceptedHandler();

    public delegate void TradeCanceledHandler();

    public class PoELogManager
    {
        public event OfferHandler OnOffer;

        public event AfkHandler OnAfk;

        public event UserNotFoundAtAreaHandler OnUserNotFoundAtArea;

        public event UserJoinedAtAreaHandler OnUserJoinedAtArea;

        public event TradeAcceptedHandler OnTradeAccepted;

        public event TradeCanceledHandler OnTradeCanceled;

        private readonly ILogReader _logReader;

        public PoELogManager(ILogReader logReader)
        {
            _logReader = logReader;

            _logReader.OnReadLine += OnLogRead;
        }

        private void OnLogRead(ReadLineEventArgs args)
        {
            Console.WriteLine(args.Line);
            
            switch (args.Line)
            {
                case "offer":
                    OnOffer?.Invoke();
                    break;
                case "afk":
                    OnAfk?.Invoke();
                    break;
                case "userNotFountAtArea":
                    OnUserNotFoundAtArea?.Invoke();
                    break;
                case "userJoinedAtArea":
                    OnUserJoinedAtArea?.Invoke();
                    break;
                case "tradeAccepted":
                    OnTradeAccepted?.Invoke();
                    break;
                case "tradeCanceled":
                    OnTradeCanceled?.Invoke();
                    break;
            }
        }

        public async Task StartAsync()
        {
            await _logReader.StartAsync();
        }
    }
}
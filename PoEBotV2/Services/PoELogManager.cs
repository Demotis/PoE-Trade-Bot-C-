using System;
using System.Threading.Tasks;
using PoE_Trade_Bot.PoEBotV2.Interfaces;
using PoE_Trade_Bot.PoEBotV2.Models;

namespace PoE_Trade_Bot.PoEBotV2.Services
{
    public delegate void OfferHandler(Offer offer);

    public delegate void AfkOnHandler();

    public delegate void AfkOffHandler();

    public delegate void UserNotFoundAtAreaHandler();

    public delegate void UserJoinedAtAreaHandler(string username);

    public delegate void TradeAcceptedHandler();

    public delegate void TradeCanceledHandler();

    public class PoELogManager
    {
        public event OfferHandler OnOffer;

        public event AfkOnHandler OnAfkOn;
        public event AfkOffHandler OnAfkOff;

        public event UserNotFoundAtAreaHandler OnUserNotFoundAtArea;

        public event UserJoinedAtAreaHandler OnUserJoinedAtArea;

        public event TradeAcceptedHandler OnTradeAccepted;

        public event TradeCanceledHandler OnTradeCanceled;

        private readonly ILogReader _logReader;
        private readonly IPoELogParser _logParser;

        public PoELogManager(ILogReader logReader, IPoELogParser logParser)
        {
            _logReader = logReader;
            _logParser = logParser;

            _logReader.OnReadLine += OnLogRead;
        }

        private void OnLogRead(ReadLineEventArgs args)
        {
            var line = args.Line;
            
            _logParser.ParseUserNotFoundAtArea(line, () => OnUserNotFoundAtArea?.Invoke());

            _logParser.ParseUserJoinedAtArea(line, username => OnUserJoinedAtArea?.Invoke(username));

            _logParser.ParseTradeAccepted(line, () => OnTradeAccepted?.Invoke());

            _logParser.ParseTradeCanceled(line, () => OnTradeCanceled?.Invoke());

            _logParser.ParseOffer(line, offer => OnOffer?.Invoke(offer));

            _logParser.ParseAfkOn(line, () => OnAfkOn?.Invoke());

            _logParser.ParseAfkOff(line, () => OnAfkOff?.Invoke());
        }

        public async Task StartAsync()
        {
            await _logReader.StartAsync();
        }
    }
}
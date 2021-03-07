using System;
using PoE_Trade_Bot.PoEBotV2.Models;

namespace PoE_Trade_Bot.PoEBotV2.Interfaces
{
    public interface IPoELogParser
    {
        public void ParseAfkOn(string logLine, Action callback);

        public void ParseAfkOff(string logLine, Action callback);

        public void ParseOffer(string logLine, Action<Offer> callback);

        public void ParseTradeAccepted(string logLine, Action callback);

        public void ParseTradeCanceled(string logLine, Action callback);

        public void ParseUserJoinedAtArea(string logLine, Action<string> callback);

        public void ParseUserNotFoundAtArea(string logLine, Action callback);
    }
}
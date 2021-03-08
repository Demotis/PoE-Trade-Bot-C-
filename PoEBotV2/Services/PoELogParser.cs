using System;
using System.Linq;
using System.Text.RegularExpressions;
using PoE_Trade_Bot.PoEBotV2.Interfaces;
using PoE_Trade_Bot.PoEBotV2.Models;
using PoE_Trade_Bot.PoEBotV2.Storage;

namespace PoE_Trade_Bot.PoEBotV2.Services
{
    class PoELogParser : IPoELogParser
    {
        private readonly string[] _offerPatterns =
        {
            "Hi, I'd like to buy your",
            "Hi, I would like to buy your"
        };

        public void ParseAfkOn(string logLine, Action callback)
        {
            if (logLine.Contains("AFK mode is now ON. Autoreply"))
                callback?.Invoke();
        }

        public void ParseAfkOff(string logLine, Action callback)
        {
            if (logLine.Contains("AFK mode is now OFF"))
                callback?.Invoke();
        }

        public void ParseOffer(string logLine, Action<Offer> callback)
        {
            if (!IsOffer(logLine)) return;

            var offer = IsManyOffer(logLine) ? ParseManyOffer(logLine) : ParseSingleOffer(logLine);

            callback?.Invoke(offer);
        }

        private Offer ParseSingleOffer(string logLine)
        {
            var username = ParseUsername(logLine);

            var match = Regex.Match(logLine,
                @"Hi, I would like to buy your (?<productName>.*?) listed for (?<offerPrice>\d*?) (?<currencyName>.+?) in");

            var productName = match.Groups["productName"].Value;
            
            var offerPrice = int.Parse(match.Groups["offerPrice"].Value);
            
            var currencyName = match.Groups["currencyName"].Value;

            var currency = CurrencyStorage.FindCurrencyByTypeName(currencyName);
            
            var price = Price.Create(offerPrice, currency);
            
            var product = ProductsStorage.FindProductByTypeName(productName);

            return Offer.Create(username, price, product);
        }

        private Offer ParseManyOffer(string logLine)
        {
            var username = ParseUsername(logLine);

            var match = Regex.Match(logLine,
                @"Hi, I'd like to buy your (?<for>\d+?) (?<productName>.+?) for my (?<offerPrice>\d+?) (?<currencyName>.+?) in");

            var forCount = int.Parse(match.Groups["for"].Value);

            var productName = match.Groups["productName"].Value;

            var offerPrice = int.Parse(match.Groups["offerPrice"].Value);

            var currencyName = match.Groups["currencyName"].Value;

            var currency = CurrencyStorage.FindCurrencyByTypeName(currencyName);

            var price = Price.Create(offerPrice, currency, forCount);

            var product = ProductsStorage.FindProductByTypeName(productName);

            return Offer.Create(username, price, product, forCount);
        }

        public void ParseTradeAccepted(string logLine, Action callback)
        {
            if (logLine.Contains(": Trade accepted.")) callback?.Invoke();
        }

        public void ParseTradeCanceled(string logLine, Action callback)
        {
            if (logLine.Contains(": Trade cancelled.")) callback?.Invoke();
        }

        public void ParseUserJoinedAtArea(string logLine, Action<string> callback)
        {
            if (!logLine.Contains("has joined the area")) return;

            var username = ParseUsername(logLine);

            callback?.Invoke(username);
        }

        public void ParseUserNotFoundAtArea(string logLine, Action callback)
        {
            if (logLine.Contains("Player not found in this area."))
                callback?.Invoke();
        }


        private string ParseUsername(string logLine)
        {
            var match = Regex.Match(logLine, @"@From (?<nickname>.*?):");

            return !match.Groups["nickname"].Success ? null : match.Groups["nickname"].Value;
        }

        private bool IsOffer(string logLine)
        {
            return _offerPatterns.Any(logLine.Contains);
        }

        private bool IsManyOffer(string logLine)
        {
            return false;
        }
    }
}
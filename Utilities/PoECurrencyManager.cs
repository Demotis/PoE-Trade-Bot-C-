using PoE_Trade_Bot.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoE_Trade_Bot.Utilities
{
    public sealed class PoECurrencyManager : IDisposable
    {
        private static readonly PoECurrencyManager instance = new PoECurrencyManager();
        private bool disposedValue;
        public static PoECurrencyManager Instance => instance;

        private Task ExchangeRatesTask;

        public Currencies Currencies { get; private set; }

        static PoECurrencyManager()
        {
        }

        private PoECurrencyManager()
        {
            Currencies = new Currencies();
            ExchangeRatesTask = new Task(CheckExchangeRates);
            ExchangeRatesTask.Start();
        }

        private void CheckExchangeRates()
        {
            DateTime timer = DateTime.Now + new TimeSpan(0, 30, 0);

            while (true)
            {
                if (timer <= DateTime.Now)
                {
                    Currencies.Update();
                    timer = DateTime.Now + new TimeSpan(0, 30, 0);
                }

                Thread.Sleep(1000 * 60 * 5);
            }
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

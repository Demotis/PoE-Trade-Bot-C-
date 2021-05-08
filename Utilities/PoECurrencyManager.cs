using PoE_Trade_Bot.Models;
using System;

namespace PoE_Trade_Bot.Utilities
{
    public sealed class PoECurrencyManager : IDisposable
    {


        private static readonly PoECurrencyManager instance = new PoECurrencyManager();
        private bool disposedValue;
        public static PoECurrencyManager Instance => instance;
        private System.Timers.Timer tTimer;

        public Currencies Currencies { get; private set; }

        

        static PoECurrencyManager()
        {
        }

        private PoECurrencyManager()
        {
            Currencies = new Currencies();

        }

        public void StartService()
        {
            tTimer = new System.Timers.Timer();
            tTimer.Interval = 30 * 60 * 1000;
            tTimer.Elapsed += CheckExchangeRates;
            tTimer.AutoReset = true;
            tTimer.Enabled = true;
            CheckExchangeRates(null, null);
        }

        private void CheckExchangeRates(object source, System.Timers.ElapsedEventArgs e)
        {
            Currencies.Update();
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (tTimer.Enabled)
                {
                    // ToDo: Add code here to stop and dispose the timer
                    tTimer.Stop();
                    tTimer.Enabled = true;
                    
                    
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

using System;

namespace PoE_Trade_Bot.PoEClient
{
    public sealed class LogManager : IDisposable
    {
        private static readonly LogManager instance = new LogManager();
        private bool disposedValue;
        public static LogManager Instance => instance;

        static LogManager()
        {
        }

        private LogManager()
        {
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

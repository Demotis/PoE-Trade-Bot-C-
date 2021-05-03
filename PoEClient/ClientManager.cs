using PoE_Trade_Bot.Services;
using System;

namespace PoE_Trade_Bot.PoEClient
{
    public sealed class ClientManager : IDisposable
    {
        private static readonly ClientManager instance = new ClientManager();
        private bool disposedValue;
        public static ClientManager Instance => instance;

        public bool IsAFK { get; set; }

        static ClientManager()
        {
        }

        private ClientManager()
        {
            if (!Win32.IsPoERun())
            {
                throw new Exception("Path of Exile is not running!");
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

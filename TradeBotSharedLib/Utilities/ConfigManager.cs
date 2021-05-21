using System;
using System.Collections.Generic;
using System.Configuration;

namespace TradeBotSharedLib
{
    public sealed class ConfigManager : IDisposable
    {
        private static readonly ConfigManager instance = new ConfigManager();
        private bool disposedValue;
        public static ConfigManager Instance => instance;


        public Dictionary<string, string> ApplicationConfig { get; private set; }

        static ConfigManager()
        {
        }

        private ConfigManager()
        {
            // Load out config values
            ApplicationConfig = new Dictionary<string, string>();
            foreach (string key in ConfigurationManager.AppSettings.AllKeys)
                ApplicationConfig[key] = ConfigurationManager.AppSettings[key];
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

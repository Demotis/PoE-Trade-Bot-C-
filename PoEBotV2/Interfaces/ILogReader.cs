using System;
using System.Threading.Tasks;

namespace PoE_Trade_Bot.PoEBotV2.Interfaces
{
    public class ReadLineEventArgs : EventArgs
    {
        public DateTime CreatedAt { get; set; }

        public string Line { get; set; }
    }

    public delegate void ReadLineHandler(ReadLineEventArgs eventArgs);

    public interface ILogReader
    {
        public event ReadLineHandler OnReadLine;

        Task StartAsync();
    }
}
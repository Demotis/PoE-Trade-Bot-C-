using System.Threading.Tasks;
using PoE_Trade_Bot.PoEBotV2.Services;

namespace PoE_Trade_Bot.PoEBotV2.Interfaces
{
    public interface ILogReader
    {
        Task StartAsync(OnEndRead onEndRead);
    }
}

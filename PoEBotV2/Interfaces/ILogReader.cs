using PoEBotV2.Services;

using System.Threading.Tasks;

namespace PoEBotV2.Interfaces
{
    public interface ILogReader
    {
        Task StartAsync(OnEndRead onEndRead);
    }
}

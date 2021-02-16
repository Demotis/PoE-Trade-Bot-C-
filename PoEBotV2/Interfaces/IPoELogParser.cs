using PoEBotV2.Types;

namespace PoE_Trade_Bot.PoEBotV2.Interfaces
{
    public interface IPoELogParser
    {
        CustomerList ParseLogs(PoELogList logList);
    }
}

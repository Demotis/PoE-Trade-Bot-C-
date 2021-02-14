using PoEBotV2.Types;

namespace PoEBotV2.Interfaces
{
    public interface IPoELogParser
    {
        CustomerList ParseLogs(PoELogList logList);
    }
}

using System.Threading.Tasks;

using PoE_Trade_Bot.PoEBotV2.Interfaces;

using PoEBotV2.Types;

namespace PoE_Trade_Bot.PoEBotV2.Services
{
    class CustomerManager : ICustomerManager
    {
        public ILogReader LogReader { get; }
        public IPoELogParser PoELogParser { get; }
        public CustomerList Customers { get; }

        public CustomerManager(ILogReader logReader, IPoELogParser poELogParser)
        {
            Customers = new CustomerList();
            LogReader = logReader;
            PoELogParser = poELogParser;
        }

        public async Task StartAsync()
        {
            await LogReader.StartAsync(OnChangeLog);
        }

        public void OnChangeLog(PoELogList newLogs)
        {
            var customers = PoELogParser.ParseLogs(newLogs);
            Customers.AddRange(customers);
        }
    }
}

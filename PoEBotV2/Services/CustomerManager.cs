using System.Threading.Tasks;
using PoE_Trade_Bot.PoEBotV2.Interfaces;
using PoEBotV2.Types;

namespace PoE_Trade_Bot.PoEBotV2.Services
{
    class CustomerManager : ICustomerManager
    {
        public ILogReader LogReader { get; }
        public IPoELogParser PoELogParser { get; }

        private readonly CustomerList customers;

        public CustomerManager(ILogReader logReader, IPoELogParser poELogParser)
        {
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
            this.customers.AddRange(customers);
        }
    }
}

using PoEBotV2.Interfaces;

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PoEBotV2.Services
{
    public delegate void OnEndRead(List<string> results);

    class LogReader : ILogReader
    {
        private int lastIndex = -1;
        public string LogFilter { get; } = "a24 [INFO Client";

        public LogReader(string logFilter)
        {
            LogFilter = logFilter;
        }

        public LogReader()
        {
        }

        public async Task StartAsync(string logDir, OnEndRead onEndRead)
        {
            void callback(object _, FileSystemEventArgs e) => ReadLogs(e.FullPath, onEndRead);

            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                watcher.Path = logDir;
                watcher.Changed += callback;
                watcher.Created += callback;

                watcher.EnableRaisingEvents = true;

                while (true) await Task.Delay(100);
            }
        }

        private void ReadLogs(string filePath, OnEndRead onEndRead)
        {
            var result = new List<string>();
            int lineIndex = 0;

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (var streanReader = new StreamReader(fileStream))
            {
                while (!streanReader.EndOfStream)
                {
                    lineIndex++;
                    var lastLine = streanReader.ReadLine();

                    if (lineIndex > lastIndex)
                    {
                        if (lastLine.Contains(LogFilter))
                        {
                            result.Add(lastLine);
                        }
                    }
                }

                streanReader.Dispose();
                fileStream.Dispose();

                lastIndex = lineIndex;
            }

            onEndRead(result);
        }
    }
}

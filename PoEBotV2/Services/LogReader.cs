using PoEBotV2.Interfaces;
using PoEBotV2.Types;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PoEBotV2.Services
{
    public delegate void OnEndRead(PoELogList results);

    class LogReader : ILogReader
    {
        private int lastIndex = -1;
        public string LogFilter { get; } = "a24 [INFO Client";
        public string LogDirPath { get; }

        public LogReader(string logFilter, string logDirPath)
        {
            LogFilter = logFilter;
            LogDirPath = logDirPath;
        }

        public LogReader(string logDirPath)
        {
            LogDirPath = logDirPath;
        }

        public async Task StartAsync(OnEndRead onEndRead)
        {
            void callback(object _, FileSystemEventArgs e) => ReadLogs(e.FullPath, onEndRead);

            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                watcher.Path = LogDirPath;
                watcher.Changed += callback;
                watcher.Created += callback;

                watcher.EnableRaisingEvents = true;

                while (true) await Task.Delay(100);
            }
        }

        private void ReadLogs(string filePath, OnEndRead onEndRead)
        {
            var result = new PoELogList();
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
                        if (CheckIsNewLog(lastLine)) result.Add(lastLine);
                    }
                }

                streanReader.Dispose();
                fileStream.Dispose();

                lastIndex = lineIndex;
            }

            onEndRead(result);
        }

        private bool CheckIsNewLog(string logLine)
        {
            //2020/11/19 22:04:46
            Match match = Regex.Match(logLine, @"(\d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2})");

            if (!match.Success) return false;

            DateTime dt = DateTime.ParseExact(match.Value, @"yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

            return DateTime.Now.AddMinutes(-1) < dt;
        }
    }
}

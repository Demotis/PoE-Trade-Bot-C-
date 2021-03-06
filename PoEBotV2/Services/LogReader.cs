using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PoE_Trade_Bot.PoEBotV2.Interfaces;

namespace PoE_Trade_Bot.PoEBotV2.Services
{
    internal class LogReader : ILogReader
    {
        private int _lastIndex = -1;

        private readonly string _logDirPath;
        public event ReadLineHandler OnReadLine;

        public LogReader(string logDirPath)
        {
            _logDirPath = logDirPath;
        }

        public async Task StartAsync()
        {
            void Callback(object _, FileSystemEventArgs e) => ReadLogs(e.FullPath);

            using var watcher = new FileSystemWatcher
                {NotifyFilter = NotifyFilters.LastWrite, Filter = @"*.txt", Path = _logDirPath};

            watcher.Changed += Callback;
            watcher.Created += Callback;

            watcher.EnableRaisingEvents = true;

            while (true) await Task.Delay(100);
        }

        private void ReadLogs(string filePath)
        {
            var lineIndex = 0;

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(fileStream);

            while (!streamReader.EndOfStream)
            {
                lineIndex++;
                var lastLine = streamReader.ReadLine();

                if (lineIndex <= _lastIndex) continue;

                if (CheckIsNewLog(lastLine))
                {
                    OnReadLine?.Invoke(new ReadLineEventArgs
                    {
                        Line = lastLine,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            streamReader.Dispose();
            fileStream.Dispose();

            _lastIndex = lineIndex;
        }

        private static bool CheckIsNewLog(string logLine)
        {
            //2020/11/19 22:04:46
            var match = Regex.Match(logLine, @"(\d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2})");

            if (!match.Success) return false;

            var dt = DateTime.ParseExact(match.Value, @"yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            var now = DateTime.Now.AddMinutes(-5);

            return now <= dt;
        }
    }
}
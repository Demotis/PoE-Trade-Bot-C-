using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PoEBotV2.Services
{
    class LogReader
    {
        private int lastIndex = -1;

        public LogReader()
        {

        }

        public async Task startAsync(string logDir)
        {
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                watcher.Path = logDir;
                watcher.Changed += OnChanged;
                watcher.Created += OnChanged;

                watcher.EnableRaisingEvents = true;

                while (true) await Task.Delay(100);
            }
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");

            var res = ReadLogs(e.FullPath);

            Console.WriteLine(String.Join("\n", res));
        }

        private List<string> ReadLogs(string filePath)
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
                        if (lastLine.Contains("a24 [INFO Client"))
                        {
                            result.Add(lastLine);
                        }
                    }
                }

                streanReader.Dispose();
                fileStream.Dispose();

                if (lineIndex > lastIndex) lastIndex = lineIndex;
            }

            return result;
        }
    }
}

using System.IO;

namespace PoEBotV2.Services
{
    class PoELogParser
    {
        public string FilePath { get; }


        public PoELogParser(string filePath)
        {
            FilePath = filePath;

        }

        //public void Parse()
        //{
        //    int lineIndex = 0;
        //    string lastLine = string.Empty;

        //    var fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        //        using (var streanReader = new StreamReader(fileStream))
        //        {
        //            while (!streanReader.EndOfStream)
        //            {
        //                lineIndex++;
        //                lastLine = streanReader.ReadLine();

        //                if (lineIndex > lastIndex)
        //                {
        //                    if (lastLine.Contains("a24 [INFO Client"))
        //                    {
        //                        //Console.WriteLine(lastLine);
        //                        //GetInfo(lastLine);
        //                    }
        //                }
        //            }

        //            streanReader.Dispose();
        //            fileStream.Dispose();
        //        }
        //}
    }
}

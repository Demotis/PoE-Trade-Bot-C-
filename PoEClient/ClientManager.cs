using PoE_Trade_Bot.Services;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace PoE_Trade_Bot.PoEClient
{
    public sealed class ClientManager : IDisposable
    {
        private static readonly ClientManager instance = new ClientManager();
        private bool disposedValue;
        public static ClientManager Instance => instance;

        public bool IsAFK { get; set; }
        public Process ActiveProcess { get; private set; }

        static ClientManager()
        {
        }

        private ClientManager()
        {
            // Find POE window and setup ActiveProcess
            Process process = null;
            if (GetProcess(ConfigManager.Instance.ApplicationConfig["POEProcessName"], out process))
                ActiveProcess = process;
            else
                throw new Exception("Path of Exile is not running!");

        }

        public void ValidateProcess()
        {
            if (ActiveProcess == null)
                throw new Exception("Path of Exile process not set.");
            if (ActiveProcess.HasExited)
                throw new Exception("Path of Exile process has been closed.");
            if (!ActiveProcess.ProcessName.ToLower().Contains(ConfigManager.Instance.ApplicationConfig["POEProcessName"].ToLower()))
                throw new Exception("Active Process is not Path of Exile.");
        }

        public void BringToForeground()
        {
            ValidateProcess();
            if (!Win32.SetForegroundWindow(ActiveProcess.MainWindowHandle))
                throw new Exception($"Unable to set POE Process as foreground window");
        }

        public bool GetProcess(string processName, out Process process)
        {
            process = null;
            foreach (Process nextProcess in Process.GetProcesses())
            {
                if (nextProcess.ProcessName.ToLower().Contains(processName.ToLower()))
                {
                    process = nextProcess;
                    return true;
                }
            }
            return false;
        }

        public void SendKey(string key)
        {
            BringToForeground();
            SendKeys.SendWait(key);
        }

        public void ChatCommand(string command)
        {
            BringToForeground();
            SendKeys.SendWait("{ENTER}");
            foreach (char c in command)
            {
                SendKeys.SendWait(c.ToString());
            }
            SendKeys.SendWait("{ENTER}");
        }

        public void SendNumber(int number)
        {
            BringToForeground();
            string str = $"{number}";
            foreach (Char c in str)
            {
                SendKeys.SendWait(c.ToString());
            }
        }
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}

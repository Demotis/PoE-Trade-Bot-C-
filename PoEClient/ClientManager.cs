using PoE_Trade_Bot.Models;
using PoE_Trade_Bot.Services;
using System;
using System.Diagnostics;
using System.Drawing;
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
        public Rectangle WindowRect { get; private set; }

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

            SetCurrentPosition();
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
            SetCurrentPosition();
        }

        public void SetCurrentPosition()
        {
            Rectangle rect;
            Win32.GetWindowRect(ActiveProcess.MainWindowHandle, out rect);
            WindowRect = rect;
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

        private void TranslatePosition(ref Position relativePosition)
        {
            relativePosition.Left = WindowRect.Left + relativePosition.Left;
            relativePosition.Top = WindowRect.Top + relativePosition.Top;
        }

        public bool OpenStash(int testCycle = 1)
        {
            if (testCycle > 20)
                return false;

            BringToForeground();

            using (Bitmap stashTitleSearch = ScreenCapture.CaptureRectangle(WindowRect))
            {
                Position foundPosition = OpenCV_Service.FindObject(stashTitleSearch, @"Assets/UI_Fragments/stashtitle.png");
                if (foundPosition.IsVisible)
                {
                    TranslatePosition(ref foundPosition);
                    Win32.MoveTo(foundPosition.Left + foundPosition.Width / 2, foundPosition.Top + foundPosition.Height);
                    Win32.DoMouseClick();

                    using (Bitmap stashOpenSearch = ScreenCapture.CaptureRectangle(WindowRect))
                    {
                        if (OpenCV_Service.FindObject(stashOpenSearch, @"Assets/UI_Fragments/open_stash.png").IsVisible)
                            return true;
                    }

                }
            }
            return OpenStash(testCycle++);
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

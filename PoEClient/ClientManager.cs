using PoE_Trade_Bot.Enums;
using PoE_Trade_Bot.Models;
using PoE_Trade_Bot.Services;
using PoE_Trade_Bot.Utilities;
using System;
using System.Diagnostics;
using System.Drawing;
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
        public Rectangle WindowRect { get; private set; }

        public Resolution ResolutionEnum { get; private set; }

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

            ResolutionEnum = (Resolution)Convert.ToInt32(ConfigManager.Instance.ApplicationConfig["POEResolution"]);

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
            Logger.Console.Debug($"Open Stash Cycle {testCycle}...");

            if (testCycle > 20)
                return false;

            BringToForeground();

            using (Bitmap stashTitleSearch = ScreenCapture.CaptureRectangle(WindowRect))
            {
                if (OpenCV_Service.FindObject(stashTitleSearch, StaticUtils.GetUIFragmentPath("open_stash")).IsVisible)
                    return true;

                Position foundPosition = OpenCV_Service.FindObject(stashTitleSearch, StaticUtils.GetUIFragmentPath("stashtitle"));
                if (foundPosition.IsVisible)
                {
                    TranslatePosition(ref foundPosition);
                    Win32.MoveTo(foundPosition.ClickTargetX, foundPosition.ClickTargetY);
                    Win32.DoMouseClick();
                    Thread.Sleep(2000);
                }
            }
            return OpenStash(testCycle++);
        }

        public bool ActivateTab(string tabName, int testCycle = 1)
        {
            Logger.Console.Debug($"Activate Tab {tabName}, Cycle {testCycle}");

            if (testCycle > 20)
                return false;

            if (!OpenStash())
                throw new Exception("Unable to open Stash");

            BringToForeground();

            using (Bitmap tabSearch = ScreenCapture.CaptureRectangle(WindowRect))
            {
                Position foundPosition;
                foundPosition = OpenCV_Service.FindObject(tabSearch, StaticUtils.GetUIFragmentPath($"active_{tabName}"));
                if (foundPosition.IsVisible)
                    return true; // Found the tab and it was active

                foundPosition = OpenCV_Service.FindObject(tabSearch, StaticUtils.GetUIFragmentPath($"notactive_{tabName}"));
                if (foundPosition.IsVisible) // Found the tab and now I need to click it
                {
                    TranslatePosition(ref foundPosition);
                    Win32.MoveTo(foundPosition.ClickTargetX, foundPosition.ClickTargetY);
                    Win32.DoMouseClick();
                    return ActivateTab(tabName, testCycle++); // We need to make sure it is now active;
                }

                // We did not find a tab that matches
                throw new Exception($"Tab {tabName} not found.");
            }
        }

        public void ClearInventory(string recycle_tab = "recycle_tab")
        {
            // Activate Recycle Tab
            ActivateTab(recycle_tab);

            // Now move everything from Inventory to Stash
            // We do not care what the item is at this point just CTRL+Click every block
            foreach (Position invData in InventoryPositions.GetInvenoryPositions(ResolutionEnum))
            {
                Position tempPosition = new Position { Left = invData.Left, Top = invData.Top, Height = invData.Height, Width = invData.Width };
                TranslatePosition(ref tempPosition);
                Win32.MoveTo(tempPosition.ClickTargetX, tempPosition.ClickTargetY);
                Win32.CtrlMouseClick();
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

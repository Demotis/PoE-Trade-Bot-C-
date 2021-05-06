using PoE_Trade_Bot.Enums;
using PoE_Trade_Bot.Models;
using PoE_Trade_Bot.Models.Test;
using PoE_Trade_Bot.Services;
using PoE_Trade_Bot.Utilities;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
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
        private System.Timers.Timer _afkTimer { get; set; }

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

            StartAFKService();
        }

        private void StartAFKService()
        {
            _afkTimer = new System.Timers.Timer();
            _afkTimer.Interval = 60000;
            _afkTimer.Elapsed += AfkTimerTick;
            _afkTimer.AutoReset = true;
            _afkTimer.Enabled = true;
        }

        private void AfkTimerTick(object source, System.Timers.ElapsedEventArgs e)
        {
            if (!BotEngine.Customer.Any())
                ChatCommand(Enums.ChatCommand.AFK_OFF.GetDescription());
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
            WINDOWINFO info = new WINDOWINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);
            Win32.GetWindowInfo(ActiveProcess.MainWindowHandle, ref info);


            Rectangle rect = new Rectangle();
            rect.X = info.rcClient.Left;
            rect.Y = info.rcClient.Top;
            rect.Height = info.rcClient.Bottom - info.rcClient.Top;
            rect.Width = info.rcClient.Right - info.rcClient.Left;

            // Win32.GetWindowRect(ActiveProcess.MainWindowHandle, out rect);
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

            // Have tried this 20 times, return failure
            if (testCycle > 20)
                return false;

            BringToForeground();

            Position absolutePosition = GetAbsoluteAssetPosition(StaticUtils.GetUIFragmentPath("open_stash"), 0.90);
            if (absolutePosition != null)
                return true;
            absolutePosition = GetAbsoluteAssetPosition(StaticUtils.GetUIFragmentPath("stashtitle"), 0.90);
            if (absolutePosition != null)
                ClickPosition(absolutePosition.ClickTargetX, absolutePosition.ClickTargetY);

            // Give it a little time incase it's loading the screen
            Thread.Sleep(1000);
            return OpenStash(++testCycle);
        }

        public void ClickPosition(int clickTargetX, int clickTargetY)
        {
            Win32.MoveTo(clickTargetX, clickTargetY);
            Win32.DoMouseClick();
            Thread.Sleep(100);
        }

        public Position GetAbsoluteAssetPosition(string assetPath, double threshold = 0.95)
        {
            Position foundPosition;
            using (Bitmap search = ScreenCapture.CaptureRectangle(WindowRect))
                foundPosition = OpenCV_Service.FindObject(search, assetPath, threshold);

            if (!foundPosition.IsVisible)
                return null;
            TranslatePosition(ref foundPosition);
            return foundPosition;
        }

        public bool ActivateTab(string tabName, int testCycle = 1)
        {
            Logger.Console.Debug($"Activate Tab {tabName}, Cycle {testCycle}");

            // Have tried this 20 times, return failure
            if (testCycle > 20)
                return false;

            if (!OpenStash())
                throw new Exception("Unable to open Stash");

            BringToForeground();
            Position absolutePosition = GetAbsoluteAssetPosition(StaticUtils.GetUIFragmentPath($"active_{tabName}"));
            if (absolutePosition == null) // Active Tab not found, let's look for inactive tab
                absolutePosition = GetAbsoluteAssetPosition(StaticUtils.GetUIFragmentPath($"notactive_{tabName}"));
            if (absolutePosition == null) // Nothing was found, Let's cycle incase we are still loading
            {
                Thread.Sleep(1000); // Give it a second to finish what it's doing
                return ActivateTab(tabName, ++testCycle);
            }
            ClickPosition(absolutePosition.ClickTargetX, absolutePosition.ClickTargetY);
            return true;
        }

        public void ClearInventory(string recycle_tab = "recycle_tab")
        {
            Logger.Console.Debug($"Clearing Inventory to {recycle_tab}.");
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
            Logger.Console.Debug("Clearing Inventory Complete.");
        }

        public string GetItemInfo(int clickTargetX, int clickTargetY)
        {
            BringToForeground();
            Win32.MoveTo(clickTargetX, clickTargetY);
            Thread.Sleep(10);
            Clipboard.Clear();
            string ss = null;
            SendKey("^c");
            Thread.Sleep(100);
            ss = Win32.GetText();
            if (string.IsNullOrWhiteSpace(ss))
                ss = "empty_string";
            return ss.Replace("\r", "");
        }

        public Tab GetTabData(string tabName = "trade_tab")
        {
            Logger.Console.Debug($"Tab Scan of {tabName}.");

            ActivateTab(tabName);

            Tab returnTab = new Tab();

            foreach (Position stashData in StashPositions.GetStashPositions(ResolutionEnum))
            {
                Position tempPosition = new Position { Left = stashData.Left, Top = stashData.Top, Height = stashData.Height, Width = stashData.Width };
                TranslatePosition(ref tempPosition);

                ItemInfoParser itemParser = new ItemInfoParser(GetItemInfo(tempPosition.ClickTargetX, tempPosition.ClickTargetY));
                if (itemParser.Item.Name == "Not For Sell")
                    continue;
                itemParser.AddPlace(tempPosition.ClickTargetX, tempPosition.ClickTargetY);
                returnTab.AddItem(itemParser.Item);
            }

            Logger.Console.Debug("Tab Scan completed.");
            return returnTab;
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_afkTimer != null)
                    {
                        if (_afkTimer.Enabled)
                            _afkTimer.Stop();
                        _afkTimer.Elapsed -= AfkTimerTick;
                        _afkTimer = null;
                    }
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

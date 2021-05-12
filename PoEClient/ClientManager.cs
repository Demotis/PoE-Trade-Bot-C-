using PoETradeBot.Enums;
using PoETradeBot.Models;
using PoETradeBot.Models.Test;
using PoETradeBot.Services;
using PoETradeBot.Utilities;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace PoETradeBot.PoEClient
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

            if (!BringToForeground())
                throw new Exception("Error accessing Path of Exile!");

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
            if (!BotEngine.CustomerQueue.Any())
                ChatCommand(Enums.ChatCommand.AFK_OFF.GetDescription());
        }

        public bool ValidateProcess()
        {
            if (ActiveProcess == null)
                return false;
            if (ActiveProcess.HasExited)
                return false;
            if (!ActiveProcess.ProcessName.ToLower().Contains(ConfigManager.Instance.ApplicationConfig["POEProcessName"].ToLower()))
                return false;
            return true;
        }

        public bool BringToForeground()
        {
            if (!ValidateProcess())
                return false;
            if (!Win32.SetForegroundWindow(ActiveProcess.MainWindowHandle))
                return false;
            return SetCurrentPosition();
        }

        public bool SetCurrentPosition()
        {
            try
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
                return true;
            }
            catch (Exception ex)
            {
                Logger.Application.Fatal("SetCurrentPosition", ex);
                return false;
            }
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

        public bool SendKey(string key)
        {
            if (!BringToForeground())
                return false;
            SendKeys.SendWait(key);
            return true;
        }

        public bool ChatCommand(string command)
        {
            Logger.Console.Info($"CONSOLE: {command}");
            if (!BringToForeground())
                return false;
            SendKeys.SendWait("{ENTER}");
            foreach (char c in command)
            {
                SendKeys.SendWait(c.ToString());
            }
            SendKeys.SendWait("{ENTER}");
            return true;
        }

        public bool SendNumber(int number)
        {
            if (!BringToForeground())
                return false;
            string str = $"{number}";
            foreach (Char c in str)
            {
                SendKeys.SendWait(c.ToString());
            }
            return true;
        }

        public Position TranslatePosition(Position relativePosition)
        {
            Position returnPosition = relativePosition.Clone();
            returnPosition.Left = WindowRect.Left + returnPosition.Left;
            returnPosition.Top = WindowRect.Top + returnPosition.Top;
            return returnPosition;
        }

        public bool OpenStash(int testCycle = 1)
        {
            Logger.Console.Debug($"Open Stash Cycle {testCycle}...");

            // Have tried this 20 times, return failure
            if (testCycle > 20)
                return false;

            if (!BringToForeground())
                return false;

            Position absolutePosition = GetAbsoluteAssetPosition(StaticUtils.GetUIFragmentPath("open_stash"), 0.90);
            if (absolutePosition.IsVisible)
                return true;
            absolutePosition = GetAbsoluteAssetPosition(StaticUtils.GetUIFragmentPath("stashtitle"), 0.90);
            // Shift the position to the bottom of the tag
            absolutePosition.Top += absolutePosition.Height;
            if (absolutePosition.IsVisible)
                ClickPosition(absolutePosition, 100);

            // Give it a little time incase it's loading the screen
            Thread.Sleep(1000);
            return OpenStash(++testCycle);
        }

        public void GetPartialStackToCusor(Position absolutePosition, int amount)
        {
            ShiftClickPosition(absolutePosition);
            foreach (char character in amount.ToString())
                SendKey(character.ToString());
            SendKeys.SendWait("{ENTER}");
        }

        public void ShiftClickPosition(Position absolutePosition)
        {
            Win32.MoveTo(absolutePosition.ClickTargetX, absolutePosition.ClickTargetY);
            Win32.ShiftClick();
            Win32.MoveTo(0, 0);
        }


        public void CtrlClickPosition(Position absolutePosition, int clickDelay = 100)
        {
            Win32.MoveTo(absolutePosition.ClickTargetX, absolutePosition.ClickTargetY);
            Win32.CtrlMouseClick(clickDelay);
            Win32.MoveTo(0, 0);
        }

        public void ClickPosition(Position absolutePosition, int clickDelay = 100)
        {
            Win32.MoveTo(absolutePosition.ClickTargetX, absolutePosition.ClickTargetY);
            Win32.DoMouseClick(clickDelay);
            Thread.Sleep(50);
            Win32.MoveTo(0, 0);
        }

        public void HoverPosition(Position absolutePosition)
        {
            Win32.MoveTo(absolutePosition.ClickTargetX, absolutePosition.ClickTargetY);
            Win32.MoveTo(0, 0);
        }

        public Position GetAbsoluteAssetPosition(string assetPath, double threshold = 0.95)
        {
            Position foundPosition;
            using (Bitmap search = ScreenCapture.CaptureRectangle(WindowRect))
                foundPosition = OpenCV_Service.FindObject(search, assetPath, threshold);

            if (!foundPosition.IsVisible)
                return foundPosition;
            return TranslatePosition(foundPosition);
        }

        public bool ActivateTab(string tabName, int testCycle = 1)
        {
            Logger.Console.Debug($"Activate Tab {tabName}, Cycle {testCycle}");

            // Have tried this 20 times, return failure
            if (testCycle > 20)
                return false;

            if (!OpenStash())
                return false;

            if (!BringToForeground())
                return false;
            Position absolutePosition = GetAbsoluteAssetPosition(StaticUtils.GetUIFragmentPath($"active_{tabName}"), 0.90);
            if (!absolutePosition.IsVisible) // Active Tab not found, let's look for inactive tab
                absolutePosition = GetAbsoluteAssetPosition(StaticUtils.GetUIFragmentPath($"notactive_{tabName}"));
            if (!absolutePosition.IsVisible) // Nothing was found, Let's cycle incase we are still loading
            {
                Thread.Sleep(1000); // Give it a second to finish what it's doing
                return ActivateTab(tabName, ++testCycle);
            }
            ClickPosition(absolutePosition);
            return true;
        }

        public bool GetItemFromStash(Position itemRelativePosition, string stashTab)
        {
            // Activate the Tab
            if (!ActivateTab(stashTab))
                return false;
            CtrlClickPosition(TranslatePosition(itemRelativePosition));

            // ToDo: Validate the item is in the Inventory
            return true;
        }

        public bool ClearInventory(string recycle_tab = "recycle_tab")
        {
            Logger.Console.Debug($"Clearing Inventory to {recycle_tab}.");
            // Activate Recycle Tab
            if (!ActivateTab(recycle_tab))
                return false;

            // Now move everything from Inventory to Stash
            // We do not care what the item is at this point just CTRL+Click every block
            // The speed is very fast, cycle twice just to make sure everything is moved.
            DumpInventory();
            Logger.Console.Debug("Clearing Inventory Complete.");
            return true;
        }

        public bool DumpInventory()
        {
            for (int i = 0; i < 2; i++)
                foreach (Position invData in InventoryPositions.GetInvenoryPositions(ResolutionEnum))
                    CtrlClickPosition(TranslatePosition(invData), 10);
            return true;
        }

        public ItemInfoParser GetItemInfo(Position position, bool isRelativePosition = true)
        {
            Position absolutePosition = position.Clone();
            if (isRelativePosition)
                absolutePosition = TranslatePosition(position);
            if (!BringToForeground())
                return new ItemInfoParser();
            Win32.MoveTo(absolutePosition.ClickTargetX, absolutePosition.ClickTargetY);
            Thread.Sleep(10);
            Clipboard.Clear();
            SendKey("^c");
            Thread.Sleep(100);
            string ss = Win32.GetText();
            Win32.MoveTo(0, 0);
            if (string.IsNullOrWhiteSpace(ss))
                return new ItemInfoParser();
            return new ItemInfoParser(ss);
        }

        public Tab GetTabData(string tabName = "trade_tab")
        {
            Logger.Console.Debug($"Tab Scan of {tabName}.");

            Tab returnTab = new Tab();

            if (!ActivateTab(tabName))
                return returnTab;

            foreach (Position stashData in StashPositions.GetStashPositions(ResolutionEnum))
            {
                if (stashData == null) continue;
                ItemInfoParser itemParser = GetItemInfo(stashData);
                if (itemParser.Item.Name == "Not For Sell")
                    continue;
                itemParser.AddPlace(stashData.ClickTargetX, stashData.ClickTargetY);
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

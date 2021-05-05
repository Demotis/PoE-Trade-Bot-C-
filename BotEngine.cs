using PoE_Trade_Bot.Models;
using PoE_Trade_Bot.Models.Test;
using PoE_Trade_Bot.PoEClient;
using PoE_Trade_Bot.Services;
using PoE_Trade_Bot.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace PoE_Trade_Bot
{
    public class BotEngine
    {

        private readonly int Top_Stash64 = 135;
        private readonly int Left_Stash64 = 25;

        public static List<CustomerInfo> Customer;
        public static List<CustomerInfo> CompletedTrades;

        private Tab _Tab;

        public BotEngine()
        {
            Customer = new List<CustomerInfo>();
            CompletedTrades = new List<CustomerInfo>();
        }

        public void StartBot()
        {
            _Tab = new Tab();
            StartTrader_PoEbota();
            Console.ReadKey();
        }

        //Trade Functions

        private void StartTrader_PoEbota()
        {
            // Travel to the hideout
            ClientManager.Instance.ChatCommand(Enums.ChatCommand.GOTO_MY_HIDEOUT.GetDescription());

            bool IsFirstTime = true;

            DateTime timer = DateTime.Now + new TimeSpan(0, new Random().Next(4, 6), 0);

            while (true)
            {
                if ((ClientManager.Instance.IsAFK && !Customer.Any()) || (!Customer.Any() && timer < DateTime.Now))
                {
                    ClientManager.Instance.BringToForeground();

                    ClientManager.Instance.ChatCommand("&I am here");

                    timer = DateTime.Now + new TimeSpan(0, new Random().Next(4, 6), 0);

                    ClientManager.Instance.IsAFK = false;
                }

                if (IsFirstTime)
                {
                    if (!ClientManager.Instance.OpenStash())
                    {
                        IsFirstTime = false;
                        throw new Exception("Stash is not found in the area.");
                    }

                    ClientManager.Instance.ClearInventory();
                    ScanTab();
                    IsFirstTime = false;
                }

                if (Customer.Any() && !IsFirstTime)
                {
                    ClientManager.Instance.BringToForeground();

                    Logger.Console.Info($"\nTrade start with {Customer.First().Nickname}");

                    #region Many items

                    if (Customer.First().OrderType == CustomerInfo.OrderTypes.MANY)
                    {
                        InviteCustomer();

                        if (!ClientManager.Instance.OpenStash())
                        {
                            KickFormParty();
                            Customer.Remove(Customer.First());

                            Logger.Console.Info("\nTrade end!");
                            continue;
                        }

                        if (!TakeItems())
                        {
                            KickFormParty();
                            Customer.Remove(Customer.First());

                            Logger.Console.Info("\nTrade end!");
                            continue;
                        }

                        //check is area contain customer
                        if (!CheckArea())
                        {
                            KickFormParty();

                            Customer.Remove(Customer.First());

                            Logger.Console.Info("\nTrade end!");
                            continue;
                        }

                        //start trade
                        if (!TradeQuery())
                        {
                            KickFormParty();

                            Customer.Remove(Customer.First());

                            Logger.Console.Info("\nTrade end!");
                            continue;
                        }


                        if (!PutItems())
                        {
                            KickFormParty();

                            Customer.Remove(Customer.First());

                            Logger.Console.Info("\nTrade end!");
                            continue;
                        }

                        if (!CheckCurrency())
                        {
                            KickFormParty();

                            Customer.Remove(Customer.First());

                            Logger.Console.Info("\nTrade end!");
                            continue;
                        }
                    }

                    #endregion

                    #region Single item

                    if (Customer.First().OrderType == CustomerInfo.OrderTypes.SINGLE)
                    {
                        InviteCustomer();

                        if (!ClientManager.Instance.OpenStash())
                        {
                            KickFormParty();
                            Customer.Remove(Customer.First());

                            Logger.Console.Info("\nTrade end!");
                            continue;
                        }

                        if (!TakeProduct())
                        {
                            KickFormParty();
                            Customer.Remove(Customer.First());

                            Logger.Console.Info("\nTrade end!");
                            continue;
                        }

                        //check is area contain customer
                        if (!CheckArea())
                        {
                            KickFormParty();

                            Customer.Remove(Customer.First());

                            Logger.Console.Info("\nTrade end!");
                            continue;
                        }

                        //start trade
                        if (!TradeQuery())
                        {
                            KickFormParty();

                            Customer.Remove(Customer.First());

                            Logger.Console.Info("\nTrade end!");
                            continue;
                        }

                        //get product
                        if (!GetProduct())
                        {
                            KickFormParty();

                            Customer.Remove(Customer.First());

                            Logger.Console.Info("\nTrade end!");
                            continue;
                        }

                        //search and check currency
                        if (!CheckCurrency())
                        {
                            KickFormParty();

                            Customer.Remove(Customer.First());

                            Logger.Console.Info("\nTrade end!");
                            continue;
                        }
                    }

                    #endregion



                    ClientManager.Instance.ChatCommand($"@{Customer.First().Nickname} ty gl");

                    KickFormParty();

                    CompletedTrades.Add(Customer.First());

                    Customer.Remove(Customer.First());

                    if (!ClientManager.Instance.OpenStash())
                    {
                        Logger.Console.Warn("Stash not found. I cant clean inventory after trade.");
                    }
                    else
                    {
                        ClientManager.Instance.ClearInventory();
                    }

                    Logger.Console.Info("Trade comlete sccessfull");
                }

                Thread.Sleep(100);
            }
        }

        private void InviteCustomer()
        {
            ClientManager.Instance.BringToForeground();

            Logger.Console.Info("Invite in party...");

            string command = "/invite " + Customer.First().Nickname;

            ClientManager.Instance.ChatCommand(command);
        }

        private bool TakeProduct()
        {
            Bitmap screen_shot = null;
            Position found_pos = null;

            Logger.Console.Debug("Search trade tab...");

            for (int count_try = 0; count_try < 16; count_try++)
            {
                screen_shot = ScreenCapture.CaptureRectangle(10, 90, 450, 30);

                found_pos = OpenCV_Service.FindObject(screen_shot, StaticUtils.GetUIFragmentPath("notactive_trade_tab"));

                if (found_pos.IsVisible)
                    break;
                else
                {
                    found_pos = OpenCV_Service.FindObject(screen_shot, StaticUtils.GetUIFragmentPath("active_trade_tab"));
                    if (found_pos.IsVisible)
                    {
                        break;
                    }
                }

                screen_shot.Dispose();

                Thread.Sleep(500);
            }

            screen_shot.Dispose();

            if (found_pos.IsVisible)
            {
                Win32.MoveTo(10 + found_pos.Left + found_pos.Width / 2, 90 + found_pos.Top + found_pos.Height / 2);

                Thread.Sleep(100);

                Win32.DoMouseClick();

                Thread.Sleep(300);

                Win32.MoveTo(Left_Stash64 + 38 * (Customer.First().Left - 1), Top_Stash64 + 38 * (Customer.First().Top - 1));

                Thread.Sleep(1000);

                string ctrlc = CtrlC_PoE();

                string product_clip = GetNameItem_PoE(ctrlc);

                if (product_clip == null || !Customer.First().Product.Contains(product_clip))
                {
                    Logger.Console.Info("not found item");

                    ClientManager.Instance.ChatCommand($"@{Customer.First().Nickname} I sold it, sry");

                    ClientManager.Instance.SendKey("{ESC}");

                    return false;
                }

                if (!IsValidPrice(ctrlc))
                {
                    Logger.Console.Info("Fake price");

                    ClientManager.Instance.ChatCommand($"@{Customer.First().Nickname} It is not my price!");

                    ClientManager.Instance.SendKey("{ESC}");

                    return false;
                }

                Win32.CtrlMouseClick();

                Thread.Sleep(100);

                Win32.MoveTo(750, 350);

                ClientManager.Instance.SendKey("{ESC}");

                return true;

            }

            Logger.Console.Warn("Trade tab is not found");

            return false;
        }

        private bool CheckArea()
        {
            Logger.Console.Debug("Check area...");
            for (int i = 0; i < 60; i++)
            {
                if (Customer.First().IsInArea)
                {
                    return true;
                }
                Thread.Sleep(500);
            }
            Logger.Console.Warn("Player not here");
            return false;
        }

        private bool TradeQuery()
        {
            Position found_pos = null;

            Bitmap screen_shot = null;

            bool amIdoRequest = false;

            for (int try_count = 0; try_count < 3; try_count++)
            {
                Logger.Console.Debug("Try to accept or do trade...");

                for (int i = 0; i < 10; i++)
                {
                    if (!amIdoRequest)
                    {
                        screen_shot = ScreenCapture.CaptureRectangle(1030, 260, 330, 500);

                        found_pos = OpenCV_Service.FindObject(screen_shot, StaticUtils.GetUIFragmentPath("accespt"));

                        if (found_pos.IsVisible)
                        {
                            Logger.Console.Debug("I will Accept trade request!");

                            Win32.MoveTo(1030 + found_pos.Left + found_pos.Width / 2, 260 + found_pos.Top + found_pos.Height / 2);

                            Thread.Sleep(100);

                            Win32.DoMouseClick();

                            Thread.Sleep(100);

                            Win32.MoveTo((1030 + screen_shot.Width) / 2, (260 + screen_shot.Height) / 2);

                            amIdoRequest = true;
                        }
                        else
                        {
                            Logger.Console.Debug("i write trade");
                            string trade_command = "/tradewith " + Customer.First().Nickname;

                            ClientManager.Instance.ChatCommand(trade_command);

                            screen_shot = ScreenCapture.CaptureRectangle(455, 285, 475, 210);

                            if (!Customer.First().IsInArea)
                                return false;

                            found_pos = OpenCV_Service.FindObject(screen_shot, StaticUtils.GetUIFragmentPath("trade_waiting"));
                            if (found_pos.IsVisible)
                            {
                                Win32.MoveTo(455 + found_pos.Left + found_pos.Width / 2, 285 + found_pos.Top + found_pos.Height / 2);

                                screen_shot.Dispose();

                                amIdoRequest = true;
                            }
                            else
                            {
                                Logger.Console.Debug("Check trade window");
                                screen_shot = ScreenCapture.CaptureRectangle(330, 15, 235, 130);
                                found_pos = OpenCV_Service.FindObject(screen_shot, StaticUtils.GetUIFragmentPath("trade_window_title"));
                                if (found_pos.IsVisible)
                                {
                                    Logger.Console.Debug("I am in trade!");
                                    screen_shot.Dispose();
                                    return true;
                                }
                            }
                        }
                    }
                    else
                    {
                        Logger.Console.Debug("Check trade window");
                        screen_shot = ScreenCapture.CaptureRectangle(330, 15, 235, 130);
                        found_pos = OpenCV_Service.FindObject(screen_shot, StaticUtils.GetUIFragmentPath("trade_window_title"));
                        if (found_pos.IsVisible)
                        {
                            screen_shot.Dispose();
                            Logger.Console.Debug("I am in trade!");
                            return true;
                        }
                    }
                    Thread.Sleep(500);
                }

            }
            return false;
        }

        private bool GetProduct()
        {
            int x_inventory = 925;
            int y_inventory = 440;
            int offset = 37;

            Bitmap screen_shot;

            for (int j = 0; j < 12; j++)
            {
                for (int i = 0; i < 5; i++)
                {
                    Win32.MoveTo(x_inventory + offset * j, y_inventory + 175);

                    Thread.Sleep(100);

                    screen_shot = ScreenCapture.CaptureRectangle(x_inventory - 30 + offset * j, y_inventory - 30 + offset * i, 60, 60);

                    Position pos = OpenCV_Service.FindObject(screen_shot, StaticUtils.GetUIFragmentPath("empty_cel"), 0.4);

                    if (!pos.IsVisible)
                    {
                        Clipboard.Clear();

                        string ss = null;

                        Thread.Sleep(100);

                        Win32.MoveTo(x_inventory + offset * j, y_inventory + offset * i);

                        var time = DateTime.Now + new TimeSpan(0, 0, 5);

                        while (ss == null)
                        {
                            ClientManager.Instance.SendKey("^c");
                            ss = Win32.GetText();

                            if (time < DateTime.Now)
                                ss = "empty_string";
                        }

                        if (ss == "empty_string")
                            continue;

                        if (Customer.First().Product.Contains(GetNameItem_PoE(ss)))
                        {
                            Logger.Console.Debug($"{ss} is found in inventory");

                            Win32.CtrlMouseClick();

                            screen_shot.Dispose();

                            return true;
                        }

                    }
                    screen_shot.Dispose();
                }
            }
            ClientManager.Instance.SendKey("{ESC}");

            ClientManager.Instance.ChatCommand("@" + Customer.First().Nickname + " i sold it, sry");

            return false;
        }

        private bool CheckCurrency()
        {
            List<Position> found_positions = null;

            List<Currency_ExRate> main_currs = new List<Currency_ExRate>();

            //set main currencies

            main_currs.Add(Customer.First().Currency);

            if (Customer.First().Currency.Name != "chaos orb")
            {
                main_currs.Add(PoECurrencyManager.Instance.Currencies.GetCurrencyByName("chaos"));
            }

            if (Customer.First().Currency.Name != "divine orb")
            {
                main_currs.Add(PoECurrencyManager.Instance.Currencies.GetCurrencyByName("divine"));
            }

            if (Customer.First().Currency.Name != "exalted orb")
            {
                main_currs.Add(PoECurrencyManager.Instance.Currencies.GetCurrencyByName("exalted"));
            }

            if (Customer.First().Currency.Name != "orb of alchemy")
            {
                main_currs.Add(PoECurrencyManager.Instance.Currencies.GetCurrencyByName("alchemy"));
            }

            if (Customer.First().Currency.Name == "exalted orb")
            {
                ClientManager.Instance.ChatCommand($"@{Customer.First().Nickname} exalted orb = {PoECurrencyManager.Instance.Currencies.GetCurrencyByName("exalted").ChaosEquivalent}");

                main_currs.Add(PoECurrencyManager.Instance.Currencies.GetCurrencyByName("exalted"));
            }

            Bitmap screen_shot = null;

            int x_trade = 220;
            int y_trade = 140;

            for (int i = 0; i < 30; i++)
            {
                double price = 0;

                foreach (Currency_ExRate cur in main_currs)
                {
                    Win32.MoveTo(0, 0);

                    Thread.Sleep(100);

                    screen_shot = ScreenCapture.CaptureRectangle(x_trade, y_trade, 465, 200);

                    found_positions = OpenCV_Service.FindCurrencies(screen_shot, cur.ImageName, 0.6);

                    foreach (Position pos in found_positions)
                    {
                        Win32.MoveTo(x_trade + pos.Left + pos.Width / 2, y_trade + pos.Top + pos.Height / 2);

                        Thread.Sleep(100);

                        string ctrlc = CtrlC_PoE();

                        var curbyname = PoECurrencyManager.Instance.Currencies.GetCurrencyByName(GetNameItem_PoE(ctrlc));

                        if (curbyname == null)

                            price += GetSizeInStack(CtrlC_PoE()) * cur.ChaosEquivalent;

                        else

                            price += GetSizeInStack(CtrlC_PoE()) * curbyname.ChaosEquivalent;


                        screen_shot.Dispose();
                    }

                    if (price >= Customer.First().Chaos_Price && price != 0)
                        break;
                }

                Logger.Console.Info("Bid price (in chaos) = " + price + " Necessary (in chaos) = " + Customer.First().Chaos_Price);

                if (price >= Customer.First().Chaos_Price)
                {
                    Logger.Console.Debug("I want accept trade");

                    screen_shot = ScreenCapture.CaptureRectangle(200, 575, 130, 40);

                    Position pos = OpenCV_Service.FindObject(screen_shot, StaticUtils.GetUIFragmentPath("accept_tradewindow"));

                    if (pos.IsVisible)
                    {
                        Win32.MoveTo(210 + pos.Left + pos.Width / 2, 580 + pos.Top + pos.Height / 2);

                        Thread.Sleep(100);

                        Win32.DoMouseClick();

                        screen_shot.Dispose();

                        var timer = DateTime.Now + new TimeSpan(0, 0, 5);

                        while (Customer.First().TradeStatus != CustomerInfo.TradeStatuses.ACCEPTED)
                        {
                            if (Customer.First().TradeStatus == CustomerInfo.TradeStatuses.CANCELED)
                                return false;

                            if (DateTime.Now > timer)
                                break;
                        }

                        if (Customer.First().TradeStatus == CustomerInfo.TradeStatuses.ACCEPTED)
                            return true;

                        else continue;

                    }
                }
                else
                {
                    screen_shot.Dispose();
                }

                Thread.Sleep(500);
            }

            ClientManager.Instance.SendKey("{ESC}");

            return false;
        }

        private void KickFormParty()
        {
            ClientManager.Instance.ChatCommand("/kick " + Customer.First().Nickname);
        }

        //for many items

        private void ScanTab(string name_tab = "trade_tab")
        {
            Position found_pos = null;

            Logger.Console.Debug($"Search {name_tab} trade tab...");

            for (int count_try = 0; count_try < 16; count_try++)
            {
                var screen_shot = ScreenCapture.CaptureRectangle(10, 90, 450, 30);

                found_pos = OpenCV_Service.FindObject(screen_shot, StaticUtils.GetUIFragmentPath($"notactive_{name_tab}"));

                if (found_pos.IsVisible)
                {
                    break;
                }
                else
                {
                    found_pos = OpenCV_Service.FindObject(screen_shot, StaticUtils.GetUIFragmentPath($"active_{name_tab}"));
                    if (found_pos.IsVisible)
                    {
                        screen_shot.Dispose();

                        break;
                    }
                }
                screen_shot.Dispose();

                Thread.Sleep(500);
            }

            if (found_pos.IsVisible)
            {
                Win32.MoveTo(10 + found_pos.Left + found_pos.Width / 2, 90 + found_pos.Top + found_pos.Height / 2);

                Thread.Sleep(200);

                Win32.DoMouseClick();

                Thread.Sleep(250);

                List<Cell> skip = new List<Cell>();

                for (int i = 0; i < 12; i++)
                {
                    for (int j = 0; j < 12; j++)
                    {
                        if (skip.Find(cel => cel.Left == i && cel.Top == j) != null)
                        {
                            continue;
                        }

                        Win32.MoveTo(0, 0);

                        Thread.Sleep(100);

                        Win32.MoveTo(Left_Stash64 + 38 * i, Top_Stash64 + 38 * j);

                        #region OpenCv

                        var screen_shot = ScreenCapture.CaptureRectangle(Left_Stash64 - 30 + 38 * i, Top_Stash64 - 30 + 38 * j, 60, 60);

                        Position pos = OpenCV_Service.FindObject(screen_shot, StaticUtils.GetUIFragmentPath("empty_cel"), 0.5);

                        if (!pos.IsVisible)
                        {
                            #region Good code

                            string item_info = CtrlC_PoE();

                            if (item_info != "empty_string")
                            {
                                var item = new Item
                                {
                                    Price = GetPrice_PoE(item_info),
                                    Name = GetNameItem_PoE_Pro(item_info),
                                    StackSize = GetStackSize_PoE_Pro(item_info)
                                };

                                item.Places.Add(new Cell(i, j));

                                if (item.StackSize == 1)
                                {
                                    item.SizeInStack = 1;
                                }
                                else
                                {
                                    item.SizeInStack = (int)GetSizeInStack(item_info);
                                }

                                if (item.Name.Contains("Resonator"))
                                {
                                    if (item.Name.Contains("Potent"))
                                    {
                                        item.Places.Add(new Cell(i, j + 1));
                                        skip.Add(new Cell(i, j + 1));

                                    }

                                    if (item.Name.Contains("Prime") || item.Name.Contains("Powerful"))
                                    {
                                        item.Places.Add(new Cell(i, j + 1));
                                        skip.Add(new Cell(i, j + 1));
                                        item.Places.Add(new Cell(i + 1, j + 1));
                                        skip.Add(new Cell(i + 1, j + 1));
                                        item.Places.Add(new Cell(i + 1, j));
                                        skip.Add(new Cell(i + 1, j));
                                    }
                                }

                                _Tab.AddItem(item);

                                #endregion
                            }

                            screen_shot.Dispose();

                            #endregion
                        }
                    }
                }

                ClientManager.Instance.SendKey("{ESC}");

                Logger.Console.Debug("Scan is end!");
            }
            else
            {
                throw new Exception($"{name_tab} not found.");
            }
        }

        private bool TakeItems(string name_tab = "trade_tab")
        {
            Position found_pos = null;

            Logger.Console.Debug($"Search {name_tab} trade tab...");

            for (int count_try = 0; count_try < 16; count_try++)
            {
                var screen_shot = ScreenCapture.CaptureRectangle(10, 90, 450, 30);

                found_pos = OpenCV_Service.FindObject(screen_shot, StaticUtils.GetUIFragmentPath($"notactive_{name_tab}"));

                if (found_pos.IsVisible)
                    break;
                else
                {
                    found_pos = OpenCV_Service.FindObject(screen_shot, StaticUtils.GetUIFragmentPath($"active_{name_tab}"));
                    if (found_pos.IsVisible)
                    {
                        screen_shot.Dispose();

                        break;
                    }
                }
                screen_shot.Dispose();

                Thread.Sleep(500);
            }

            if (found_pos.IsVisible)
            {
                Win32.MoveTo(10 + found_pos.Left + found_pos.Width / 2, 90 + found_pos.Top + found_pos.Height / 2);

                Thread.Sleep(200);

                Win32.DoMouseClick();

                Thread.Sleep(250);

                var customer = Customer.First();

                var min_price = new Price
                {
                    Cost = customer.Cost,
                    CurrencyType = customer.Currency,
                    ForNumberItems = customer.NumberProducts
                };

                var items = _Tab.GetItems(customer.NumberProducts, customer.Product, min_price);

                if (items.Any())
                {
                    int TotalAmount = 0;

                    foreach (Item i in items)
                    {
                        TotalAmount += i.SizeInStack;

                        Win32.MoveTo(Left_Stash64 + 38 * i.Places.First().Left, Top_Stash64 + 38 * i.Places.First().Top);

                        Thread.Sleep(100);

                        string item_info = CtrlC_PoE();

                        if (!item_info.Contains(i.Name))
                        {
                            Logger.Console.Info("Information incorrect.");

                            return false;
                        }

                        if (TotalAmount > customer.NumberProducts)
                        {
                            TotalAmount -= i.SizeInStack;
                            int necessary = customer.NumberProducts - TotalAmount;
                            i.SizeInStack -= necessary;
                            _Tab.AddItem(i);
                            TotalAmount += necessary;
                            Win32.ShiftClick();
                            Thread.Sleep(100);
                            ClientManager.Instance.SendNumber(necessary);
                            ClientManager.Instance.SendKey("{ENTER}");
                            PutInInventory();

                        }
                        else
                        {
                            Win32.CtrlMouseClick();
                        }


                        if (TotalAmount == customer.NumberProducts)
                        {
                            ClientManager.Instance.SendKey("{ESC}");

                            return true;
                        }
                    }
                }
                else
                {
                    Logger.Console.Info("Items not found!");

                    ClientManager.Instance.ChatCommand($"@{customer.Nickname} maybe I sold it");
                }

            }

            Logger.Console.Warn("Tab not found");

            return false;
        }

        private void PutInInventory()
        {
            var screen_shot = ScreenCapture.CaptureRectangle(900, 420, 460, 200);

            var empty_poss = OpenCV_Service.FindObjects(screen_shot, StaticUtils.GetUIFragmentPath("empty_cel"), 0.5);

            if (empty_poss.Any())
            {
                foreach (Position pos in empty_poss)
                {
                    Win32.MoveTo(900 + pos.Left, 420 + pos.Top);

                    var info = CtrlC_PoE();

                    Thread.Sleep(100);

                    if (info == "empty_string")
                    {
                        Win32.DoMouseClick();

                        Thread.Sleep(150);

                        screen_shot.Dispose();

                        return;
                    }
                }
            }

            else
                Logger.Console.Fatal("Inventory is full");
        }

        private bool PutItems()
        {
            int x_inventory = 925;
            int y_inventory = 440;
            int offset = 37;

            var customer = Customer.First();

            int TotalAmount = 0;

            List<Cell> skip = new List<Cell>();

            for (int j = 0; j < 12; j++)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (skip.Find(cel => cel.Left == i && cel.Top == j) != null)
                    {
                        continue;
                    }

                    Win32.MoveTo(x_inventory + offset * j, y_inventory + 175);

                    Thread.Sleep(100);

                    var screen_shot = ScreenCapture.CaptureRectangle(x_inventory - 30 + offset * j, y_inventory - 30 + offset * i, 60, 60);

                    var pos = OpenCV_Service.FindObject(screen_shot, StaticUtils.GetUIFragmentPath("empty_cel"), 0.5);

                    if (!pos.IsVisible)
                    {
                        Win32.MoveTo(x_inventory + offset * j, y_inventory + offset * i);

                        var item_info = CtrlC_PoE();

                        string name = GetNameItem_PoE_Pro(item_info);

                        if (name != customer.Product)
                        {
                            continue;
                        }

                        int SizeInStack = GetStackSize_PoE_Pro(item_info);

                        TotalAmount += SizeInStack;

                        if (name.Contains("Resonator"))
                        {
                            if (name.Contains("Potent"))
                            {
                                skip.Add(new Cell(i, j + 1));

                            }

                            if (name.Contains("Prime") || name.Contains("Powerful"))
                            {
                                skip.Add(new Cell(i, j + 1));
                                skip.Add(new Cell(i + 1, j + 1));
                                skip.Add(new Cell(i + 1, j));
                            }
                        }

                        Win32.CtrlMouseClick();

                        Thread.Sleep(250);

                        if (TotalAmount >= customer.NumberProducts)
                        {
                            screen_shot.Dispose();

                            Logger.Console.Debug($"I put {TotalAmount} items in trade window");

                            return true;
                        }
                    }

                    screen_shot.Dispose();
                }
            }
            ClientManager.Instance.SendKey("{ESC}");

            ClientManager.Instance.ChatCommand("@" + Customer.First().Nickname + " i sold it, sry");

            return false;
        }

        //util

        private int GetStackSize_PoE_Pro(string item_info)
        {
            if (!item_info.Contains("Stack Size:"))
                return 1;

            int res = Convert.ToInt32(Regex.Match(item_info, @"Stack Size: [0-9.]+/([0-9.]+)").Groups[1].Value);

            return res;
        }

        private string GetNameItem_PoE_Pro(string item_info)
        {
            if (item_info.Contains("Rarity: Currency"))
            {
                string str = Regex.Match(item_info, @"Rarity: Currency\s([\w ']+)").Groups[1].Value;
                return str;
            }

            if (item_info.Contains("Map Tier:"))
            {
                if (item_info.Contains("Rarity: Rare"))
                {
                    var match = Regex.Match(item_info, @"Rarity: Rare\s([\w ']*)\s([\w ']*)");

                    if (!string.IsNullOrEmpty(match.Groups[2].Value))
                    {
                        return match.Groups[2].Value.Replace(" Map", "");
                    }
                    else
                        return match.Groups[1].Value.Replace(" Map", "");
                }

                if (item_info.Contains("Rarity: Normal"))
                {
                    return Regex.Match(item_info, @"Rarity: Normal\s([\w ']*)").Groups[1].Value.Replace(" Map", "");
                }

                if (item_info.Contains("Rarity: Unique"))
                {
                    var match = Regex.Match(item_info, @"Rarity: Unique\s([\w ']*)\s([\w ']*)");

                    if (!string.IsNullOrEmpty(match.Groups[2].Value))
                    {
                        return $"{match.Groups[1].Value} {match.Groups[2].Value}".Replace(" Map", "");
                    }
                    else
                        return "Undefined item";
                }

            }

            if (item_info.Contains("Rarity: Divination Card"))
            {
                return Regex.Match(item_info, @"Rarity: Divination Card\s([\w ']*)").Groups[1].Value;
            }

            //I think that it for predicate fragments
            if (!item_info.Contains("Requirements:"))
            {
                if (item_info.Contains("Rarity: Normal"))
                {
                    return Regex.Match(item_info, @"Rarity: Normal\s([\w ']*)").Groups[1].Value;
                }
            }

            return "Not For Sell";

        }

        private string GetNameItem_PoE(string clip)
        {
            if (!String.IsNullOrEmpty(clip) && clip != "empty_string")
            {
                var lines = clip.Split('\n');

                if (lines.Count() == 1)
                    return null;

                if (!lines[2].Contains("---"))
                {
                    return lines[1].Replace("\r", "") + " " + lines[2].Replace("\r", "");
                }
                else
                    return lines[1].Replace("\r", "");

            }
            return null;
        }

        private bool IsValidPrice(string ctrlC_PoE)
        {
            bool isvalidprice = false;
            bool isvalidcurrency = false;

            if (!String.IsNullOrEmpty(ctrlC_PoE) && ctrlC_PoE != "empty_string")
            {
                var lines = ctrlC_PoE.Split('\n');

                foreach (string str in lines)
                {
                    if (str.Contains("Note: ~price"))
                    {
                        var result = Regex.Replace(str, "[^0-9.]", "");

                        double price = Convert.ToDouble(result);

                        if (price <= Customer.First().Cost)
                            isvalidprice = true;

                        int length = str.Length - 1;
                        int begin = 0;

                        for (int i = length; i > 0; i--)
                        {
                            if (str[i] == ' ')
                            {
                                begin = i + 1;
                                break;
                            }
                        }

                        result = str.Substring(begin, str.Length - begin).Replace("\r", "");

                        if (PoECurrencyManager.Instance.Currencies.GetCurrencyByName(result).Name == Customer.First().Currency.Name)
                        {
                            isvalidcurrency = true;
                        }


                        if (isvalidcurrency && isvalidprice)
                            return true;
                    }
                }
            }
            return false;
        }

        private double GetSizeInStack(string ctrlC_PoE)
        {
            if (!String.IsNullOrEmpty(ctrlC_PoE) && ctrlC_PoE != "empty_string")
            {
                int begin = ctrlC_PoE.IndexOf("Stack Size: ") + 12;
                int length = ctrlC_PoE.IndexOf("/") - begin;

                return Convert.ToDouble(ctrlC_PoE.Substring(begin, length));
            }
            return 0;
        }

        private string CtrlC_PoE()
        {
            Clipboard.Clear();

            string ss = null;

            Thread.Sleep(100);

            var time = DateTime.Now + new TimeSpan(0, 0, 1);

            while (ss == null)
            {
                ClientManager.Instance.SendKey("^c");
                ss = Win32.GetText();

                if (time < DateTime.Now)
                    ss = "empty_string";
            }

            return ss.Replace("\r", "");
        }

        public Price GetPrice_PoE(string item_info)
        {
            Price price = new Price();

            if (!item_info.Contains("Note: ~price"))
                return new Price();

            if (Regex.IsMatch(item_info, "~price [0-9.]+/[0-9.]+"))
            {
                price.Cost = Convert.ToDouble(Regex.Replace(item_info, @"([\w\s\W\n]+Note: ~price )|(/+[\w\s\W]*)|([^0-9.])", ""));

                price.ForNumberItems = Convert.ToInt32(Regex.Replace(item_info, @"([\w\s\W]+/)|([^0-9.])", ""));

                price.CurrencyType = PoECurrencyManager.Instance.Currencies.GetCurrencyByName(Regex.Replace(item_info, @"[\w\s\W]+\d+\s|\n", ""));
            }
            if (Regex.IsMatch(item_info, @"~price +[0-9.]+\s\D*"))
            {
                price.Cost = Convert.ToDouble(Regex.Replace(item_info, @"[\w\W]*~price |[^0-9.]*", "").Replace('.', ','));

                price.ForNumberItems = GetStackSize_PoE_Pro(item_info);

                price.CurrencyType = PoECurrencyManager.Instance.Currencies.GetCurrencyByName(Regex.Replace(item_info, @"[\w\s\W]+\d+\s|\n", ""));
            }

            if (!price.IsSet)
                return new Price();
            else
                return price;
        }
    }
}
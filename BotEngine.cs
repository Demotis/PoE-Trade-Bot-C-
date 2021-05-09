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
        public static List<CustomerInfo> CustomerQueue;
        public static List<CustomerInfo> CompletedTrades;

        private Tab TradeTabData;

        public BotEngine()
        {
            CustomerQueue = new List<CustomerInfo>();
            CompletedTrades = new List<CustomerInfo>();
            TradeTabData = new Tab();

            // Start Currencies Service
            PoECurrencyManager.Instance.StartService();
            LogManager.Instance.StartService();
        }

        public void StartBot()
        {
            ClientManager.Instance.ChatCommand(Enums.ChatCommand.AFK_OFF.GetDescription());
            ClientManager.Instance.ChatCommand(Enums.ChatCommand.GOTO_MY_HIDEOUT.GetDescription());
            PrepareTradeData();
            StartTrader_PoEbota();
            Console.ReadKey();
        }

        private void PrepareTradeData()
        {
            if (!ClientManager.Instance.OpenStash())
                throw new Exception("Stash is not found in the area.");
            ClientManager.Instance.ClearInventory();
            TradeTabData = ClientManager.Instance.GetTabData("trade_tab");
        }

        private void StartTrader_PoEbota()
        {
            while (true)
            {
                if (!CustomerQueue.Any())
                {
                    Thread.Sleep(500);
                    continue;
                }

                // We have a customer in queue
                CustomerInfo customer = CustomerQueue.First();

                if (customer.OrderType == CustomerInfo.OrderTypes.CURRENCY &&
                    ProcessCurrencySale(customer))
                    ClientManager.Instance.ChatCommand($"@{customer.Nickname} Thank you for the trade.");

                if (customer.OrderType == CustomerInfo.OrderTypes.ITEM &&
                    ProcessItemSale(customer))
                    ClientManager.Instance.ChatCommand($"@{customer.Nickname} Thank you for the trade.");

                // Cleanup After Trade - We are going to send the Kick command even if we didn't add customer
                BotEngineUtils.KickFromParty(customer);
                CompletedTrades.Add(customer);
                CustomerQueue.Remove(customer);
                if (!ClientManager.Instance.ClearInventory())
                    Logger.Console.Error("Stash not found. I cant clean inventory after trade.");

                Logger.Console.Info("Trade Complete");
            }
        }

        private bool ProcessCurrencySale(CustomerInfo customer)
        {
            if (!ClientManager.Instance.OpenStash())
                return NotifyItemSold(customer); // We have an issue opening the Stash

            if (!TakeItems())
                return NotifyItemSold(customer);

            // Trade Ready let's send the Invite
            if (!BotEngineUtils.InviteCustomer(customer))
                return false;

            //start trade
            if (!OpenTradeWindow(customer))
                return false;

            if (!PutItems())
            {
                Logger.Console.Info("\nTrade end!");
                continue;
            }

            if (!CheckCurrency())
            {
                Logger.Console.Info("\nTrade end!");
                continue;
            }
            return true;
        }

        private bool ProcessItemSale(CustomerInfo customer)
        {
            if (!ClientManager.Instance.OpenStash())
                return NotifyItemSold(customer); // We have an issue opening the Stash

            // Get the Item from Stash to Inventory
            if (!TakeProduct(customer))
                return NotifyItemSold(customer);

            // Trade Ready let's send the Invite
            if (!BotEngineUtils.InviteCustomer(customer))
                return false;

            //start trade
            if (!OpenTradeWindow(customer))
                return false;

            //get product
            if (!GetProduct(customer))
                return false;

            //search and check currency
            if (!CheckCurrency())
                return false;

            return true;
        }


        /// <summary>
        /// Notifies the item sold.
        /// This is also a default message to send if there is an issue with any trade action
        /// </summary>
        /// <param name="customer">The customer.</param>
        private bool NotifyItemSold(CustomerInfo customer)
        {
            Logger.Console.Fatal("There was an issue with sale, please check log.");
            return ClientManager.Instance.ChatCommand($"@{customer.Nickname} Sorry, sold it already.");
        }

        private bool TakeProduct(CustomerInfo customer)
        {
            ClientManager.Instance.ActivateTab("trade_tab");

            // Get item position
            int clientResolution = Convert.ToInt32(ClientManager.Instance.ResolutionEnum.GetDescription());
            Position itemPosition = StashPositions.StashPositionData[clientResolution][customer.Left, customer.Top];
            ItemInfoParser itemInfo = ClientManager.Instance.GetItemInfo(itemPosition);

            // Verify Item Data
            if (string.IsNullOrWhiteSpace(itemInfo.Item.Name) || // Did we find an item in that slot?
                itemInfo.Item.Name == "Not For Sell" || // Is the Item Listed?
                !customer.Product.Contains(itemInfo.Item.Name) || // Is this the item the customer wants?
                itemInfo.Item.Price.Cost > customer.Cost || // Did he offer the asking price?
                itemInfo.Item.Price.CurrencyType.Name == customer.CurrencyType.Name) // And in the right currency?
                return false;

            // Everything is good at this point. Move the item to Inventory
            return ClientManager.Instance.GetItemFromStash(itemPosition, customer.Stash_Tab);
        }

        private bool OpenTradeWindow(CustomerInfo customer)
        {
            bool amIdoRequest = false;
            for (int try_count = 0; try_count < 3; try_count++)
            {
                Logger.Console.Debug("Try to accept or do trade...");

                for (int i = 0; i < 10; i++)
                {
                    if (!amIdoRequest)
                    {

                        Position acceptTradeRequestButton = ClientManager.Instance.GetAbsoluteAssetPosition("accespt");
                        if (acceptTradeRequestButton.IsVisible)
                        {
                            Logger.Console.Debug("Customer Opened Trade Request - Accepting");
                            ClientManager.Instance.ClickPosition(acceptTradeRequestButton);
                            amIdoRequest = true;
                        }
                        else
                        {
                            Logger.Console.Debug("Sending Trade Request to Customer");
                            ClientManager.Instance.ChatCommand($"/tradewith {customer.Nickname}");
                            if (!customer.IsInArea) // Customer Left Area??
                                return false;

                            if (ClientManager.Instance.GetAbsoluteAssetPosition("trade_waiting").IsVisible)
                                amIdoRequest = true;
                            else
                            {
                                Logger.Console.Debug("Check trade window");
                                if (ClientManager.Instance.GetAbsoluteAssetPosition("trade_window_title").IsVisible)
                                    return true;
                            }
                        }
                    }
                    else
                    {
                        Logger.Console.Debug("Check trade window");
                        if (ClientManager.Instance.GetAbsoluteAssetPosition("trade_window_title").IsVisible)
                            return true;
                    }
                    Thread.Sleep(500);
                }

            }
            return false;
        }

        private bool GetProduct(CustomerInfo customer)
        {
            bool itemsMoved = false;
            foreach (Position invData in InventoryPositions.GetInvenoryPositions(ClientManager.Instance.ResolutionEnum))
            {
                ItemInfoParser itemInfo = ClientManager.Instance.GetItemInfo(invData);
                if (!customer.Product.Contains(itemInfo.Item.Name)) continue;
                 
                ClientManager.Instance.CtrlClickPosition(ClientManager.Instance.TranslatePosition(invData));
                itemsMoved = true;
            }
            return itemsMoved;
        }

        private bool CheckCurrency(CustomerInfo customer)
        {
            double chaosValueInTrade = 0.0;
            // Count Currency in trade window



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

                    found_positions = OpenCV_Service.FindCurrencies(screen_shot, StaticUtils.GetCurrencyPath(cur.NormalName), 0.6);

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

                    if (price >= CustomerQueue.First().Chaos_Price && price != 0)
                        break;
                }

                Logger.Console.Info("Bid price (in chaos) = " + price + " Necessary (in chaos) = " + customer.Chaos_Price);

                if (price >= CustomerQueue.First().Chaos_Price)
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

                        while (CustomerQueue.First().TradeStatus != CustomerInfo.TradeStatuses.ACCEPTED)
                        {
                            if (CustomerQueue.First().TradeStatus == CustomerInfo.TradeStatuses.CANCELED)
                                return false;

                            if (DateTime.Now > timer)
                                break;
                        }

                        if (CustomerQueue.First().TradeStatus == CustomerInfo.TradeStatuses.ACCEPTED)
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



        private bool TakeItems(string tabName = "trade_tab")
        {
            Logger.Console.Info($"Gettting Items for Trade");

            if (!ClientManager.Instance.ActivateTab(tabName))
                return false;

            var customer = CustomerQueue.First();

            var min_price = new Price
            {
                Cost = customer.Cost,
                CurrencyType = customer.CurrencyType,
                ForNumberItems = customer.NumberProducts
            };

            var items = TradeTabData.GetItems(customer.NumberProducts, customer.Product, min_price);

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
                        TradeTabData.AddItem(i);
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
                ClientManager.Instance.ChatCommand($"@{customer.Nickname} Sold Sorry");
            }


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

            var customer = CustomerQueue.First();

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

            ClientManager.Instance.ChatCommand("@" + CustomerQueue.First().Nickname + " i sold it, sry");

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
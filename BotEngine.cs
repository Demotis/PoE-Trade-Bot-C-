using PoETradeBot.Models;
using PoETradeBot.PoEClient;
using PoETradeBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PoETradeBot
{
    public class BotEngine
    {
        public static List<CustomerInfo> CustomerQueue;
        public static List<CustomerInfo> CompletedTrades;

        public BotEngine()
        {
            CustomerQueue = new List<CustomerInfo>();
            CompletedTrades = new List<CustomerInfo>();
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

                if (customer.OrderType == CustomerInfo.OrderTypes.API &&
                    ProcessAPI(customer))
                    ClientManager.Instance.ChatCommand($"@{customer.Nickname} Thank you for the trade.");


                // Cleanup After Trade - We are going to send the Kick command even if we didn't add customer
                BotEngineUtils.KickFromParty(customer);
                CompletedTrades.Add(customer);
                CustomerQueue.Remove(customer);
                ClientManager.Instance.SendKey(" ");
                if (!ClientManager.Instance.ClearInventory())
                    Logger.Console.Error("Stash not found. I cant clean inventory after trade.");

                Logger.Console.Info("Trade Complete");
            }
        }

        private bool ProcessAPI(CustomerInfo customer)
        {
            // Trade Ready let's send the Invite
            if (!BotEngineUtils.InviteCustomer(customer))
                return false;

            switch (customer.ApiAction)
            {
                case CustomerInfo.ApiActions.TAKE:
                    if (!ApiTakeCurrency(customer))
                        return NotifyItemSold(customer);
                    break;
                case CustomerInfo.ApiActions.GET:
                    break;
                case CustomerInfo.ApiActions.PUT:
                    break;
            }
            return ApiMakeTrade(customer);
        }

        private bool ApiTakeCurrency(CustomerInfo customer)
        {
            ClientManager.Instance.ActivateTab("currency_tab");
            Thread.Sleep(500);

            // Get Currency Position
            int clientResolution = Convert.ToInt32(ClientManager.Instance.ResolutionEnum.GetDescription());
            string requestedNormal = customer.Product.Replace(" ", "").Replace("'", "");
            Position currencySlot = ClientManager.Instance.GetAbsoluteAssetPosition(StaticUtils.GetCurrencyPath(requestedNormal), 0.90);
            if (!currencySlot.IsVisible)
                return false;

            ItemInfoParser itemInfo = ClientManager.Instance.GetItemInfo(currencySlot, false);
            int fullStacks = (itemInfo.Item.SizeInStack / itemInfo.Item.StackSize) + 1;
            for (int i = 0; i < fullStacks; i++)
                ClientManager.Instance.CtrlClickPosition(currencySlot);
            return true;
        }

        private bool ApiMakeTrade(CustomerInfo customer)
        {
            while (customer.TradeStatus != CustomerInfo.TradeStatuses.ACCEPTED && customer.IsInArea)
            {
                //start trade
                if (OpenTradeWindow(customer) &&
                    ClientManager.Instance.DumpInventory() &&
                    ApiAcceptTrade(customer))
                    return true;
                Thread.Sleep(2000);
            }
            return false;
        }

        private bool ApiAcceptTrade(CustomerInfo customer)
        {
            while (ClientManager.Instance.GetAbsoluteAssetPosition(StaticUtils.GetUIFragmentPath("trade_window_title")).IsVisible)
            {
                customer.TradeStatus = CustomerInfo.TradeStatuses.STARTED;
                foreach (Position offerSlot in TradePositions.GetPositions(ClientManager.Instance.ResolutionEnum))
                {
                    if (customer.TradeStatus != CustomerInfo.TradeStatuses.STARTED)
                        return false;
                    ClientManager.Instance.HoverPosition(ClientManager.Instance.TranslatePosition(offerSlot));
                }
                Position button = ClientManager.Instance.GetAbsoluteAssetPosition(StaticUtils.GetUIFragmentPath("accept_tradewindow"));
                if (button.IsVisible)
                    ClientManager.Instance.ClickPosition(button);
                Thread.Sleep(500);
            }

            return true;
        }

        private bool ProcessCurrencySale(CustomerInfo customer)
        {
            if (!ClientManager.Instance.OpenStash())
                return NotifyItemSold(customer); // We have an issue opening the Stash

            if (!TakeCurrency(customer))
                return NotifyItemSold(customer);

            // Trade Ready let's send the Invite
            if (!BotEngineUtils.InviteCustomer(customer))
                return false;

            return MakeTrade(customer);
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

            return MakeTrade(customer);

        }

        private bool MakeTrade(CustomerInfo customer)
        {
            while (customer.TradeStatus != CustomerInfo.TradeStatuses.ACCEPTED && customer.IsInArea)
            {
                //start trade
                if (OpenTradeWindow(customer) &&
                    OfferProduct(customer) &&
                    CheckCurrency(customer) &&
                    AcceptTrade(customer))
                    return true;
                Thread.Sleep(2000);
            }
            return false;
        }

        /// <summary>
        /// Notifies the item sold.
        /// This is also a default message to send if there is an issue with any trade action
        /// </summary>
        /// <param name="customer">The customer.</param>
        private bool NotifyItemSold(CustomerInfo customer)
        {
            Logger.Console.Fatal("There was an issue with sale, please check log.");
            ClientManager.Instance.ChatCommand($"@{customer.Nickname} Sorry, sold it already.");
            return false; // always return false
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
                itemInfo.Item.Price.CurrencyType.Name != customer.CurrencyType.Name) // And in the right currency?
                return false;

            // Everything is good at this point. Move the item to Inventory
            return ClientManager.Instance.GetItemFromStash(itemPosition, customer.StashTabNormal);
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

                        Position acceptTradeRequestButton = ClientManager.Instance.GetAbsoluteAssetPosition(StaticUtils.GetUIFragmentPath("accespt"));
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

                            if (ClientManager.Instance.GetAbsoluteAssetPosition(StaticUtils.GetUIFragmentPath("trade_waiting")).IsVisible)
                                amIdoRequest = true;
                            else
                            {
                                Logger.Console.Debug("Check trade window");
                                if (ClientManager.Instance.GetAbsoluteAssetPosition(StaticUtils.GetUIFragmentPath("trade_window_title")).IsVisible)
                                    return true;
                            }
                        }
                    }
                    else
                    {
                        Logger.Console.Debug("Check trade window");
                        if (ClientManager.Instance.GetAbsoluteAssetPosition(StaticUtils.GetUIFragmentPath("trade_window_title")).IsVisible)
                            return true;
                    }
                    Thread.Sleep(500);
                }

            }
            return false;
        }

        private bool OfferProduct(CustomerInfo customer)
        {
            if (customer.OrderType == CustomerInfo.OrderTypes.ITEM)
                return OfferSingleProduct(customer);
            else if (customer.OrderType == CustomerInfo.OrderTypes.CURRENCY)
                return OfferCurrency(customer);
            return false;
        }

        private bool OfferSingleProduct(CustomerInfo customer)
        {
            foreach (Position invData in InventoryPositions.GetInvenoryPositions(ClientManager.Instance.ResolutionEnum))
            {
                ItemInfoParser itemInfo = ClientManager.Instance.GetItemInfo(invData);
                if (!customer.Product.Contains(itemInfo.Item.RealName)) continue;

                ClientManager.Instance.CtrlClickPosition(ClientManager.Instance.TranslatePosition(invData));
                return true;
            }
            return false;
        }

        private bool OfferCurrency(CustomerInfo customer)
        {
            int amountOffered = 0;
            foreach (Position invData in InventoryPositions.GetInvenoryPositions(ClientManager.Instance.ResolutionEnum))
            {
                ItemInfoParser itemInfo = ClientManager.Instance.GetItemInfo(invData);
                if (!customer.Product.Contains(itemInfo.Item.RealName)) continue;

                ClientManager.Instance.CtrlClickPosition(ClientManager.Instance.TranslatePosition(invData));
                amountOffered += itemInfo.Item.SizeInStack;
                if (amountOffered >= customer.NumberProducts)
                    return true;
            }
            return false;
        }

        private bool CheckCurrency(CustomerInfo customer, double margin = 0.1)
        {
            // Give the customer a little time to move currency to trade window.
            Thread.Sleep(5000);

            double chaosValueInTrade = 0.0;
            double chaosValueExpected = customer.CurrencyType.ChaosEquivalent * customer.Cost;
            int amountInTrade = 0;
            // Count Currency in trade window
            while (ClientManager.Instance.GetAbsoluteAssetPosition(StaticUtils.GetUIFragmentPath("trade_window_title")).IsVisible)
            {
                customer.TradeStatus = CustomerInfo.TradeStatuses.STARTED;
                foreach (Position offerSlot in TradePositions.GetPositions(ClientManager.Instance.ResolutionEnum))
                {
                    if (customer.TradeStatus != CustomerInfo.TradeStatuses.STARTED)
                        break;

                    ItemInfoParser itemInfo = ClientManager.Instance.GetItemInfo(offerSlot);
                    if (itemInfo.Item.ChaosValue != 0)
                        chaosValueInTrade += itemInfo.Item.ChaosValue;
                    if (itemInfo.Item.SizeInStack != 0)
                        amountInTrade += itemInfo.Item.SizeInStack;
                    if (chaosValueInTrade >= chaosValueExpected - (chaosValueExpected * margin) ||
                        amountInTrade >= customer.Cost)
                        break; // No need to keep checking
                }
                if (chaosValueInTrade >= chaosValueExpected - (chaosValueExpected * margin) ||
                    amountInTrade >= customer.Cost)
                    break; // we are ready to accept the trade
            }

            return true;
        }

        private bool AcceptTrade(CustomerInfo customer)
        {
            Position button = ClientManager.Instance.GetAbsoluteAssetPosition(StaticUtils.GetUIFragmentPath("accept_tradewindow"));
            if (!button.IsVisible)
                return false;
            ClientManager.Instance.ClickPosition(button);

            while (customer.TradeStatus != CustomerInfo.TradeStatuses.ACCEPTED)
            {
                if (customer.TradeStatus == CustomerInfo.TradeStatuses.CANCELED)
                    return false;

                Thread.Sleep(500);
            }
            return true;
        }

        private bool TakeCurrency(CustomerInfo customer)
        {
            ClientManager.Instance.ActivateTab("currency_tab");

            // Get Currency Position
            int clientResolution = Convert.ToInt32(ClientManager.Instance.ResolutionEnum.GetDescription());
            string requestedNormal = customer.Product.Replace(" ", "").Replace("'", "");
            Position currencySlot = ClientManager.Instance.GetAbsoluteAssetPosition(StaticUtils.GetCurrencyPath(requestedNormal));
            ItemInfoParser itemInfo = ClientManager.Instance.GetItemInfo(currencySlot, false);

            // Do I have enough to fulfill the order?
            if (itemInfo.Item.SizeInStack < customer.NumberProducts)
                return false;

            // Ok, we have enough so lets move it to Inventory
            // First we need to figure out the off stack and full stacks
            int offStack = customer.NumberProducts % itemInfo.Item.StackSize;
            int fullStacks = customer.NumberProducts / itemInfo.Item.StackSize;

            // Now get the off stack to Invendory slot 1
            ClientManager.Instance.GetPartialStackToCusor(currencySlot, offStack);
            Position invSlot1 = ClientManager.Instance.TranslatePosition(InventoryPositions.GetInvenoryPositions(ClientManager.Instance.ResolutionEnum)[0]);
            ClientManager.Instance.ClickPosition(invSlot1);
            for (int i = 0; i < fullStacks; i++)
                ClientManager.Instance.CtrlClickPosition(currencySlot);

            // Now we should have the correct items in the Inventory
            return true;
        }
    }
}
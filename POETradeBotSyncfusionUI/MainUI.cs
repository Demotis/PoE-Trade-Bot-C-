using System;
using System.ComponentModel;
using TradeBotSharedLib.Models;
using TradeBotSharedLib.PoEClient;

namespace POETradeBotSyncfusionUI
{
    public partial class MainUI : Syncfusion.Windows.Forms.Office2007Form
    {
        public CustomerInfo CurCustomer { get; private set; }

        public MainUI()
        {
            InitializeComponent();
        }

        private void MainUI_Load(object sender, EventArgs e)
        {
            CustomerQueueGrid.DataSource = TradeBotSharedLib.Statics.CustomerQueue;
            TradeBotSharedLib.Statics.CustomerQueue.ListChanged += CustomerListChanged;

            if (ClientManager.Instance.ValidateProcess())
                ClientStatusDisplay.Text = "Path of Exile Running";
            else
                ClientStatusDisplay.Text = "Path of Exile not started";
        }

        private void CustomerListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.NewIndex != 0) return;
            CurCustomer = TradeBotSharedLib.Statics.CustomerQueue[0];
            CurrentCustomerInfo.Invoke(new Action(() => { CurrentCustomerInfo.Text = CurCustomer.ToString(); }));
        }

        private void CustomerQueueGrid_TableControlTopRowChanged(object sender, Syncfusion.Windows.Forms.Grid.Grouping.GridTableControlRowColIndexChangedEventArgs e)
        {
        }

        private void CustomerQueueGrid_SelectedRecordsChanged(object sender, Syncfusion.Grouping.SelectedRecordsChangedEventArgs e)
        {
        }

        private void InviteButton_Click(object sender, EventArgs e)
        {
            ClientManager.Instance.ChatCommand($"/invite {CurCustomer.Nickname}");
        }

        private void OpenStashButton_Click(object sender, EventArgs e)
        {
            ClientManager.Instance.OpenStash();
        }
    }
}

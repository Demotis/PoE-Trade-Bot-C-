using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using TradeBotSharedLib.Models;
using TradeBotSharedLib.PoEClient;
using TradeBotSharedLib.Utilities;

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

            LoadClientConfig();
        }

        private void LoadClientConfig()
        {
            if (!ClientManager.Instance.ValidateProcess())
                return;

            ClientStatusDisplay.Text = "Path of Exile Running";
            CurrentClientResolutionDisplay.Text = ClientManager.Instance.ActiveResolutionNormal;
            if (ClientManager.Instance.ActiveConfiguration == null)
                CreateBlankConfig();
            ActiveConfigStatus.Text = "Config Loaded";

            if (ClientManager.Instance.ActiveConfiguration.StashTag != null)
                StashTagButton.Text = "Ready";

            if (ClientManager.Instance.ActiveConfiguration.StashTitle != null)
                StashTitleButton.Text = "Ready";

            if (ClientManager.Instance.ActiveConfiguration.Inventory.UpperLeft.IsVisible &&
                ClientManager.Instance.ActiveConfiguration.Inventory.LowerRight.IsVisible)
                InventoryConfigButton.Text = "Ready";

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

        private void CreateBlankConfig()
        {
            ClientConfiguration.WriteToJsonFile<POEUIConfig>(
                ClientManager.Instance.ActiveResolutionNormal,
                new POEUIConfig()
                );
            ClientManager.Instance.SetCurrentPosition();
        }

        private void SaveConfig_Click(object sender, EventArgs e)
        {
            if (ClientManager.Instance.ActiveConfiguration == null)
                CreateBlankConfig();

            ClientConfiguration.WriteToJsonFile<POEUIConfig>(
                ClientManager.Instance.ActiveResolutionNormal,
                ClientManager.Instance.ActiveConfiguration
                );

            ClientManager.Instance.SetCurrentPosition();
            LoadClientConfig();
        }

        private void StashTitleButton_Click(object sender, EventArgs e)
        {
            SaveConfig_Click(null, null);
            var asset = GetScreenPart();
            ClientManager.Instance.ActiveConfiguration.StashTitle = asset;
            SaveConfig_Click(null, null);
        }

        private void StashTagButton_Click(object sender, EventArgs e)
        {
            SaveConfig_Click(null, null);
            var asset = GetScreenPart();
            ClientManager.Instance.ActiveConfiguration.StashTag = asset;
            SaveConfig_Click(null, null);
        }

        private Bitmap GetScreenPart()
        {
            GetImage form = new GetImage();
            form.ShowDialog();
            return form.Asset.Clone() as Bitmap;
        }

        private Position _upperLeft { get; set; }
        private Position _lowerRight { get; set; }

        private void InventoryConfigButton_Click(object sender, EventArgs e)
        {
            DialogResult dResult = MessageBox.Show("Is the POE Inventory Window open?", "Open Inventory", MessageBoxButtons.YesNoCancel);
            if (dResult == DialogResult.Cancel)
                return;
            if (dResult == DialogResult.No)
                ClientManager.Instance.SendKey("i");

            SetUpGridCorners();
            ClientManager.Instance.ActiveConfiguration.Inventory.UpperLeft =
                ClientManager.Instance.GetRelativePosition(_upperLeft.Clone());
            ClientManager.Instance.ActiveConfiguration.Inventory.LowerRight =
                ClientManager.Instance.GetRelativePosition(_lowerRight.Clone());
        }

        private void SetUpGridCorners()
        {
            ClientManager.Instance.BringToForeground();
            _upperLeft = new Position();
            _lowerRight = new Position();

            KeyboardHookManager.HookManager.MouseDown += mouseHook_MouseDown;

            InstructionText.Text = "Click the Center of the Upper Left Box";
            while (!_upperLeft.IsVisible || !_lowerRight.IsVisible)
            {
                if (_upperLeft.IsVisible)
                    InstructionText.Text = "Click the Center of the Lower Right Box";
                Thread.Sleep(100);
                Application.DoEvents();
            }
            KeyboardHookManager.HookManager.MouseDown -= mouseHook_MouseDown;
            InstructionText.Text = string.Empty;
        }

        private void mouseHook_MouseDown(object sender, MouseEventArgs e)
        {
            if (!_upperLeft.IsVisible)
                _upperLeft.MousePoint = e.Location;
            else
                _lowerRight.MousePoint = e.Location;
        }

        private void DumpInventoryButton_Click(object sender, EventArgs e)
        {
            if (ClientManager.Instance.ActiveConfiguration.Inventory == null)
            {
                MessageBox.Show("You must setup the Inventory Configuration");
                return;
            }
            ClientManager.Instance.ActiveConfiguration.Inventory.DumpInventory();
        }
    }
}

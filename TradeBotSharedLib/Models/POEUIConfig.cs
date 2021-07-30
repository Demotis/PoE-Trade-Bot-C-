using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using TradeBotSharedLib.Utilities;

namespace TradeBotSharedLib.Models
{
    public class POEUIConfig
    {
        public string Resolution { get; set; }

        [JsonIgnore]
        public Bitmap StashTag { get; set; }
        public string StashTagBase64
        {
            get { return StashTag.ToBase64String(ImageFormat.Png); }
            set { StashTag = value.Base64StringToBitmap(); }
        }

        [JsonIgnore]
        public Bitmap StashTitle { get; set; }
        public string StashTitleBase64
        {
            get { return StashTitle.ToBase64String(ImageFormat.Png); }
            set { StashTitle = value.Base64StringToBitmap(); }
        }

        public InventoryConfig Inventory { get; set; }

        public List<TabConfig> StashTabs { get; set; }

        public POEUIConfig()
        {
            Inventory = new InventoryConfig();
        }
    }
}

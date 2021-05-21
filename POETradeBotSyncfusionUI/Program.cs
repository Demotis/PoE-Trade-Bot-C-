using System;
using System.Windows.Forms;
using TradeBotSharedLib.PoEClient;
using TradeBotSharedLib.Utilities;

namespace POETradeBotSyncfusionUI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NDQ4MDI4QDMxMzkyZTMxMmUzMGJXSXYwWjZjMHduV1VJVzh3VFo2ODNiM0FTdVJQY3o1a3JocDZNeVJVMU09");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Start Log Listener
            PoECurrencyManager.Instance.StartService();
            LogManager.Instance.StartService();

            Application.Run(new MainUI());
        }
    }
}

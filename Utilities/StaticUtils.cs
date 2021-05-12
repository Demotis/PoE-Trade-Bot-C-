using PoETradeBot.Enums;

namespace PoETradeBot.Utilities
{
    public static class StaticUtils
    {
        public static string GetUIFragmentPath(string asset)
        {
            string returnPath = $"Assets/UI_Fragments/{ConfigManager.Instance.ApplicationConfig["POEResolution"]}/{asset}.png";
            return returnPath;
        }

        public static string GetCurrencyPath(Currency currency)
        {
            string returnPath = $"Assets/Currencies/{ConfigManager.Instance.ApplicationConfig["POEResolution"]}/{currency.GetDescription()}.png";
            return returnPath;
        }

        public static string GetCurrencyPath(string currency)
        {
            string returnPath = $"Assets/Currencies/{ConfigManager.Instance.ApplicationConfig["POEResolution"]}/{currency.ToLower()}.png";
            return returnPath;
        }
    }
}

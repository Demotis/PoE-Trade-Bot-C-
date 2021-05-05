using PoE_Trade_Bot.Enums;

namespace PoE_Trade_Bot.Utilities
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
    }
}

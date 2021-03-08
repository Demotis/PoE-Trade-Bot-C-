namespace PoE_Trade_Bot.PoEBotV2.Models
{
    public class Currency
    {
        private Currency()
        {
        }

        public ItemType ItemType { get; set; }

        public double ChaosEquivalent { get; set; }

        public string Image { get; set; }

        public static Currency Create(double chaosEquivalent, ItemType itemType, string image)
        {
            return new()
            {
                ChaosEquivalent = chaosEquivalent,
                ItemType = itemType,
                Image = image
            };
        }
    }
}
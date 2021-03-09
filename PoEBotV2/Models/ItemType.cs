namespace PoE_Trade_Bot.PoEBotV2.Models
{
    public class ItemType
    {
        private ItemType()
        {
        }

        public string Name { get; set; }

        public bool Stacked { get; set; }

        public static ItemType Create(string name, bool stacked)
        {
            return new()
            {
                Name = name,
                Stacked = stacked
            };
        }
    }
}
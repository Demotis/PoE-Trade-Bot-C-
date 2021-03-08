namespace PoE_Trade_Bot.PoEBotV2.Models
{
    public class Price
    {
        private Price()
        {
        }

        public double Value { get; set; }

        public Currency Currency { get; set; }

        public int For { get; set; }

        public static Price Create(double value, Currency currency, int forCount)
        {
            var price = Create(value, currency);

            price.For = forCount;

            return price;
        }


        public static Price Create(double value, Currency currency)
        {
            return new()
            {
                Currency = currency,
                For = 1,
                Value = value
            };
        }
    }
}
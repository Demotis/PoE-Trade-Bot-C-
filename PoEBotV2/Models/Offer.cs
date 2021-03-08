using System;

namespace PoE_Trade_Bot.PoEBotV2.Models
{
    public class Offer
    {
        private Offer()
        {
        }

        public OfferStatus Status { get; set; }
        
        public DateTime CreatedAt { get; set; }

        
        public DateTime StartedAt { get; set; }

        
        public DateTime ClosedAt { get; set; }

        public Product Product { get; set; }

        public int Count { get; set; }

        public Price OfferPrice { get; set; }

        public string Username { get; set; }

        /**
         * Create offer for multiple products
         */
        public static Offer Create(string username, Price offerPrice, Product product, int count)
        {
            var offer = Create(username, offerPrice, product);

            offer.Count = count;

            return offer;
        }

        /**
         * Create offer for one product
         */
        public static Offer Create(string username, Price offerPrice, Product product)
        {
            return new()
            {
                OfferPrice = offerPrice,
                Product = product,
                Count = 1,
                Status = OfferStatus.NEW,
                CreatedAt = DateTime.Now,
                Username = username,
            };
        }
    }
}
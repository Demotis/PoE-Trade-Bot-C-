using System;

namespace PoE_Trade_Bot.PoEBotV2.Models
{
    public class Offer
    {
        public OfferStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime ClosedAt { get; set; }

        public Product Product { get; set; }

        public int Count { get; set; }

        public Price OfferPrice { get; set; }
    }
}
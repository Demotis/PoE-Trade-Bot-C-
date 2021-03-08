using System.Collections.Generic;
using PoE_Trade_Bot.PoEBotV2.Models;

namespace PoE_Trade_Bot.PoEBotV2.Storage
{
    public static class ProductsStorage
    {
        private static readonly List<Product> Products = new List<Product>();

        public static Product FindProductByTypeName(string typeName)
        {
            return Products.Find(product => product.Type.Name == typeName);
        }
    }
}
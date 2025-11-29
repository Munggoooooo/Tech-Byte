using Tech_Byte.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Tech_Byte.Services
{
    public class CartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string SessionKey = "ShoppingCart";

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public List<CartItem> GetCart()
        {
            var session = _httpContextAccessor.HttpContext!.Session;
            var cartJson = session.GetString(SessionKey);
            return string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonConvert.DeserializeObject<List<CartItem>>(cartJson)!;
        }

        public void SaveCart(List<CartItem> cart)
        {
            var session = _httpContextAccessor.HttpContext!.Session;
            var cartJson = JsonConvert.SerializeObject(cart);
            session.SetString(SessionKey, cartJson);
        }

        public void AddToCart(InventoryItem item, int quantity)
        {
            var cart = GetCart();
            var existing = cart.FirstOrDefault(c => c.ItemId == item.Id);

            if (existing != null)
            {
                // Respect MaxQuantity
                existing.Quantity = Math.Min(existing.Quantity + quantity, item.Quantity);
                existing.MaxQuantity = item.Quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ItemId = item.Id!,
                    Name = item.Name,
                    Image = item.Image ?? "",
                    Price = (decimal)item.Price,
                    Quantity = quantity,
                    MaxQuantity = item.Quantity  // optional
                });
            }

            SaveCart(cart);
        }


        public void RemoveFromCart(string itemId)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.ItemId == itemId);
            SaveCart(cart);
        }

        public void ClearCart()
        {
            SaveCart(new List<CartItem>());
        }

        public int GetCartItemCount()
        {
            var cart = GetCart();
            return cart.Sum(c => c.Quantity);
        }

    }
}

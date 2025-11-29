using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Tech_Byte.Models;
using Tech_Byte.Services;

namespace Tech_Byte.Pages
{
    [Authorize(Roles = "Admin,Member")]
    public class CartModel : PageModel
    {
        private readonly CartService _cartService;

        public List<CartItem> CartItems { get; set; } = new();
        public decimal Subtotal => CartItems.Sum(c => c.Total);

        public CartModel(CartService cartService)
        {
            _cartService = cartService;
        }

        public void OnGet()
        {
            CartItems = _cartService.GetCart();
        }

        public IActionResult OnPostRemove(string id)
        {
            _cartService.RemoveFromCart(id);
            TempData["Message"] = "Item removed from cart.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostPlaceOrder([FromServices] MongoDBService db,
                                                  [FromServices] EmailService emailService)
        {
            var cartItems = _cartService.GetCart();

            if (!cartItems.Any())
            {
                TempData["Message"] = "Your cart is empty.";
                return RedirectToPage();
            }

            // Get current user info
            var username = User.Identity!.Name; // currently logged in user
            var user = await db.Users.Find(u => u.Username == username).FirstOrDefaultAsync();
            if (user == null)
            {
                TempData["Message"] = "User not found.";
                return RedirectToPage();
            }

            var purchase = new Purchase
            {
                CustomerUserName = user.Username,
                CustomerEmail = user.Email,
                Items = cartItems,
                Total = cartItems.Sum(i => i.Total),
                Date = DateTime.UtcNow,
                OrderId = await db.GenerateOrderIdAsync()
            };

            var success = await db.FinalizePurchaseAsync(purchase, user.Username);
            if (!success)
            {
                TempData["Message"] = "Failed to place order. One or more items may be out of stock.";
                return RedirectToPage();
            }

            _cartService.ClearCart();
            TempData["Message"] = "Order placed successfully!";

            // Send email receipt
            await emailService.SendPurchaseReceiptAsync(user.Email, purchase);

            return RedirectToPage();
        }


        public IActionResult OnPostAdd(string id, int quantity, int maxquantity, [FromServices] MongoDBService db)
        {
            // Find the product from MongoDB
            var item = db.GetByIdAsync(id).Result;
            if (item == null)
                return RedirectToPage("/Product"); // item not found, go back to products

            // Add the item to the cart
            _cartService.AddToCart(item, quantity);

            // Optionally show a success message
            TempData["Message"] = $"{item.Name} added to your cart.";

            return RedirectToPage("/Cart");
        }

        public IActionResult OnPostIncrease(string id)
        {
            var cart = _cartService.GetCart();
            var item = cart.FirstOrDefault(c => c.ItemId == id);
            if (item != null && item.Quantity < item.MaxQuantity)
            {
                item.Quantity++;
            }
            _cartService.SaveCart(cart);
            return RedirectToPage();
        }

        public IActionResult OnPostDecrease(string id)
        {
            var cart = _cartService.GetCart();
            var item = cart.FirstOrDefault(c => c.ItemId == id);
            if (item != null && item.Quantity > 1)
            {
                item.Quantity--;
            }
            _cartService.SaveCart(cart);
            return RedirectToPage();
        }


    }
}

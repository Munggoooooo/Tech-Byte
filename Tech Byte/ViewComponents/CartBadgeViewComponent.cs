using Tech_Byte.Services;
using Microsoft.AspNetCore.Mvc;

public class CartBadgeViewComponent : ViewComponent
{
    private readonly CartService _cartService;

    public CartBadgeViewComponent(CartService cartService)
    {
        _cartService = cartService;
    }

    public IViewComponentResult Invoke()
    {
        var count = _cartService.GetCartItemCount();
        return View(count);
    }
}

using Microsoft.AspNetCore.Mvc;
using Tech_Byte.Services;
using System.Threading.Tasks;

namespace Tech_Byte.ViewComponents.Metrics
{
    public class TotalOrdersViewComponent : ViewComponent
    {
        private readonly MongoDBService _db;

        public TotalOrdersViewComponent(MongoDBService db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Get all purchases/orders
            var purchases = await _db.GetAllPurchasesAsync(); // Make sure this method exists in your MongoDBService

            // Count total orders
            var totalOrders = purchases.Count;

            return View(totalOrders);
        }
    }
}

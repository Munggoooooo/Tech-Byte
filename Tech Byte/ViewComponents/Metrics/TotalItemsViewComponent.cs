using Microsoft.AspNetCore.Mvc;
using Tech_Byte.Services;
using System.Threading.Tasks;

namespace Tech_Byte.ViewComponents.Metrics
{
    public class TotalItemsViewComponent : ViewComponent
    {
        private readonly MongoDBService _db;

        public TotalItemsViewComponent(MongoDBService db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Get all items from inventory
            var items = await _db.GetAllAsync();

            // Count total items
            var totalItems = items.Count;

            return View(totalItems);
        }
    }
}

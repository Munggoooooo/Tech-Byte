using Microsoft.AspNetCore.Mvc.RazorPages;
using Tech_Byte.Models;
using Tech_Byte.Services;
using Newtonsoft.Json;

namespace Tech_Byte.Pages
{
    public class HomeModel : PageModel
    {
        private readonly MongoDBService _db;

        public List<InventoryItem> RecentProducts { get; set; }

        public List<InventoryItem> MostPopularItems { get; set; }

        public HomeModel(MongoDBService db)
        {
            _db = db;
        }

        public async Task OnGetAsync()
        {
            RecentProducts = await _db.GetRecentlyAddedAsync(4); // Get the latest number of items
            MostPopularItems = await _db.GetMostPopularItemsAsync(4);
        }
    }
}

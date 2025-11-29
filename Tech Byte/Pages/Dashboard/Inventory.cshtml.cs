using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tech_Byte.Models;
using Tech_Byte.Services;

namespace Tech_Byte.Pages.Dashboard
{
    [Authorize(Roles = "Admin")]
    public class InventoryModel : PageModel
    {
        private readonly MongoDBService _db;

        public List<InventoryItem> Items { get; set; }
        public string SearchTerm { get; set; }

        public InventoryModel(MongoDBService db)
        {
            _db = db;
        }

        public async Task OnGetAsync(string search)
        {
            SearchTerm = search;

            if (string.IsNullOrEmpty(search))
            {
                // If no search term is provided, fetch all items
                Items = await _db.GetAllAsync();
            }
            else
            {
                // If a search term is provided, fetch filtered items
                Items = await _db.SearchItemsAsync(search);
            }
        }
    }
}

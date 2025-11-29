using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tech_Byte.Models;
using Tech_Byte.Services;

namespace Tech_Byte.Pages
{
    [Authorize(Roles = "Admin,Member")]
    public class ProductModel : PageModel
    {
        private readonly MongoDBService _db;

        public List<InventoryItem> Items { get; set; }
        public string? SearchTerm { get; set; }

        public ProductModel(MongoDBService db)
        {
            _db = db;
        }

        public async Task OnGetAsync(string search, string category)
        {
            SearchTerm = search;

            if (!string.IsNullOrEmpty(category))
            {
                // Filter by category
                Items = await _db.SearchItemsByCategoryAsync(category);
            }
            else if (!string.IsNullOrEmpty(search))
            {
                // Filter by search term
                Items = await _db.SearchItemsAsync(search);
            }
            else
            {
                // No filter, get all
                Items = await _db.GetAllAsync();
            }
        }

    }
}

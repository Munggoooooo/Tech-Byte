using Tech_Byte.Models;
using Tech_Byte.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace Tech_Byte.Pages.Dashboard
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly MongoDBService _db;

        [BindProperty]
        public InventoryItem Item { get; set; }

        public DeleteModel(MongoDBService db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToPage("/Dashboard/Inventory");

            Item = await _db.GetByIdAsync(id);
            if (Item == null) return RedirectToPage("/Dashboard/Inventory");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Item.Id)) return RedirectToPage("/Dashboard/Inventory");

            var performedBy = User.Identity!.Name; // logged-in admin

            await _db.DeleteAsync(Item.Id, performedBy);
            return RedirectToPage("/Dashboard/Inventory");
        }
    }
}

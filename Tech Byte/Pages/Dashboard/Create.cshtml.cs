using Tech_Byte.Models;
using Tech_Byte.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace Tech_Byte.Pages.Dashboard
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly MongoDBService _db;

        [BindProperty]
        public InventoryItem Item { get; set; }

        public CreateModel(MongoDBService db)
        {
            _db = db;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page(); 
            }

            var performedBy = User.Identity!.Name; // logged-in admin

            await _db.CreateAsync(Item, performedBy);
            return RedirectToPage("/Dashboard/Inventory");
        }
    }
}

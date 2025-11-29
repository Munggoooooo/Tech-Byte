using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tech_Byte.Models;
using Tech_Byte.Services;

namespace Tech_Byte.Pages.Dashboard
{
    [Authorize(Roles = "Admin,Member")]
    public class PurchaseHistoryModel : PageModel
    {
        private readonly MongoDBService _db;

        public List<Purchase> Purchases { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public PurchaseHistoryModel(MongoDBService db)
        {
            _db = db;
        }

        public async Task OnGetAsync()
        {
            Purchases = await _db.SearchPurchasesAsync(SearchTerm);
        }
    }
}

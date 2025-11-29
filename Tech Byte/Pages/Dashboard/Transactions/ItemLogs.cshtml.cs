using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tech_Byte.Models;
using Tech_Byte.Services;

namespace Tech_Byte.Pages.Dashboard.Transactions
{
    [Authorize(Roles = "Admin")]
    public class ItemLogsModel : PageModel
    {
        private readonly MongoDBService _db;

        public List<Transaction> Transactions { get; set; }

        public List<Purchase> Purchases { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public ItemLogsModel(MongoDBService db)
        {
            _db = db;
        }

        public async Task OnGetAsync()
        {
            Purchases = await _db.GetPurchasesAsync();
            Transactions = await _db.SearchTransactionsAsync(SearchTerm);
        }
    }
}

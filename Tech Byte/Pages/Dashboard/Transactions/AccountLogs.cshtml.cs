using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tech_Byte.Services;
using Tech_Byte.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Tech_Byte.Utilities;

namespace Tech_Byte.Pages.Dashboard.Transactions
{
    [Authorize(Roles = "Admin")]
    public class AccountLogsModel : PageModel
    {
        private readonly MongoDBService _db;

        public AccountLogsModel(MongoDBService db)
        {
            _db = db;
        }

        public string SearchTerm { get; set; }

        public List<AccountLog> Logs { get; set; }

        public async Task OnGet(string SearchTerm)
        {
            this.SearchTerm = SearchTerm;

            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                Logs = await _db.GetAccountLogsAsync();
                return;
            }

            // Case-insensitive regex
            var regex = new BsonRegularExpression(SearchTerm, "i");

            var filter = Builders<AccountLog>.Filter.Or(
                Builders<AccountLog>.Filter.Regex(l => l.Action, regex),
                Builders<AccountLog>.Filter.Regex(l => l.PerformedBy, regex),
                Builders<AccountLog>.Filter.Regex(l => l.AccountUsername, regex),
                Builders<AccountLog>.Filter.Regex(l => l.Details, regex)
            );

            Logs = await _db.AccountLogs.Find(filter)
                                        .SortByDescending(l => l.Timestamp)
                                        .ToListAsync();
        }
    }
}

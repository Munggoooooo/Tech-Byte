using Microsoft.AspNetCore.Mvc;
using Tech_Byte.Services;

namespace Tech_Byte.ViewComponents.Metrics
{
    public class TotalUsersViewComponent : ViewComponent
    {
        private readonly MongoDBService _db;

        public TotalUsersViewComponent(MongoDBService db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var totalUsers = await _db.GetTotalUsersAsync();
            return View(totalUsers);
        }
    }
}

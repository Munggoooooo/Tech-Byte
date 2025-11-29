using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Tech_Byte.Models;
using Tech_Byte.Services;

namespace Tech_Byte.ViewComponents
{
    public class NotificationBadgeViewComponent : ViewComponent
    {
        private readonly MongoDBService _db;

        public NotificationBadgeViewComponent(MongoDBService db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var contacts = await _db.GetAllContactsAsync();
            var unreadCount = contacts.Count(c => !c.IsRead);
            return View(unreadCount);
        }
    }
}

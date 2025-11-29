using Microsoft.AspNetCore.Mvc;
using Tech_Byte.Services; // Your MongoDBService namespace
using System.Threading.Tasks;

namespace Tech_Byte.ViewComponents.Metrics
{
    public class TotalSalesViewComponent : ViewComponent
    {
        private readonly MongoDBService _db;

        public TotalSalesViewComponent(MongoDBService db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Get all monthly sales
            var monthlySales = await _db.GetMonthlySalesAsync();

            // Sum total sales
            var totalSales = monthlySales.Sum(s => s.TotalSales);

            return View(totalSales);
        }
    }
}

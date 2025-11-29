using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tech_Byte.Models;
using Tech_Byte.Services;

namespace Tech_Byte.Pages.Dashboard
{
    [Authorize(Roles = "Admin")]
    public class SalesModel : PageModel
    {
        private readonly MongoDBService _db;

        public List<MonthlySalesDto> MonthlySales { get; set; }

        public List<MonthlySalesCategoryDTO> MonthlyCategoryItems { get; set; }

        public SalesModel(MongoDBService db)
        {
            _db = db;
        }
        public async Task OnGetAsync()
        {
            MonthlySales = await _db.GetMonthlySalesAsync();

            MonthlyCategoryItems = await _db.GetItemsSoldPerCategoryPerMonthAsync();

        }
    }
}

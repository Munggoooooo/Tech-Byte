using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Tech_Byte.Models;
using Tech_Byte.Services;

namespace Tech_Byte.Pages.Account
{
    [Authorize(Roles = "Admin,Member")] // Any logged-in user can view this page
    public class ProfileModel : PageModel
    {
        private readonly MongoDBService _mongoService;

        public User CurrentUser { get; set; }

        public ProfileModel(MongoDBService mongoService)
        {
            _mongoService = mongoService;
        }

        public async Task OnGet()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (!string.IsNullOrEmpty(email))
            {
                CurrentUser = await _mongoService.GetUserByEmailAsync(email);
            }
        }
    }
}

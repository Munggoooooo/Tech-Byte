using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Tech_Byte.Models;
using Tech_Byte.Services;

namespace Tech_Byte.Pages.Account
{
    public class ActivateModel : PageModel
    {
        private readonly MongoDBService _db;

        public ActivateModel(MongoDBService db)
        {
            _db = db;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Code { get; set; }

        public void OnGet(string email)
        {
            Email = email;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _db.Users.Find(x => x.Email == Email).FirstOrDefaultAsync();

            if (user == null || user.ActivationCode != Code)
            {
                ModelState.AddModelError("", "Invalid activation code.");
                return Page();
            }

            var update = Builders<User>.Update
                .Set(u => u.IsActivated, true)
                .Set(u => u.ActivationCode, null);

            await _db.Users.UpdateOneAsync(u => u.Id == user.Id, update);

            TempData["ActivationSuccess"] = "Your account has been activated! You can now log in.";
            return RedirectToPage("/Account/Login");
        }
    }
}

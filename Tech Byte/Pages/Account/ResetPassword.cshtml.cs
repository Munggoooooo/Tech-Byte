using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tech_Byte.Services;
using MongoDB.Driver;

namespace Tech_Byte.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly MongoDBService _db;

        public ResetPasswordModel(MongoDBService db)
        {
            _db = db;
        }

        [BindProperty(SupportsGet = true)]
        public string Token { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(Token))
                return RedirectToPage("/Login");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return Page();
            }

            var user = await _db.Users.Find(u => u.PasswordResetToken == Token).FirstOrDefaultAsync();

            if (user == null || user.PasswordResetExpires < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Invalid or expired password reset link.");
                return Page();
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetExpires = null;

            await _db.Users.ReplaceOneAsync(u => u.Id == user.Id, user);

            TempData["ResetPasswordSuccess"] = "Password changed successfully! You can now log in.";
            return RedirectToPage("/Account/Login");
        }
    }
}

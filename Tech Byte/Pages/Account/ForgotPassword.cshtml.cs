using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tech_Byte.Services;
using MongoDB.Driver;
using System.Security.Cryptography;

namespace Tech_Byte.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly MongoDBService _db;
        private readonly EmailService _email;

        public ForgotPasswordModel(MongoDBService db, EmailService email)
        {
            _db = db;
            _email = email;
        }

        [BindProperty]
        public string Email { get; set; }

        public string Message { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _db.Users.Find(u => u.Email == Email).FirstOrDefaultAsync();

            if (user == null)
            {
                ModelState.AddModelError("", "Email not found.");
                return Page();
            }

            // Generate secure token
            string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
                .Replace("/", "_")
                .Replace("+", "-");

            user.PasswordResetToken = token;
            user.PasswordResetExpires = DateTime.UtcNow.AddMinutes(30);

            await _db.Users.ReplaceOneAsync(u => u.Id == user.Id, user);

            string resetLink = $"{Request.Scheme}://{Request.Host}/ResetPassword?token={token}";

            await _email.SendPasswordResetAsync(user.Email, resetLink);

            Message = "Password reset link has been sent to your email.";
            return Page();
        }
    }
}

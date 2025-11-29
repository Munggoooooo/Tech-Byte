using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tech_Byte.Models;
using Tech_Byte.Services;
using MongoDB.Driver;

namespace Tech_Byte.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly MongoDBService _db;
        private readonly EmailService _email;

        public RegisterModel(MongoDBService db, EmailService email)
        {
            _db = db;
            _email = email;
        }

        [BindProperty]
        public User Input { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check existing email
            var existing = await _db.Users.Find(u => u.Email == Input.Email).FirstOrDefaultAsync();
            if (existing != null)
            {
                ModelState.AddModelError("", "Email is already registered.");
                return Page();
            }

            // Generate activation code
            var code = new Random().Next(100000, 999999).ToString();

            // Hash password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Input.PasswordHash);

            var newUser = new User
            {
                Name = Input.Name,
                Username = Input.Username,
                Email = Input.Email,
                PasswordHash = hashedPassword,
                Role = Input.Role,
                Contact = Input.Contact,
                Address = Input.Address,
                IsActivated = false,
                ActivationCode = code
            };

            await _db.Users.InsertOneAsync(newUser);
            await _email.SendActivationCodeAsync(newUser.Email, code);

            TempData["RegistrationSuccess"] = "Registered Successfully! Account activation code was sent to your email.";
            return RedirectToPage("/Account/Activate", new { email = newUser.Email });
        }
    }
}

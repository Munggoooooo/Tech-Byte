using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tech_Byte.Services;
using MongoDB.Driver;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Tech_Byte.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly MongoDBService _db;

        public LoginModel(MongoDBService db)
        {
            _db = db;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _db.Users.Find(u => u.Email == Email).FirstOrDefaultAsync();

            if (user == null || !BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return Page();
            }

            if (!user.IsActivated)
            {
                ModelState.AddModelError("", "Account is not activated.");
                return Page();
            }

            // Create login cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),      // User display name
                new Claim(ClaimTypes.Email, user.Email),        // User email
                new Claim(ClaimTypes.Role, user.Role)           // Role: "Admin" or "Member"
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Keeps the user logged in across sessions
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            return RedirectToPage("/Index");
        }
    }
}

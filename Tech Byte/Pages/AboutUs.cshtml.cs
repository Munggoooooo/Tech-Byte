using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tech_Byte.Services;
using Tech_Byte.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Tech_Byte.Pages
{
    [Authorize(Roles = "Admin,Member")]
    public class ContactUsModel : PageModel
    {
        private readonly MongoDBService _db;
        private readonly EmailService _email;

        public ContactUsModel(MongoDBService db, EmailService email)
        {
            _db = db;
            _email = email;
        }

        [BindProperty]
        public string Subject { get; set; }

        [BindProperty]
        public string Message { get; set; }

        public string MessageInfo { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var username = User.Identity.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            var contact = new ContactMessage
            {
                FromUsername = username,
                UserRole = role,
                Email = email,
                Subject = Subject,
                Message = Message,
                Timestamp = DateTime.UtcNow
            };

            // Save to database
            await _db.AddContactMessageAsync(contact);

            // Send email to all admins
            var adminEmails = await _db.GetAdminEmailsAsync();
            foreach (var adminEmail in adminEmails)
            {
                await _email.SendContactMessageAsync(adminEmail, contact);
            }

            MessageInfo = "Your message has been sent to the administrators.";
            return Page();
        }
    }
}

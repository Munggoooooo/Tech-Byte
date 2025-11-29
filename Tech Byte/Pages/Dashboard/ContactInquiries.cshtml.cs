using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Claims;
using Tech_Byte.Models;
using Tech_Byte.Services;

namespace Tech_Byte.Pages.Dashboard
{
    [Authorize(Roles = "Admin")]
    public class ContactInquiriesModel : PageModel
    {
        private readonly MongoDBService _db;
        private readonly EmailService _email;

        public ContactInquiriesModel(MongoDBService db, EmailService email)
        {
            _db = db;
            _email = email;
        }

        public List<ContactMessage> Contacts { get; set; }

        public int UnreadCount { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                Contacts = await _db.SearchContactsAsync(SearchTerm);
            }
            else
            {
                Contacts = await _db.GetAllContactsAsync();
            }

            // Count unread messages
            UnreadCount = Contacts.Count(c => !c.IsRead);
        }

        [BindProperty]
        public string ContactId { get; set; }

        [BindProperty]
        public string ReplyMessage { get; set; }

        public async Task<IActionResult> OnPostMarkReadAsync()
        {
            if (!string.IsNullOrEmpty(ContactId))
            {
                await _db.MarkContactAsReadAsync(ContactId);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReplyAsync()
        {
            if (!string.IsNullOrEmpty(ContactId) && !string.IsNullOrEmpty(ReplyMessage))
            {
                // Update DB
                await _db.ReplyToContactAsync(ContactId, ReplyMessage);

                // Send email to user
                var contact = await _db.Contacts.Find(c => c.Id == ObjectId.Parse(ContactId)).FirstOrDefaultAsync();
                if (contact != null)
                {
                    var adminName = User.Identity.Name ?? "Admin";
                    await _email.SendReplyToUserAsync(contact.Email, adminName, ReplyMessage);
                }
            }
            return RedirectToPage();
        }
    }
}

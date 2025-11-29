using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tech_Byte.Models;
using Tech_Byte.Services;

namespace Tech_Byte.Pages.Dashboard
{
    [Authorize(Roles = "Admin")]
    public class AccountsModel : PageModel
    {
        private readonly MongoDBService _db;

        public AccountsModel(MongoDBService db)
        {
            _db = db;
        }

        public List<User> Users { get; set; }

        public string SearchTerm { get; set; }

        public async Task OnGet(string search)
        {
            SearchTerm = search;

            if (string.IsNullOrWhiteSpace(search))
            {
                Users = await _db.Users.Find(_ => true).ToListAsync();
                return;
            }

            var regex = new BsonRegularExpression(search, "i");

            var filter = Builders<User>.Filter.Or(
                Builders<User>.Filter.Regex(u => u.Name, regex),
                Builders<User>.Filter.Regex(u => u.Username, regex),
                Builders<User>.Filter.Regex(u => u.Email, regex),
                Builders<User>.Filter.Regex(u => u.Role, regex),
                Builders<User>.Filter.Regex(u => u.Contact, regex),
                Builders<User>.Filter.Regex(u => u.Address, regex),
                Builders<User>.Filter.Where(u => (u.IsActivated ? "active" : "inactive")
                    .Contains(search.ToLower()))
            );

            Users = await _db.Users.Find(filter).ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateUser(string Name, string Username, string Email, string Password, string Role, string Contact, string Address)
        {
            var newUser = new User

            {
                Id = ObjectId.GenerateNewId(),
                Name = Name,
                Username = Username,
                Email = Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password),
                Role = Role,
                Contact = Contact,
                Address = Address,
                IsActivated = true
            };

            await _db.Users.InsertOneAsync(newUser);
            await _db.LogAccountAction("Create", User.Identity.Name, newUser);

            TempData["AccountCreationSuccess"] = "Account created!.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditUser(string Id, string Name, string Username, string Email, string Password, string Role, string Contact, string Address)
        {
            var userId = ObjectId.Parse(Id);  // Parse the user Id from the hidden field

            // Find the user by Id
            var user = await _db.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound();  // If user not found, return a 404
            }

            // Update the user properties
            user.Name = Name;
            user.Username = Username;
            user.Email = Email;
            user.Role = Role;
            user.Contact = Contact;
            user.Address = Address;

            // If a new password is provided, hash it using BCrypt
            if (!string.IsNullOrEmpty(Password))
            {
                // Hash the new password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);
                user.PasswordHash = hashedPassword;  // Update the password hash
            }

            // Update the user in the database
            var updateResult = await _db.Users.ReplaceOneAsync(u => u.Id == userId, user);

            // Log the edit action (You may want to log the before and after state of the user here)
            await _db.LogAccountAction("Edit", User.Identity.Name, user);

            TempData["AccountUpdateSuccess"] = "Account updated!.";

            // Redirect to the same page to show updated user list
            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostDeleteUser(string Id)
        {
            // Parse the user ID from the string parameter
            var userId = ObjectId.Parse(Id);

            // Find the user from the database by their ID
            var user = await _db.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();

            // If the user doesn't exist, return a NotFound result
            if (user == null)
            {
                return NotFound(); // User not found
            }

            // Log the delete action before actually deleting the user
            await _db.LogAccountAction("Delete", User.Identity.Name, user);

            // Delete the user from the Users collection
            var deleteResult = await _db.Users.DeleteOneAsync(u => u.Id == userId);

            // If delete operation was successful, redirect back to the page
            if (deleteResult.DeletedCount > 0)
            {
                // Optionally, you could add a success message
                TempData["AccountDeleteSuccess"] = "Account deleted!.";
            }

            // Redirect to the same page to refresh the user list
            return RedirectToPage();
        }

    }
}

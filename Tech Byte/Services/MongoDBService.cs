using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tech_Byte.Models;

namespace Tech_Byte.Services
{
    using MongoDB.Driver;
    using Newtonsoft.Json;
    using Tech_Byte.Utilities;

    public class MongoDBService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<AccountLog> _accountLogs;
        private readonly IMongoCollection<InventoryItem> _items;
        private readonly IMongoCollection<Transaction> _transactions;
        private readonly IMongoCollection<Purchase> _purchases;
        private readonly IMongoCollection<OrderSequence> _orderSequences;
        private readonly IMongoCollection<ContactMessage> _contacts;

        public MongoDBService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("MongoDB"));
            var database = client.GetDatabase("TechByteDB");

            // Collection for Users
            _users = database.GetCollection<User>("Users");

            // Collection for Account logs
            _accountLogs = database.GetCollection<AccountLog>("AccountLogs");

            // Collection for Inventory Items
            _items = database.GetCollection<InventoryItem>("Items");

            // Collection for Transaction logs
            _transactions = database.GetCollection<Transaction>("Transactions");

            // Collection for Purchase logs
            _purchases = database.GetCollection<Purchase>("Purchases");

            // Collection for Order Sequence logs
            _orderSequences = database.GetCollection<OrderSequence>("OrderSequences");

            // Collection for ContactMessages
            _contacts = database.GetCollection<ContactMessage>("ContactMessages");
        }

        // Users
        public IMongoCollection<User> Users => _users;

        // Account Logs
        public IMongoCollection<AccountLog> AccountLogs => _accountLogs;

        // Get User Details
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        // Get Account Logs
        public async Task LogAccountAction(string action, string performedBy, User userData)
        {
            var log = new AccountLog
            {
                Id = ObjectId.GenerateNewId(),
                Action = action,
                PerformedBy = performedBy,
                AccountUsername = userData.Username,
                Timestamp = DateTime.UtcNow,
                Details = JsonConvert.SerializeObject(userData, new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter> { new ObjectIdConverter() }
                })
            };

            await _accountLogs.InsertOneAsync(log);
        }


        // Get Account List
        public async Task<List<AccountLog>> GetAccountLogsAsync()
        {
            return await _accountLogs.Find(_ => true)
                                     .SortByDescending(l => l.Timestamp)
                                     .ToListAsync();
        }


        // Get user Count
        public async Task<long> GetTotalUsersAsync()
        {
            return await Users.CountDocumentsAsync(_ => true);
        }


        // Add new purchase
        public async Task AddPurchaseAsync(Purchase purchase)
        {
            await _purchases.InsertOneAsync(purchase);
        }

        // Get all purchase history
        public async Task<List<Purchase>> GetPurchasesAsync()
        {
            return await _purchases.Find(_ => true)
                                   .SortByDescending(p => p.Date)
                                   .ToListAsync();
        }

        // Get all items
        public async Task<List<InventoryItem>> GetAllAsync() =>
            await _items.Find(_ => true).ToListAsync();

        // Get an item by its ID
        public async Task<InventoryItem> GetByIdAsync(string id) =>
            await _items.Find(i => i.Id == id).FirstOrDefaultAsync();

        // Create a new item
        public async Task CreateAsync(InventoryItem item, string performedBy)
        {
            await _items.InsertOneAsync(item);
            await LogTransactionAsync("Create", item.Id, null, item, performedBy);
        }

        // Update an existing item
        public async Task UpdateAsync(string id, InventoryItem item, string performedBy)
        {
            // Get the current item data before update
            var oldItem = await GetByIdAsync(id);
            if (oldItem == null) return;

            // Log the "Update" transaction
            await LogTransactionAsync("Update", id, oldItem, item, performedBy);

            // Update the item in the database
            await _items.ReplaceOneAsync(i => i.Id == id, item);
        }

        // Delete an item
        public async Task DeleteAsync(string id, string performedBy)
        {
            // Get the item to be deleted
            var itemToDelete = await GetByIdAsync(id);
            if (itemToDelete == null) return;

            // Log the "Delete" transaction
            await LogTransactionAsync("Delete", id, itemToDelete, null, performedBy);

            // Delete the item from the database
            await _items.DeleteOneAsync(i => i.Id == id);
        }

        // Search items by name or description
        public async Task<List<InventoryItem>> SearchItemsAsync(string searchTerm)
        {
            var filter = Builders<InventoryItem>.Filter.Empty;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var regex = new MongoDB.Bson.BsonRegularExpression(searchTerm, "i");
                filter &= Builders<InventoryItem>.Filter.Regex(i => i.Name, regex);
                filter |= Builders<InventoryItem>.Filter.Regex(i => i.Category, regex);

            }

            return await _items.Find(filter).ToListAsync();
        }

        // Search items by category
        public async Task<List<InventoryItem>> SearchItemsByCategoryAsync(string category)
        {
            var filter = Builders<InventoryItem>.Filter.Eq(i => i.Category, category);
            return await _items.Find(filter).ToListAsync();
        }

        // Search Entries in Transactions
        public async Task<List<Transaction>> SearchTransactionsAsync(string searchTerm)
        {
            var filter = Builders<Transaction>.Filter.Empty;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var regex = new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"); // case-insensitive

                filter = Builders<Transaction>.Filter.Or(
                    Builders<Transaction>.Filter.Regex(t => t.ActionType, regex),
                    Builders<Transaction>.Filter.Regex(t => t.PerformedBy, regex),
                    Builders<Transaction>.Filter.Regex(t => t.ItemName, regex),
                    Builders<Transaction>.Filter.Regex(t => t.ItemCategory, regex)
                );
            }

            return await _transactions.Find(filter)
                                      .SortByDescending(t => t.Timestamp)
                                      .ToListAsync();
        }

        // Search Entries on Purchase History
        public async Task<List<Purchase>> SearchPurchasesAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                // No filter, return all
                return await GetPurchasesAsync();
            }

            var regex = new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"); // case-insensitive

            // Build a filter: match CustomerUserName, CustomerEmail, OrderId, or any Item Name
            var filter = Builders<Purchase>.Filter.Or(
                Builders<Purchase>.Filter.Regex(p => p.CustomerUserName, regex),
                Builders<Purchase>.Filter.Regex(p => p.CustomerEmail, regex),
                Builders<Purchase>.Filter.Regex(p => p.OrderId, regex),
                Builders<Purchase>.Filter.ElemMatch(p => p.Items,
                    i => i.Name != null && i.Name.ToLower().Contains(searchTerm.ToLower()))
            );

            return await _purchases.Find(filter)
                                   .SortByDescending(p => p.Date)
                                   .ToListAsync();
        }

        // Search Entries in Contacts
        public async Task<List<ContactMessage>> SearchContactsAsync(string searchTerm)
        {
            var filter = Builders<ContactMessage>.Filter.Empty;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var regex = new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"); // case-insensitive
                filter = Builders<ContactMessage>.Filter.Or(
                    Builders<ContactMessage>.Filter.Regex(c => c.FromUsername, regex),
                    Builders<ContactMessage>.Filter.Regex(c => c.UserRole, regex),
                    Builders<ContactMessage>.Filter.Regex(c => c.Email, regex),
                    Builders<ContactMessage>.Filter.Regex(c => c.Subject, regex),
                    Builders<ContactMessage>.Filter.Regex(c => c.Message, regex)
                );
            }

            return await _contacts.Find(filter)
                                  .SortByDescending(c => c.Timestamp)
                                  .ToListAsync();
        }


        // Log transaction in the database (for Create, Update, and Delete)
        public async Task LogTransactionAsync(string actionType, string itemId, InventoryItem oldItem, InventoryItem newItem, string performedBy, string? orderId = null)
        {
            string itemName = newItem?.Name ?? oldItem?.Name; // Get the name from new or old item
            string imageUrl = newItem?.Image ?? oldItem?.Image;
            string itemCategory = newItem?.Category ?? oldItem?.Category;

            var transaction = new Transaction
            {
                ActionType = actionType,
                ItemId = itemId,
                ItemName = itemName,
                ItemCategory = itemCategory,
                Image = imageUrl,
                Timestamp = DateTime.UtcNow,
                OldData = oldItem != null ? Newtonsoft.Json.JsonConvert.SerializeObject(oldItem) : null,
                NewData = newItem != null ? Newtonsoft.Json.JsonConvert.SerializeObject(newItem) : null,
                OrderId = orderId,
                PerformedBy = performedBy
            };

            await _transactions.InsertOneAsync(transaction);
        }


        // Get all transactions (logs)
        public async Task<List<Transaction>> GetTransactionsAsync()
        {
            return await _transactions.Find(_ => true).ToListAsync();
        }

        public async Task<List<InventoryItem>> GetRecentlyAddedAsync(int count)
        {
            return await _items.Find(_ => true)
                               .SortByDescending(i => i.Id) // ObjectId contains creation timestamp
                               .Limit(count)
                               .ToListAsync();
        }

        // Final Purchase
        public async Task<bool> FinalizePurchaseAsync(Purchase purchase, string performedBy)
        {
            var orderId = await GenerateOrderIdAsync();
            purchase.OrderId = orderId;

            foreach (var cartItem in purchase.Items)
            {
                var inventoryItem = await GetByIdAsync(cartItem.ItemId);
                if (inventoryItem == null || inventoryItem.Quantity < cartItem.Quantity)
                {
                    // Not enough stock or item no longer exists
                    return false;
                }

                // Clone the old item for logging before changes
                var oldItem = new InventoryItem
                {
                    Id = inventoryItem.Id,
                    Name = inventoryItem.Name,
                    Category = inventoryItem.Category,
                    Description = inventoryItem.Description,
                    Price = inventoryItem.Price,
                    Quantity = inventoryItem.Quantity,
                    Image = inventoryItem.Image
                };

                // Update the quantity after purchase
                inventoryItem.Quantity -= cartItem.Quantity;

                // Save the new state for logging
                var newItem = new InventoryItem
                {
                    Id = inventoryItem.Id,
                    Name = inventoryItem.Name,
                    Category = inventoryItem.Category,
                    Description = inventoryItem.Description,
                    Price = inventoryItem.Price,
                    Quantity = inventoryItem.Quantity,
                    Image = inventoryItem.Image
                };

                // Update the inventory in DB
                await UpdateAsync(inventoryItem.Id, inventoryItem, "System");

                // Log the Order transaction with old and new data
                await LogTransactionAsync("Order", inventoryItem.Id, oldItem, newItem, performedBy, orderId);
            }

            // Save the purchase after all stock has been validated and deducted
            await AddPurchaseAsync(purchase);
            return true;
        }

        // Get all Purchases
        public async Task<List<Purchase>> GetAllPurchasesAsync()
        {
            return await _purchases.Find(_ => true).ToListAsync();
        }


        // Order ID Generation
        public async Task<string> GenerateOrderIdAsync()
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");

            var filter = Builders<OrderSequence>.Filter.Eq(s => s.Date, today);
            var update = Builders<OrderSequence>.Update.Inc(s => s.Sequence, 1);
            var options = new FindOneAndUpdateOptions<OrderSequence>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            var result = await _orderSequences.FindOneAndUpdateAsync(filter, update, options);
            int orderNumber = result.Sequence;

            return $"ORD-{today}-{orderNumber:D4}";
        }

        // Product Popularity
        public async Task<List<InventoryItem>> GetMostPopularItemsAsync(int limit)
        {
            // Aggregate to get total quantity sold per item
            var pipeline = new[]
            {
            // Unwind items array so each item is a separate document
            new BsonDocument("$unwind", "$Items"),
            // Group by item ID, sum quantities
            new BsonDocument("$group", new BsonDocument
            {
            { "_id", "$Items.ItemId" },
            { "totalQuantity", new BsonDocument("$sum", "$Items.Quantity") }
            }),
            // Sort descending by quantity sold
            new BsonDocument("$sort", new BsonDocument("totalQuantity", -1)),
            // Limit the number of results
            new BsonDocument("$limit", limit)
            };

            var results = await _purchases.Aggregate<BsonDocument>(pipeline).ToListAsync();

            // Extract the item IDs from results
            var popularItemIds = results.Select(r => r["_id"].AsString).ToList();

            // Filter inventory items to match popular IDs
            var filter = Builders<InventoryItem>.Filter.In(i => i.Id, popularItemIds);
            var popularItems = await _items.Find(filter).ToListAsync();

            // Sort popularItems in order of popularity to match the pipeline output order
            popularItems = popularItems.OrderBy(i => popularItemIds.IndexOf(i.Id)).ToList();

            return popularItems;
        }

        // Add new properties
        public IMongoCollection<ContactMessage> Contacts => _contacts;

        // Add a new contact message
        public async Task AddContactMessageAsync(ContactMessage contact)
        {
            await _contacts.InsertOneAsync(contact);
        }

        // Get all contact messages
        public async Task<List<ContactMessage>> GetAllContactsAsync()
        {
            return await _contacts.Find(_ => true)
                                  .SortByDescending(c => c.Timestamp)
                                  .ToListAsync();
        }

        // Get all admin emails
        public async Task<List<string>> GetAdminEmailsAsync()
        {
            var admins = await _users.Find(u => u.Role == "Admin").ToListAsync();
            return admins.Select(a => a.Email).ToList();
        }

        // Mark a message as read
        public async Task MarkContactAsReadAsync(string id)
        {
            var filter = Builders<ContactMessage>.Filter.Eq(c => c.Id, ObjectId.Parse(id));
            var update = Builders<ContactMessage>.Update.Set(c => c.IsRead, true);
            await _contacts.UpdateOneAsync(filter, update);
        }

        // Add reply message
        public async Task ReplyToContactAsync(string id, string reply)
        {
            var filter = Builders<ContactMessage>.Filter.Eq(c => c.Id, ObjectId.Parse(id));
            var update = Builders<ContactMessage>.Update
                            .Set(c => c.ReplyMessage, reply)
                            .Set(c => c.ReplyTimestamp, DateTime.UtcNow)
                            .Set(c => c.IsRead, true);
            await _contacts.UpdateOneAsync(filter, update);
        }


        // Get Monthly Sales Report
        public async Task<List<MonthlySalesDto>> GetMonthlySalesAsync()
        {
            var pipeline = new[]
            {
        new BsonDocument
        {
            {
                "$group",
                new BsonDocument
                {
                    { "_id",
                        new BsonDocument
                        {
                            { "year", new BsonDocument("$year", "$Date") },
                            { "month", new BsonDocument("$month", "$Date") }
                        }
                    },
                    { "TotalSales", new BsonDocument("$sum", "$Total") },
                    { "Orders", new BsonDocument("$sum", 1) }
                }
            }
        },
        new BsonDocument("$sort",
            new BsonDocument
            {
                { "_id.year", 1 },
                { "_id.month", 1 }
            }
        )
        };

            var result = await _purchases.AggregateAsync<MonthlySalesDto>(pipeline);

            return result.ToList();
        }


        // Get Monthly Item sold per category
        public async Task<List<MonthlySalesCategoryDTO>> GetItemsSoldPerCategoryPerMonthAsync()
        {
            var pipeline = new[]
            {
        new BsonDocument("$unwind", "$Items"),
        new BsonDocument("$addFields",
            new BsonDocument("Items.ItemObjectId",
                new BsonDocument("$toObjectId", "$Items.ItemId")
            )
        ),
        new BsonDocument("$lookup", new BsonDocument
        {
            { "from", "Items" },
            { "localField", "Items.ItemObjectId" },
            { "foreignField", "_id" },
            { "as", "ItemDetails" }
        }),
        new BsonDocument("$unwind", "$ItemDetails"),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", new BsonDocument
                {
                    { "category", "$ItemDetails.Category" },
                    { "year", new BsonDocument("$year", "$Date") },
                    { "month", new BsonDocument("$month", "$Date") }
                }
            },
            { "totalItemsSold", new BsonDocument("$sum", "$Items.Quantity") }
        }),
        new BsonDocument("$sort", new BsonDocument
        {
            { "_id.category", 1 },
            { "_id.year", 1 },
            { "_id.month", 1 }
        })
        };

            var result = await _purchases.Aggregate<BsonDocument>(pipeline).ToListAsync();

            // Debug log
            foreach (var r in result)
                Console.WriteLine(r.ToJson());

            return result.Select(r => new MonthlySalesCategoryDTO
            {
                Category = r["_id"]["category"].AsString,
                Year = r["_id"]["year"].AsInt32,
                Month = r["_id"]["month"].AsInt32,
                TotalItemsSold = r["totalItemsSold"].AsInt32
            }).ToList();
        }


    }

}

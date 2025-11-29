namespace Tech_Byte.Models
{
    public class CartItem
    {
        public string? ItemId { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;
        public int MaxQuantity { get; set; }
        public decimal Total => Price * Quantity;

        // Method to update quantity
        public void UpdateQuantity(int quantity)
        {
            Quantity = quantity;
        }
    }
}

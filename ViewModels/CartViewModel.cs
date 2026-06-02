using BookStoreApp.Models;

namespace BookStoreApp.ViewModels
{
    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        
        public decimal TotalAmount => Items.Sum(i => i.SubTotal);
        
        public int TotalItems => Items.Sum(i => i.Quantity);
    }
}

using BookStoreApp.Models;

namespace BookStoreApp.ViewModels
{
    public class HomeViewModel
    {
        public List<Book> FeaturedBooks { get; set; } = new List<Book>();
        public List<Book> NewArrivals { get; set; } = new List<Book>();
        public List<Book> BestSellers { get; set; } = new List<Book>();
        public List<Category> Categories { get; set; } = new List<Category>();
    }
}

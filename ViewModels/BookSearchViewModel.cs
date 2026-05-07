using BookStoreApp.Models;

namespace BookStoreApp.ViewModels
{
    public class BookSearchViewModel
    {
        public string? Keyword { get; set; }
        public int? CategoryId { get; set; }
        public string? SortBy { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public List<Book> Books { get; set; } = new List<Book>();
        public List<Category> Categories { get; set; } = new List<Category>();
        
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }
}

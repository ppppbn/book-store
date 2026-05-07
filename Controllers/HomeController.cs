using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using BookStoreApp.Data;
using BookStoreApp.Models;
using BookStoreApp.ViewModels;

namespace BookStoreApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BookStoreDbContext _context;
        private readonly IMemoryCache _cache;

        public HomeController(ILogger<HomeController> logger, BookStoreDbContext context, IMemoryCache cache)
        {
            _logger = logger;
            _context = context;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy danh sách thể loại (có cache)
            var categories = await _cache.GetOrCreateAsync("categories_list", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                entry.SlidingExpiration = TimeSpan.FromMinutes(10);
                return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            });

            // Lấy sách bán chạy (có cache)
            var bestSellers = await _cache.GetOrCreateAsync("best_sellers", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                return await _context.Books
                    .Include(b => b.Category)
                    .Include(b => b.OrderDetails)
                    .OrderByDescending(b => b.OrderDetails.Sum(od => od.Quantity))
                    .Take(8)
                    .ToListAsync();
            });

            // Lấy sách mới nhất
            var newArrivals = await _context.Books
                .Include(b => b.Category)
                .OrderByDescending(b => b.CreatedAt)
                .Take(8)
                .ToListAsync();

            // Lấy sách nổi bật (random - lấy về client rồi random)
            var allAvailableBooks = await _context.Books
                .Include(b => b.Category)
                .Where(b => b.StockQuantity > 0)
                .ToListAsync();
            
            var featuredBooks = allAvailableBooks
                .OrderBy(b => Guid.NewGuid())
                .Take(4)
                .ToList();

            var viewModel = new HomeViewModel
            {
                Categories = categories ?? new List<Category>(),
                BestSellers = bestSellers ?? new List<Book>(),
                NewArrivals = newArrivals,
                FeaturedBooks = featuredBooks
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using BookStoreApp.Data;
using BookStoreApp.Models;
using BookStoreApp.ViewModels;

namespace BookStoreApp.Controllers
{
    public class BooksController : Controller
    {
        private readonly BookStoreDbContext _context;
        private readonly IMemoryCache _cache;

        public BooksController(BookStoreDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: Books (Public - Danh sách sách cho khách hàng)
        public async Task<IActionResult> Index(int? categoryId, int page = 1, string? keyword = null)
        {
            var viewModel = await GetBookSearchViewModelAsync(categoryId, page, keyword);
            return View(viewModel);
        }

        // GET: Books/Search (AJAX search)
        public async Task<IActionResult> Search(int? categoryId, int page = 1, string? keyword = null)
        {
            var viewModel = await GetBookSearchViewModelAsync(categoryId, page, keyword);
            return PartialView("_BookListPartial", viewModel);
        }

        private async Task<BookSearchViewModel> GetBookSearchViewModelAsync(int? categoryId, int page, string? keyword)
        {
            const int pageSize = 10;

            // Lấy danh sách thể loại (có cache)
            var categories = await _cache.GetOrCreateAsync("categories_list", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                entry.SlidingExpiration = TimeSpan.FromMinutes(10);
                return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            });

            // Query sách
            var query = _context.Books.Include(b => b.Category).AsQueryable();

            // Filter theo thể loại
            if (categoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }

            // Tìm kiếm theo từ khóa
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(b => 
                    b.Title.ToLower().Contains(lowerKeyword) || 
                    b.Author.ToLower().Contains(lowerKeyword));
            }

            // Đếm tổng số sách
            var totalItems = await query.CountAsync();

            // Phân trang
            var books = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new BookSearchViewModel
            {
                Books = books,
                Categories = categories ?? new List<Category>(),
                CategoryId = categoryId,
                Keyword = keyword,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        // GET: Books/Details/5 (Public)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (book == null)
            {
                return NotFound();
            }

            // Lấy sách cùng thể loại
            ViewBag.RelatedBooks = await _context.Books
                .Where(b => b.CategoryId == book.CategoryId && b.Id != book.Id)
                .Take(4)
                .ToListAsync();

            return View(book);
        }

        // ==================== ADMIN ACTIONS ====================

        // GET: Books/Manage (Admin - Quản lý sách)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage(int page = 1, string? keyword = null)
        {
            const int pageSize = 15;

            var query = _context.Books.Include(b => b.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(b => 
                    b.Title.ToLower().Contains(lowerKeyword) || 
                    b.Author.ToLower().Contains(lowerKeyword));
            }

            var totalItems = await query.CountAsync();
            var books = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new BookSearchViewModel
            {
                Books = books,
                Keyword = keyword,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return View(viewModel);
        }

        // GET: Books/Create (Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewData["CategoryId"] = new SelectList(
                await _context.Categories.OrderBy(c => c.Name).ToListAsync(), 
                "Id", "Name");
            return View();
        }

        // POST: Books/Create (Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Title,Author,Publisher,PublishedYear,Price,StockQuantity,Description,ImageUrl,CategoryId")] Book book)
        {
            if (ModelState.IsValid)
            {
                book.CreatedAt = DateTime.Now;
                _context.Add(book);
                await _context.SaveChangesAsync();
                
                // Xóa cache
                _cache.Remove("best_sellers");
                
                TempData["SuccessMessage"] = "Thêm sách thành công!";
                return RedirectToAction(nameof(Manage));
            }
            
            ViewData["CategoryId"] = new SelectList(
                await _context.Categories.OrderBy(c => c.Name).ToListAsync(), 
                "Id", "Name", book.CategoryId);
            return View(book);
        }

        // GET: Books/Edit/5 (Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            
            ViewData["CategoryId"] = new SelectList(
                await _context.Categories.OrderBy(c => c.Name).ToListAsync(), 
                "Id", "Name", book.CategoryId);
            return View(book);
        }

        // POST: Books/Edit/5 (Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,Publisher,PublishedYear,Price,StockQuantity,Description,ImageUrl,CategoryId,CreatedAt")] Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                    
                    // Xóa cache
                    _cache.Remove("best_sellers");
                    
                    TempData["SuccessMessage"] = "Cập nhật sách thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Manage));
            }
            
            ViewData["CategoryId"] = new SelectList(
                await _context.Categories.OrderBy(c => c.Name).ToListAsync(), 
                "Id", "Name", book.CategoryId);
            return View(book);
        }

        // GET: Books/Delete/5 (Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5 (Admin)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                // Kiểm tra xem sách có trong đơn hàng nào không
                var hasOrders = await _context.OrderDetails.AnyAsync(od => od.BookId == id);
                if (hasOrders)
                {
                    TempData["ErrorMessage"] = "Không thể xóa sách này vì đã có trong đơn hàng.";
                    return RedirectToAction(nameof(Manage));
                }

                // Xóa các cart items liên quan
                var cartItems = await _context.CartItems.Where(ci => ci.BookId == id).ToListAsync();
                _context.CartItems.RemoveRange(cartItems);

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                
                // Xóa cache
                _cache.Remove("best_sellers");
                
                TempData["SuccessMessage"] = "Xóa sách thành công!";
            }
            
            return RedirectToAction(nameof(Manage));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}

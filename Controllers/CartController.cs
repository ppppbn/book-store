using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreApp.Data;
using BookStoreApp.Models;
using BookStoreApp.ViewModels;

namespace BookStoreApp.Controllers
{
    public class CartController : Controller
    {
        private readonly BookStoreDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(BookStoreDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Cart
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.CartItems
                .Include(ci => ci.Book)
                    .ThenInclude(b => b!.Category)
                .Where(ci => ci.UserId == userId)
                .OrderByDescending(ci => ci.AddedAt)
                .ToListAsync();

            var viewModel = new CartViewModel
            {
                Items = cartItems
            };

            return View(viewModel);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int bookId, int quantity = 1)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để thêm sách vào giỏ hàng." });
            }

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return Json(new { success = false, message = "Sách không tồn tại." });
            }

            if (book.StockQuantity < quantity)
            {
                return Json(new { success = false, message = "Số lượng sách trong kho không đủ." });
            }

            // Kiểm tra xem sách đã có trong giỏ hàng chưa
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.BookId == bookId);

            if (existingItem != null)
            {
                // Cập nhật số lượng
                existingItem.Quantity += quantity;
                if (existingItem.Quantity > book.StockQuantity)
                {
                    existingItem.Quantity = book.StockQuantity;
                }
                _context.Update(existingItem);
            }
            else
            {
                // Thêm mới
                var cartItem = new CartItem
                {
                    UserId = userId,
                    BookId = bookId,
                    Quantity = quantity,
                    AddedAt = DateTime.Now
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            var cartCount = await GetCartCountAsync(userId);
            return Json(new { success = true, message = "Đã thêm sách vào giỏ hàng!", cartCount });
        }

        private async Task<IActionResult> ProcessBuyNow(int bookId, int quantity)
        {
            var userId = _userManager.GetUserId(User);
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return NotFound();
            }

            if (book.StockQuantity < quantity)
            {
                TempData["ErrorMessage"] = "Số lượng sách trong kho không đủ.";
                return RedirectToAction("Details", "Books", new { id = bookId });
            }

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.BookId == bookId);

            int cartItemId;
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                if (existingItem.Quantity > book.StockQuantity)
                {
                    existingItem.Quantity = book.StockQuantity;
                }
                _context.Update(existingItem);
                await _context.SaveChangesAsync();
                cartItemId = existingItem.Id;
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = userId!,
                    BookId = bookId,
                    Quantity = quantity,
                    AddedAt = DateTime.Now
                };
                _context.CartItems.Add(cartItem);
                await _context.SaveChangesAsync();
                cartItemId = cartItem.Id;
            }

            return RedirectToAction("Checkout", "Orders", new { selectedIds = cartItemId.ToString() });
        }

        // POST: Cart/BuyNow
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyNow(int bookId, int quantity = 1)
        {
            return await ProcessBuyNow(bookId, quantity);
        }

        // GET: Cart/BuyNow
        [Authorize]
        [HttpGet]
        [ActionName("BuyNow")]
        public async Task<IActionResult> BuyNowGet(int bookId, int quantity = 1)
        {
            return await ProcessBuyNow(bookId, quantity);
        }

        // POST: Cart/UpdateQuantity
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = _userManager.GetUserId(User);
            var cartItem = await _context.CartItems
                .Include(ci => ci.Book)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == userId);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng." });
            }

            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Số lượng không hợp lệ." });
            }

            if (cartItem.Book != null && quantity > cartItem.Book.StockQuantity)
            {
                return Json(new { success = false, message = "Số lượng vượt quá tồn kho." });
            }

            cartItem.Quantity = quantity;
            _context.Update(cartItem);
            await _context.SaveChangesAsync();

            var subTotal = cartItem.Book != null ? cartItem.Book.Price * quantity : 0;
            var cartTotal = await GetCartTotalAsync(userId!);
            var cartCount = await GetCartCountAsync(userId!);

            return Json(new { 
                success = true, 
                subTotal = subTotal,
                cartTotal = cartTotal,
                cartCount = cartCount
            });
        }

        // POST: Cart/RemoveFromCart
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userId = _userManager.GetUserId(User);
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == userId);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng." });
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            var cartTotal = await GetCartTotalAsync(userId!);
            var cartCount = await GetCartCountAsync(userId!);

            return Json(new { 
                success = true, 
                message = "Đã xóa sản phẩm khỏi giỏ hàng.",
                cartTotal = cartTotal,
                cartCount = cartCount
            });
        }

        // GET: Cart/GetCartCount (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { count = 0 });
            }

            var count = await GetCartCountAsync(userId);
            return Json(new { count });
        }

        // GET: Cart/GetCartSummary (AJAX - Partial View)
        [HttpGet]
        public async Task<IActionResult> GetCartSummary()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return PartialView("_CartSummaryPartial", new CartViewModel());
            }

            var cartItems = await _context.CartItems
                .Include(ci => ci.Book)
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            var viewModel = new CartViewModel
            {
                Items = cartItems
            };

            return PartialView("_CartSummaryPartial", viewModel);
        }

        private async Task<int> GetCartCountAsync(string userId)
        {
            return await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .SumAsync(ci => ci.Quantity);
        }

        private async Task<decimal> GetCartTotalAsync(string userId)
        {
            return await _context.CartItems
                .Include(ci => ci.Book)
                .Where(ci => ci.UserId == userId)
                .SumAsync(ci => ci.Book != null ? ci.Book.Price * ci.Quantity : 0);
        }
    }
}

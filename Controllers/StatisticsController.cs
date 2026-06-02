using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreApp.Data;
using BookStoreApp.Models.Enums;
using BookStoreApp.ViewModels;

namespace BookStoreApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StatisticsController : Controller
    {
        private readonly BookStoreDbContext _context;

        public StatisticsController(BookStoreDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);

            var viewModel = new StatisticsViewModel
            {
                // Tổng số sách
                TotalBooks = await _context.Books.CountAsync(),

                // Tổng số đơn hàng
                TotalOrders = await _context.Orders.CountAsync(),

                // Tổng số khách hàng
                TotalCustomers = await _context.Users.CountAsync() - 1, // Trừ admin

                // Khách hàng mới trong tháng
                NewCustomersThisMonth = await _context.Users
                    .Where(u => u.Id != null)
                    .CountAsync() - 1, // Simplified - could track registration date

                // Doanh thu tháng này
                RevenueThisMonth = await _context.Orders
                    .Where(o => o.OrderDate >= startOfMonth && o.Status == OrderStatus.Completed)
                    .SumAsync(o => o.TotalAmount),

                // Doanh thu tháng trước
                RevenueLastMonth = await _context.Orders
                    .Where(o => o.OrderDate >= startOfLastMonth && 
                               o.OrderDate < startOfMonth && 
                               o.Status == OrderStatus.Completed)
                    .SumAsync(o => o.TotalAmount),

                // Tổng doanh thu
                TotalRevenue = await _context.Orders
                    .Where(o => o.Status == OrderStatus.Completed)
                    .SumAsync(o => o.TotalAmount),

                // Số đơn hàng theo trạng thái
                PendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
                ConfirmedOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Confirmed),
                ShippingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Shipping),
                CompletedOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Completed),
                CancelledOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Cancelled),

                // Top 10 sách bán chạy
                TopSellingBooks = await _context.OrderDetails
                    .Include(od => od.Book)
                    .Where(od => od.Order!.Status == OrderStatus.Completed)
                    .GroupBy(od => new { od.BookId, od.Book!.Title, od.Book.Author, od.Book.ImageUrl })
                    .Select(g => new TopSellingBook
                    {
                        BookId = g.Key.BookId,
                        Title = g.Key.Title,
                        Author = g.Key.Author,
                        ImageUrl = g.Key.ImageUrl,
                        TotalSold = g.Sum(od => od.Quantity),
                        TotalRevenue = g.Sum(od => od.Quantity * od.UnitPrice)
                    })
                    .OrderByDescending(b => b.TotalSold)
                    .Take(10)
                    .ToListAsync(),

                // Doanh thu 6 tháng gần nhất
                MonthlyRevenues = await GetMonthlyRevenuesAsync()
            };

            return View(viewModel);
        }

        private async Task<List<MonthlyRevenue>> GetMonthlyRevenuesAsync()
        {
            var result = new List<MonthlyRevenue>();
            var now = DateTime.Now;

            for (int i = 5; i >= 0; i--)
            {
                var month = now.AddMonths(-i);
                var startOfMonth = new DateTime(month.Year, month.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1);

                var revenue = await _context.Orders
                    .Where(o => o.OrderDate >= startOfMonth && 
                               o.OrderDate < endOfMonth && 
                               o.Status == OrderStatus.Completed)
                    .SumAsync(o => o.TotalAmount);

                var orderCount = await _context.Orders
                    .Where(o => o.OrderDate >= startOfMonth && 
                               o.OrderDate < endOfMonth && 
                               o.Status == OrderStatus.Completed)
                    .CountAsync();

                result.Add(new MonthlyRevenue
                {
                    Year = month.Year,
                    Month = month.Month,
                    MonthName = $"T{month.Month}/{month.Year}",
                    Revenue = revenue,
                    OrderCount = orderCount
                });
            }

            return result;
        }
    }
}

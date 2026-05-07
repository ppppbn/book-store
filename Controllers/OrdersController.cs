using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreApp.Data;
using BookStoreApp.Models;
using BookStoreApp.Models.Enums;
using BookStoreApp.ViewModels;

namespace BookStoreApp.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly BookStoreDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(BookStoreDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Orders/Checkout
        public async Task<IActionResult> Checkout()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId!);
            
            var cartItems = await _context.CartItems
                .Include(ci => ci.Book)
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            var viewModel = new CheckoutViewModel
            {
                ShippingAddress = user?.Address ?? string.Empty,
                CartItems = cartItems,
                PaymentMethod = "COD"
            };

            return View(viewModel);
        }

        // POST: Orders/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            
            var cartItems = await _context.CartItems
                .Include(ci => ci.Book)
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            model.CartItems = cartItems;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra tồn kho
            foreach (var item in cartItems)
            {
                if (item.Book == null || item.Quantity > item.Book.StockQuantity)
                {
                    ModelState.AddModelError("", $"Sách \"{item.Book?.Title}\" không đủ số lượng trong kho.");
                    return View(model);
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tạo đơn hàng
                var order = new Order
                {
                    UserId = userId!,
                    OrderDate = DateTime.Now,
                    TotalAmount = cartItems.Sum(ci => ci.SubTotal),
                    Status = OrderStatus.Pending,
                    ShippingAddress = model.ShippingAddress,
                    PaymentMethod = model.PaymentMethod,
                    Note = model.Note
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Tạo chi tiết đơn hàng và trừ tồn kho
                foreach (var item in cartItems)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        BookId = item.BookId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Book!.Price
                    };
                    _context.OrderDetails.Add(orderDetail);

                    // Trừ tồn kho
                    item.Book.StockQuantity -= item.Quantity;
                    _context.Books.Update(item.Book);
                }

                // Xóa giỏ hàng
                _context.CartItems.RemoveRange(cartItems);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(PaymentConfirmation), new { orderId = order.Id });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Có lỗi xảy ra khi xử lý đơn hàng. Vui lòng thử lại.");
                return View(model);
            }
        }

        // GET: Orders/PaymentConfirmation/5
        public async Task<IActionResult> PaymentConfirmation(int orderId)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/MyOrders
        public async Task<IActionResult> MyOrders(int page = 1)
        {
            const int pageSize = 10;
            var userId = _userManager.GetUserId(User);

            var query = _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate);

            var totalItems = await query.CountAsync();
            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new OrderListViewModel
            {
                Orders = orders,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return View(viewModel);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền truy cập
            if (!isAdmin && order.UserId != userId)
            {
                return Forbid();
            }

            return View(order);
        }

        // ==================== ADMIN ACTIONS ====================

        // GET: Orders/Manage (Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage(OrderStatus? status, int page = 1)
        {
            const int pageSize = 15;

            var query = _context.Orders
                .Include(o => o.User)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            var totalItems = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new OrderListViewModel
            {
                Orders = orders,
                FilterStatus = status,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return View(viewModel);
        }

        // POST: Orders/UpdateStatus (Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
            }

            // Validate trạng thái chuyển đổi hợp lệ
            var validTransitions = new Dictionary<OrderStatus, OrderStatus[]>
            {
                { OrderStatus.Pending, new[] { OrderStatus.Confirmed, OrderStatus.Cancelled } },
                { OrderStatus.Confirmed, new[] { OrderStatus.Shipping, OrderStatus.Cancelled } },
                { OrderStatus.Shipping, new[] { OrderStatus.Completed, OrderStatus.Cancelled } },
                { OrderStatus.Completed, Array.Empty<OrderStatus>() },
                { OrderStatus.Cancelled, Array.Empty<OrderStatus>() }
            };

            if (!validTransitions[order.Status].Contains(newStatus))
            {
                return Json(new { success = false, message = "Không thể chuyển đổi trạng thái này." });
            }

            // Nếu hủy đơn hàng, hoàn lại tồn kho
            if (newStatus == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
            {
                var orderDetails = await _context.OrderDetails
                    .Include(od => od.Book)
                    .Where(od => od.OrderId == orderId)
                    .ToListAsync();

                foreach (var detail in orderDetails)
                {
                    if (detail.Book != null)
                    {
                        detail.Book.StockQuantity += detail.Quantity;
                        _context.Books.Update(detail.Book);
                    }
                }
            }

            order.Status = newStatus;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật trạng thái thành công.", statusDisplay = order.StatusDisplay });
        }

        // POST: Orders/BulkUpdateStatus (Admin) - API cho bulk actions
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkUpdateStatus(string orderIds, string action)
        {
            if (string.IsNullOrEmpty(orderIds))
            {
                return Json(new { success = false, message = "Vui lòng chọn ít nhất một đơn hàng." });
            }

            var orderIdList = orderIds.Split(',').Select(int.Parse).ToList();

            var orders = await _context.Orders
                .Where(o => orderIdList.Contains(o.Id))
                .ToListAsync();

            if (!orders.Any())
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng nào." });
            }

            // Validate: chỉ cho phép thao tác trên đơn hàng ở trạng thái Pending
            var invalidOrders = orders.Where(o => o.Status != OrderStatus.Pending).ToList();
            if (invalidOrders.Any())
            {
                return Json(new { 
                    success = false, 
                    message = $"Có {invalidOrders.Count} đơn hàng không ở trạng thái 'Chờ xác nhận'. Chỉ có thể xác nhận/hủy đơn hàng đang chờ xác nhận." 
                });
            }

            var newStatus = action == "confirm" ? OrderStatus.Confirmed : OrderStatus.Cancelled;
            int successCount = 0;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var order in orders)
                {
                    // Nếu hủy đơn hàng, hoàn lại tồn kho
                    if (newStatus == OrderStatus.Cancelled)
                    {
                        var orderDetails = await _context.OrderDetails
                            .Include(od => od.Book)
                            .Where(od => od.OrderId == order.Id)
                            .ToListAsync();

                        foreach (var detail in orderDetails)
                        {
                            if (detail.Book != null)
                            {
                                detail.Book.StockQuantity += detail.Quantity;
                                _context.Books.Update(detail.Book);
                            }
                        }
                    }

                    order.Status = newStatus;
                    _context.Orders.Update(order);
                    successCount++;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var actionText = action == "confirm" ? "xác nhận" : "hủy";
                return Json(new { 
                    success = true, 
                    message = $"Đã {actionText} thành công {successCount} đơn hàng." 
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Có lỗi xảy ra. Vui lòng thử lại." });
            }
        }
    }
}

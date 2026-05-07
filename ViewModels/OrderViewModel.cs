using BookStoreApp.Models;
using BookStoreApp.Models.Enums;

namespace BookStoreApp.ViewModels
{
    public class OrderViewModel
    {
        public Order Order { get; set; } = new Order();
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }

    public class OrderListViewModel
    {
        public List<Order> Orders { get; set; } = new List<Order>();
        public OrderStatus? FilterStatus { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }

    public class UpdateOrderStatusViewModel
    {
        public int OrderId { get; set; }
        public OrderStatus NewStatus { get; set; }
    }
}

namespace BookStoreApp.Models.Enums
{
    /// <summary>
    /// Trạng thái đơn hàng
    /// </summary>
    public enum OrderStatus
    {
        Pending = 0,      // Chờ xác nhận
        Confirmed = 1,    // Đã xác nhận
        Shipping = 2,     // Đang giao hàng
        Completed = 3,    // Hoàn thành
        Cancelled = 4     // Đã hủy
    }
}

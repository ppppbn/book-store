namespace BookStoreApp.ViewModels
{
    public class StatisticsViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }

        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueLastMonth { get; set; }
        public decimal TotalRevenue { get; set; }

        public int PendingOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int ShippingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }

        public List<TopSellingBook> TopSellingBooks { get; set; } = new List<TopSellingBook>();
        public List<MonthlyRevenue> MonthlyRevenues { get; set; } = new List<MonthlyRevenue>();
    }

    public class TopSellingBook
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class MonthlyRevenue
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }
}

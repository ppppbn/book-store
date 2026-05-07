using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BookStoreApp.Models;

namespace BookStoreApp.Data
{
    /// <summary>
    /// Khởi tạo dữ liệu mẫu cho database
    /// </summary>
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var context = new BookStoreDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<BookStoreDbContext>>());

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Tạo roles
            await CreateRolesAsync(roleManager);

            // Tạo admin user
            await CreateAdminUserAsync(userManager);

            // Seed categories và books nếu chưa có
            if (!await context.Categories.AnyAsync())
            {
                await SeedCategoriesAndBooksAsync(context);
            }
        }

        private static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "Customer" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task CreateAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            const string adminEmail = "admin@bookstore.com";
            const string adminPassword = "Admin@123";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Quản trị viên",
                    Address = "123 Đường ABC, Quận 1, TP.HCM",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        private static async Task SeedCategoriesAndBooksAsync(BookStoreDbContext context)
        {
            // Tạo thể loại
            var categories = new List<Category>
            {
                new Category { Name = "Văn học Việt Nam", Description = "Các tác phẩm văn học của các tác giả Việt Nam" },
                new Category { Name = "Văn học Nước ngoài", Description = "Các tác phẩm văn học dịch từ nước ngoài" },
                new Category { Name = "Khoa học", Description = "Sách về khoa học tự nhiên, vật lý, hóa học, sinh học" },
                new Category { Name = "Kinh tế", Description = "Sách về kinh tế, kinh doanh, tài chính" },
                new Category { Name = "Công nghệ", Description = "Sách về công nghệ thông tin, lập trình, AI" },
                new Category { Name = "Thiếu nhi", Description = "Sách dành cho trẻ em và thiếu niên" },
                new Category { Name = "Tâm lý - Kỹ năng sống", Description = "Sách về tâm lý học, phát triển bản thân" }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            // Tạo sách mẫu với hình ảnh từ internet
            var books = new List<Book>
            {
                // Văn học Việt Nam (CategoryId = 1)
                new Book
                {
                    Title = "Dế Mèn Phiêu Lưu Ký",
                    Author = "Tô Hoài",
                    Publisher = "NXB Kim Đồng",
                    PublishedYear = 2020,
                    Price = 85000,
                    StockQuantity = 50,
                    Description = "Câu chuyện phiêu lưu của chú Dế Mèn qua nhiều vùng đất, gặp gỡ nhiều bạn bè và học được nhiều bài học quý giá về cuộc sống.",
                    ImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1390702482i/20526127.jpg",
                    CategoryId = categories[0].Id
                },
                new Book
                {
                    Title = "Số Đỏ",
                    Author = "Vũ Trọng Phụng",
                    Publisher = "NXB Văn Học",
                    PublishedYear = 2019,
                    Price = 95000,
                    StockQuantity = 35,
                    Description = "Tiểu thuyết trào phúng nổi tiếng của văn học Việt Nam, phản ánh xã hội Việt Nam những năm 1930.",
                    ImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1546031081i/23346417.jpg",
                    CategoryId = categories[0].Id
                },
                new Book
                {
                    Title = "Truyện Kiều",
                    Author = "Nguyễn Du",
                    Publisher = "NXB Giáo Dục",
                    PublishedYear = 2021,
                    Price = 75000,
                    StockQuantity = 100,
                    Description = "Kiệt tác văn học Việt Nam, kể về cuộc đời đầy bi kịch của nàng Kiều.",
                    ImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1328764288i/7979711.jpg",
                    CategoryId = categories[0].Id
                },

                // Văn học Nước ngoài (CategoryId = 2)
                new Book
                {
                    Title = "Đắc Nhân Tâm",
                    Author = "Dale Carnegie",
                    Publisher = "NXB Tổng Hợp",
                    PublishedYear = 2022,
                    Price = 120000,
                    StockQuantity = 80,
                    Description = "Cuốn sách kinh điển về nghệ thuật giao tiếp và ứng xử với mọi người.",
                    ImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1442726934i/4865.jpg",
                    CategoryId = categories[1].Id
                },
                new Book
                {
                    Title = "Nhà Giả Kim",
                    Author = "Paulo Coelho",
                    Publisher = "NXB Hội Nhà Văn",
                    PublishedYear = 2021,
                    Price = 79000,
                    StockQuantity = 60,
                    Description = "Câu chuyện về chàng chăn cừu Santiago và hành trình theo đuổi giấc mơ của mình.",
                    ImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1654371463i/18144590.jpg",
                    CategoryId = categories[1].Id
                },
                new Book
                {
                    Title = "1984",
                    Author = "George Orwell",
                    Publisher = "NXB Văn Học",
                    PublishedYear = 2020,
                    Price = 110000,
                    StockQuantity = 45,
                    Description = "Tiểu thuyết dystopia kinh điển về một xã hội toàn trị trong tương lai.",
                    ImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1657781256i/61439040.jpg",
                    CategoryId = categories[1].Id
                },

                // Khoa học (CategoryId = 3)
                new Book
                {
                    Title = "Lược Sử Thời Gian",
                    Author = "Stephen Hawking",
                    Publisher = "NXB Trẻ",
                    PublishedYear = 2020,
                    Price = 135000,
                    StockQuantity = 30,
                    Description = "Giải thích các khái niệm phức tạp về vũ trụ một cách dễ hiểu.",
                    ImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1333578746i/3869.jpg",
                    CategoryId = categories[2].Id
                },
                new Book
                {
                    Title = "Sapiens: Lược Sử Loài Người",
                    Author = "Yuval Noah Harari",
                    Publisher = "NXB Thế Giới",
                    PublishedYear = 2022,
                    Price = 199000,
                    StockQuantity = 55,
                    Description = "Hành trình 70,000 năm của loài người từ thời kỳ đồ đá đến thế kỷ 21.",
                    ImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1595674533i/23692271.jpg",
                    CategoryId = categories[2].Id
                },
                new Book
                {
                    Title = "Cosmos",
                    Author = "Carl Sagan",
                    Publisher = "NXB Khoa Học",
                    PublishedYear = 2019,
                    Price = 185000,
                    StockQuantity = 25,
                    Description = "Khám phá vũ trụ và vị trí của con người trong đó.",
                    ImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1388620656i/55030.jpg",
                    CategoryId = categories[2].Id
                },

                // Kinh tế (CategoryId = 4)
                new Book
                {
                    Title = "Cha Giàu Cha Nghèo",
                    Author = "Robert Kiyosaki",
                    Publisher = "NXB Trẻ",
                    PublishedYear = 2021,
                    Price = 140000,
                    StockQuantity = 70,
                    Description = "Những bài học về tài chính cá nhân và đầu tư.",
                    ImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1388211242i/69571.jpg",
                    CategoryId = categories[3].Id
                },
                new Book
                {
                    Title = "Nghĩ Giàu Làm Giàu",
                    Author = "Napoleon Hill",
                    Publisher = "NXB Tổng Hợp",
                    PublishedYear = 2020,
                    Price = 125000,
                    StockQuantity = 40,
                    Description = "13 nguyên tắc để đạt được thành công và giàu có.",
                    ImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1463241782i/30186948.jpg",
                    CategoryId = categories[3].Id
                },
                new Book
                {
                    Title = "Chiến Lược Đại Dương Xanh",
                    Author = "W. Chan Kim",
                    Publisher = "NXB Kinh Tế",
                    PublishedYear = 2022,
                    Price = 175000,
                    StockQuantity = 35,
                    Description = "Chiến lược kinh doanh đột phá để tạo ra thị trường mới.",
                    ImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1416445924i/4898.jpg",
                    CategoryId = categories[3].Id
                },

                // Công nghệ (CategoryId = 5)
                new Book
                {
                    Title = "Clean Code",
                    Author = "Robert C. Martin",
                    Publisher = "NXB Bách Khoa",
                    PublishedYear = 2021,
                    Price = 350000,
                    StockQuantity = 25,
                    Description = "Hướng dẫn viết code sạch và dễ bảo trì.",
                    ImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1436202607i/3735293.jpg",
                    CategoryId = categories[4].Id
                },
                new Book
                {
                    Title = "The Pragmatic Programmer",
                    Author = "David Thomas, Andrew Hunt",
                    Publisher = "NXB Thống Kê",
                    PublishedYear = 2023,
                    Price = 320000,
                    StockQuantity = 40,
                    Description = "Hướng dẫn trở thành lập trình viên thực thụ với các kỹ năng thực tế.",
                    ImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1401432508i/4099.jpg",
                    CategoryId = categories[4].Id
                },
                new Book
                {
                    Title = "Design Patterns",
                    Author = "Gang of Four",
                    Publisher = "NXB Đại Học Quốc Gia",
                    PublishedYear = 2022,
                    Price = 450000,
                    StockQuantity = 20,
                    Description = "Các mẫu thiết kế phần mềm hướng đối tượng kinh điển.",
                    ImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1348027904i/85009.jpg",
                    CategoryId = categories[4].Id
                },

                // Thiếu nhi (CategoryId = 6)
                new Book
                {
                    Title = "Harry Potter và Hòn Đá Phù Thủy",
                    Author = "J.K. Rowling",
                    Publisher = "NXB Trẻ",
                    PublishedYear = 2021,
                    Price = 150000,
                    StockQuantity = 90,
                    Description = "Tập đầu tiên trong series Harry Potter huyền thoại.",
                    ImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1474154022i/3.jpg",
                    CategoryId = categories[5].Id
                },
                new Book
                {
                    Title = "Charlie và Nhà Máy Sô-cô-la",
                    Author = "Roald Dahl",
                    Publisher = "NXB Kim Đồng",
                    PublishedYear = 2022,
                    Price = 95000,
                    StockQuantity = 150,
                    Description = "Câu chuyện kỳ diệu về cậu bé Charlie và chuyến tham quan nhà máy sô-cô-la.",
                    ImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1309211401i/6310.jpg",
                    CategoryId = categories[5].Id
                },
                new Book
                {
                    Title = "Hoàng Tử Bé",
                    Author = "Antoine de Saint-Exupéry",
                    Publisher = "NXB Hội Nhà Văn",
                    PublishedYear = 2020,
                    Price = 65000,
                    StockQuantity = 75,
                    Description = "Câu chuyện triết lý nhẹ nhàng về tình bạn và cuộc sống.",
                    ImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1367545443i/157993.jpg",
                    CategoryId = categories[5].Id
                },

                // Tâm lý - Kỹ năng sống (CategoryId = 7)
                new Book
                {
                    Title = "Đời Ngắn Đừng Ngủ Dài",
                    Author = "Robin Sharma",
                    Publisher = "NXB Trẻ",
                    PublishedYear = 2021,
                    Price = 98000,
                    StockQuantity = 55,
                    Description = "Những bài học về quản lý thời gian và sống có ý nghĩa.",
                    ImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1312051574i/6138802.jpg",
                    CategoryId = categories[6].Id
                },
                new Book
                {
                    Title = "Sức Mạnh Của Thói Quen",
                    Author = "Charles Duhigg",
                    Publisher = "NXB Lao Động",
                    PublishedYear = 2022,
                    Price = 135000,
                    StockQuantity = 45,
                    Description = "Hiểu về thói quen và cách thay đổi chúng để thành công.",
                    ImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1545854312i/12609433.jpg",
                    CategoryId = categories[6].Id
                },
                new Book
                {
                    Title = "Atomic Habits",
                    Author = "James Clear",
                    Publisher = "NXB Thế Giới",
                    PublishedYear = 2023,
                    Price = 168000,
                    StockQuantity = 60,
                    Description = "Phương pháp xây dựng thói quen tốt và loại bỏ thói quen xấu.",
                    ImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1655988385i/40121378.jpg",
                    CategoryId = categories[6].Id
                }
            };

            await context.Books.AddRangeAsync(books);
            await context.SaveChangesAsync();
        }
    }
}

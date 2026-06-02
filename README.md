# Nhà Sách Online - Ứng dụng Quản lý Cửa hàng Bán sách Trực tuyến

## Giới thiệu
Đây là ứng dụng web quản lý cửa hàng bán sách trực tuyến được xây dựng bằng ASP.NET Core MVC. Ứng dụng bao gồm đầy đủ các chức năng: quản lý sách, thể loại, giỏ hàng, đặt hàng, authentication/authorization, và thống kê.

## Công nghệ sử dụng
- **Framework:** ASP.NET Core MVC (.NET 9)
- **Database:** SQLite (Entity Framework Core - Code First)
- **Authentication:** ASP.NET Core Identity
- **Frontend:** Razor Views + Bootstrap 5 + jQuery
- **Caching:** IMemoryCache + ResponseCache
- **Language:** C#, giao diện tiếng Việt

## Tính năng chính

### Người dùng khách (Guest)
- Xem danh sách sách, chi tiết sách
- Tìm kiếm sách theo tên, tác giả
- Lọc sách theo thể loại
- Phân trang AJAX

### Khách hàng (Customer)
- Đăng ký, đăng nhập, đăng xuất
- Quản lý giỏ hàng (thêm, sửa số lượng, xóa)
- Đặt hàng và theo dõi đơn hàng
- Xem lịch sử đơn hàng

### Quản trị viên (Admin)
- Quản lý sách (CRUD)
- Quản lý thể loại (CRUD)
- Quản lý đơn hàng (xem, cập nhật trạng thái)
- Xem thống kê (doanh thu, sách bán chạy, đơn hàng)

## Cài đặt và Chạy

### Yêu cầu
- .NET SDK 9.0 trở lên

### Bước 1: Clone project
```bash
git clone <repository-url>
cd BookStoreApp
```

### Bước 2: Restore packages
```bash
dotnet restore
```

### Bước 3: Chạy ứng dụng
```bash
dotnet run
```

Ứng dụng sẽ tự động:
- Tạo database SQLite (`bookstore.db`)
- Chạy migrations
- Seed dữ liệu mẫu (thể loại, sách, tài khoản admin)

### Bước 4: Truy cập
Mở trình duyệt và truy cập: `http://localhost:5121` (hoặc port được hiển thị trong console)

## Tài khoản mặc định

### Admin
- **Email:** admin@bookstore.com
- **Password:** Admin@123

## Cấu trúc Project

```
BookStoreApp/
├── Controllers/          # Các controller
├── Models/               # Entity models
│   └── Enums/            # Enum types
├── ViewModels/           # View models
├── Data/                 # DbContext và SeedData
├── Views/                # Razor views
│   ├── Shared/           # Layout và Partial views
│   ├── Home/
│   ├── Account/
│   ├── Books/
│   ├── Categories/
│   ├── Cart/
│   ├── Orders/
│   └── Statistics/
├── wwwroot/              # Static files
│   ├── css/
│   ├── js/
│   └── lib/              # Bootstrap, jQuery
├── Program.cs            # Entry point
└── appsettings.json      # Configuration
```

## Tính năng kỹ thuật

### 1. MVC Architecture
- Controllers xử lý logic
- Models chứa dữ liệu
- Views hiển thị giao diện

### 2. Entity Framework Core
- Code-First approach
- Migrations cho version control database
- Lazy loading với virtual navigation properties

### 3. ASP.NET Core Identity
- Đăng ký, đăng nhập, đăng xuất
- Role-based authorization (Admin, Customer)
- Cookie authentication

### 4. AJAX & Partial Views
- Tìm kiếm sách realtime (debounce 300ms)
- Phân trang không reload trang
- Cập nhật giỏ hàng inline
- Toast notifications

### 5. Caching
- IMemoryCache cho danh sách thể loại, sách bán chạy
- ResponseCache cho trang public
- Cache invalidation khi CRUD

### 6. Security
- Anti-Forgery Token (CSRF protection)
- Input validation (Data Annotations)
- XSS protection (Razor auto-encoding)
- Authorization checks

## Dữ liệu mẫu
- 7 thể loại sách
- 20 cuốn sách mẫu
- 2 roles (Admin, Customer)
- 1 tài khoản admin

## Screenshots

### Trang chủ
- Banner hero
- Thể loại sách
- Sách nổi bật, mới nhất, bán chạy

### Danh sách sách
- Filter theo thể loại
- Tìm kiếm AJAX
- Phân trang AJAX
- Card hiển thị sách

### Chi tiết sách
- Thông tin đầy đủ
- Chọn số lượng
- Sách cùng thể loại

### Giỏ hàng
- Danh sách sản phẩm
- Cập nhật số lượng inline
- Tổng tiền realtime

### Checkout
- Form địa chỉ giao hàng
- Chọn phương thức thanh toán
- Tóm tắt đơn hàng

### Admin Dashboard
- Thống kê tổng quan
- Biểu đồ doanh thu
- Top sách bán chạy

---
© 2026 Nhà Sách Online

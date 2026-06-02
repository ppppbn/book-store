using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreApp.Models
{
    /// <summary>
    /// Item trong giỏ hàng
    /// </summary>
    public class CartItem
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int BookId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Display(Name = "Ngày thêm")]
        public DateTime AddedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("BookId")]
        public virtual Book? Book { get; set; }

        // Computed property - Thành tiền
        [NotMapped]
        public decimal SubTotal => Book != null ? Book.Price * Quantity : 0;
    }
}

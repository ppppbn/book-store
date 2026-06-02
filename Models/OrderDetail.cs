using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreApp.Models
{
    /// <summary>
    /// Chi tiết đơn hàng
    /// </summary>
    public class OrderDetail
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Số lượng phải từ 1 đến 1000")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Đơn giá")]
        [DisplayFormat(DataFormatString = "{0:N0} ₫")]
        public decimal UnitPrice { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("BookId")]
        public virtual Book? Book { get; set; }

        // Computed property - Thành tiền
        [NotMapped]
        public decimal SubTotal => UnitPrice * Quantity;
    }
}

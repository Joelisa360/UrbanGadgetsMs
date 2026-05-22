using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UrbanGadgets.Models
{
    public class Sale
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        public int Quantity { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.UtcNow;

        // NEW
        public string? ReceiptNumber { get; set; }
        public string? CashierName { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Profit { get; set; }
        public decimal Discount { get; set; }
    }
}
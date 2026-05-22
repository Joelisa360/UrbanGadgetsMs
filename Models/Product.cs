using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;


namespace UrbanGadgets.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BuyingPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }   // Selling Price

        public int Quantity { get; set; }

        public int ReorderLevel { get; set; } = 5;

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        //Navigation
        public List<RestockItem> RestockItems { get; set; }
    = new List<RestockItem>();
    }
}
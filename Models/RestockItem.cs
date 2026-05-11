using System.ComponentModel.DataAnnotations.Schema;

namespace UrbanGadgets.Models
{
    public class RestockItem
    {
        public int Id { get; set; }

        public int RestockReportId { get; set; }

        public RestockReport? RestockReport { get; set; }

        public int? ProductId { get; set; }

        public Product? Product { get; set; }

        // NEW → editable fields
        public string ProductName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal BuyingPrice { get; set; }

        public decimal Price { get; set; }   // selling price

        [NotMapped]
        public decimal Total => Quantity * BuyingPrice;
    }
}
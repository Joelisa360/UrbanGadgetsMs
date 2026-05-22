using UrbanGadgets.Models;

namespace UrbanGadgetsMS.Models
{
    public class StockMovement
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public Product? Product { get; set; }

        public int Quantity { get; set; }

        public string MovementType { get; set; } = string.Empty;
        // Examples:
        // "Sale"
        // "Purchase"
        // "Adjustment"
        // "Return"

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
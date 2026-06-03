using System.ComponentModel.DataAnnotations;
using UrbanGadgetsMS.Models;

namespace UrbanGadgetsMS.Models
{
    public class RestockReport
    {
        public int Id { get; set; }

        [Required]
        public string ReportNumber { get; set; } = string.Empty;

        public DateTime ReportDate { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public List<RestockItem> Items { get; set; }
            = new List<RestockItem>();

        public int? BusinessId { get; set; }
        public Business? Business { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;


namespace UrbanGadgetsMS.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        public string? ExpenseName { get; set; }

        public decimal Amount { get; set; }

       // public string Category { get; set; }   // Rent, Transport, Utilities...

        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }

        public ExpenseCategory Category { get; set; }

        public int? BusinessId { get; set; }
        public Business? Business { get; set; }
    }
}
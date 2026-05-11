using System.ComponentModel.DataAnnotations;
using UrbanGadgetsMS.Models;

namespace UrbanGadgets.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        public string ExpenseName { get; set; }

        public decimal Amount { get; set; }

       // public string Category { get; set; }   // Rent, Transport, Utilities...

        public DateTime ExpenseDate { get; set; } = DateTime.Now;

        public string Notes { get; set; }

        public ExpenseCategory Category { get; set; }
    }
}
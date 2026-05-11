using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace UrbanGadgets.Models
{
    [Index(nameof(CategoryName), IsUnique = true)]
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string CategoryName { get; set; }
    }
}
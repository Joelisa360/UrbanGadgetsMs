using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using UrbanGadgetsMS.Models;

namespace UrbanGadgetsMS.Models
{
    [Index(nameof(BusinessId), nameof(CategoryName), IsUnique = true)]
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string CategoryName { get; set; }

        public int? BusinessId { get; set; }
        public Business? Business { get; set; }
    }
}
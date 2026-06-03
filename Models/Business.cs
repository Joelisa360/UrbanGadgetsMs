using System.ComponentModel.DataAnnotations;

namespace UrbanGadgetsMS.Models
{
    public class Business
    {
        public int Id { get; set; }

        [Required]
        public string BusinessName { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
using System.ComponentModel.DataAnnotations;

namespace UrbanGadgetsMS.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Surname { get; set; }

        [Required]
        public string OtherName { get; set; }

        [Required]
        public string Contact { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string BusinessName { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
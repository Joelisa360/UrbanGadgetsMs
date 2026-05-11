using System.ComponentModel.DataAnnotations;

namespace UrbanGadgets.ViewModels
{
    public class LoginVM
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
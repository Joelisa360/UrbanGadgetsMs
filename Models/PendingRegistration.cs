using System.ComponentModel.DataAnnotations;

namespace UrbanGadgetsMS.Models
{
    public class PendingRegistration
    {
        public int Id { get; set; }

        public string Surname { get; set; }

        public string OtherName { get; set; }

        public string Contact { get; set; }

        public string Email { get; set; }

        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public string BusinessName { get; set; }

        public bool Approved { get; set; } = false;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    }
}

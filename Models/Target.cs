namespace UrbanGadgetsMS.Models
{
    public class Target
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime EndDate { get; set; }

        public bool IsAchieved { get; set; } = false;

        public string? Notes { get; set; }
    }
}
namespace UrbanGadgetsMS.Models
{
    public class OnboardingProgress
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }

        public bool CategoriesDone { get; set; }
        public bool CashiersDone { get; set; }
        public bool RestockDone { get; set; }
        public bool Completed { get; set; }
    }
}

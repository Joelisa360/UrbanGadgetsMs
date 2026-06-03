using System.ComponentModel.DataAnnotations.Schema;

namespace UrbanGadgetsMS.Models
{
    public class AppSetting
    {
        public int Id { get; set; }

        // Appearance
        public bool DarkMode { get; set; }
        public bool CompactTables { get; set; }
        public string ThemeColor { get; set; } = "Blue";

        // Notifications
        public bool LowStockAlerts { get; set; }
        public bool ExpenseAlerts { get; set; }
        public bool PopupNotifications { get; set; }

        // Security
        public bool PinLock { get; set; }
        public string? PinCode { get; set; }
        public bool RequirePinOnLogin { get; set; }
        public string AutoLogout { get; set; } = "15 Minutes";

        // Sales
        public bool AutoPrintReceipt { get; set; }
        public bool ConfirmBeforeSale { get; set; }
        public bool AllowDiscounts { get; set; }

        public decimal MonthlyExpenseLimit { get; set; }
        public decimal MonthlySalesTarget { get; set; }

        public int? BusinessId { get; set; }
        public Business? Business { get; set; }

        public bool IsOnboardingComplete { get; set; }
    }
}
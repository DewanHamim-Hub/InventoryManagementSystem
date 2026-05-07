using IMS.Models;

namespace IMS.ViewModels
{
    public class ReportsViewModel
    {
        // Filters
        public string Period { get; set; } = "Daily"; // Daily | Weekly | Monthly
        public DateTime? Date { get; set; } // for Daily

        public int? Year { get; set; }  // for Monthly
        public int? Month { get; set; } // 1-12 for Monthly

        public DateTime? WeekStart { get; set; } // for Weekly (start date)

        // Computed Date Range (display)
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        // Results (raw lists if you want to show logs)
        public List<Sale> Sales { get; set; } = new();
        public List<Restock> Restocks { get; set; } = new();

        // KPI Summary
        public int TotalUnitsSold { get; set; }
        public decimal TotalSalesValue { get; set; }
        public int TotalUnitsRestocked { get; set; }

        public int NetUnitsChange => TotalUnitsRestocked - TotalUnitsSold;

        // Simple “top products by quantity sold”
        public List<TopProductRow> TopProducts { get; set; } = new();
    }

    public class TopProductRow
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
        public decimal SalesValue { get; set; }
    }
}
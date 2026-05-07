using IMS.Models;

namespace IMS.ViewModels
{
    public class SearchViewModel
    {
        // Filters
        public string? Keyword { get; set; }
        public string? Category { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // For dropdown
        public List<string> Categories { get; set; } = new();

        // Results
        public List<Product> Products { get; set; } = new();
        public List<Sale> Sales { get; set; } = new();
        public List<Restock> Restocks { get; set; } = new();
    }
}
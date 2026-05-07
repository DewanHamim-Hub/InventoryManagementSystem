using IMS.Data;
using IMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Controllers
{
    [Authorize] // Manager & Staff
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Search
        public async Task<IActionResult> Index(string? keyword, string? category, DateTime? fromDate, DateTime? toDate)
        {
            // Normalize date range (inclusive)
            DateTime? from = fromDate?.Date;
            DateTime? to = toDate?.Date.AddDays(1).AddTicks(-1);

            // Products
            var productsQuery = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(keyword) ||
                    p.Category.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                productsQuery = productsQuery.Where(p => p.Category == category);
            }

            // Sales
            var salesQuery = _context.Sales
                .Include(s => s.Product)
                .Include(s => s.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                salesQuery = salesQuery.Where(s =>
                    s.Product != null &&
                    (s.Product.Name.Contains(keyword) || s.Product.Category.Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                salesQuery = salesQuery.Where(s => s.Product != null && s.Product.Category == category);
            }

            if (from.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate >= from.Value);
            if (to.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate <= to.Value);

            // Restocks
            var restocksQuery = _context.Restocks
                .Include(r => r.Product)
                .Include(r => r.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                restocksQuery = restocksQuery.Where(r =>
                    r.Product != null &&
                    (r.Product.Name.Contains(keyword) || r.Product.Category.Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                restocksQuery = restocksQuery.Where(r => r.Product != null && r.Product.Category == category);
            }

            if (from.HasValue) restocksQuery = restocksQuery.Where(r => r.RestockDate >= from.Value);
            if (to.HasValue) restocksQuery = restocksQuery.Where(r => r.RestockDate <= to.Value);

            // Categories for dropdown
            var categories = await _context.Products
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // ViewModel
            var vm = new SearchViewModel
            {
                Keyword = keyword,
                FromDate = fromDate,
                ToDate = toDate,
                Category = category,
                Categories = categories,
                Products = await productsQuery.OrderBy(p => p.Name).ToListAsync(),
                Sales = await salesQuery.OrderByDescending(s => s.SaleDate).ToListAsync(),
                Restocks = await restocksQuery.OrderByDescending(r => r.RestockDate).ToListAsync()
            };

            return View(vm);
        }
    }
}
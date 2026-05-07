using IMS.Data;
using IMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Reports
        public async Task<IActionResult> Index(string period = "Daily", DateTime? date = null, DateTime? weekStart = null, int? year = null, int? month = null)
        {
            var vm = new ReportsViewModel
            {
                Period = period
            };

            // Decide date range
            (DateTime from, DateTime to) = ResolveRange(period, date, weekStart, year, month);

            vm.From = from;
            vm.To = to;

            // Load sales/restocks in range
            vm.Sales = await _context.Sales
                .Include(s => s.Product)
                .Where(s => s.SaleDate >= from && s.SaleDate <= to)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();

            vm.Restocks = await _context.Restocks
                .Include(r => r.Product)
                .Where(r => r.RestockDate >= from && r.RestockDate <= to)
                .OrderByDescending(r => r.RestockDate)
                .ToListAsync();

            // KPIs
            vm.TotalUnitsSold = vm.Sales.Sum(s => s.QuantitySold);
            vm.TotalSalesValue = vm.Sales.Sum(s => s.QuantitySold * s.Product.UnitPrice);
            vm.TotalUnitsRestocked = vm.Restocks.Sum(r => r.QuantityAdded);

            // Top products (by units sold)
            vm.TopProducts = vm.Sales
                .GroupBy(s => new { s.ProductID, s.Product.Name })
                .Select(g => new TopProductRow
                {
                    ProductID = g.Key.ProductID,
                    ProductName = g.Key.Name,
                    UnitsSold = g.Sum(x => x.QuantitySold),
                    SalesValue = g.Sum(x => x.QuantitySold * x.Product.UnitPrice)
                })
                .OrderByDescending(x => x.UnitsSold)
                .Take(10)
                .ToList();

            // Keep filter values for UI
            vm.Date = date;
            vm.WeekStart = weekStart;
            vm.Year = year;
            vm.Month = month;

            return View(vm);
        }

        private static (DateTime from, DateTime to) ResolveRange(string period, DateTime? date, DateTime? weekStart, int? year, int? month)
        {
            period = (period ?? "Daily").Trim();

            if (period.Equals("Weekly", StringComparison.OrdinalIgnoreCase))
            {
                var start = (weekStart ?? DateTime.Today).Date;
                var end = start.AddDays(7).AddTicks(-1);
                return (start, end);
            }

            if (period.Equals("Monthly", StringComparison.OrdinalIgnoreCase))
            {
                var y = year ?? DateTime.Today.Year;
                var m = month ?? DateTime.Today.Month;

                var start = new DateTime(y, m, 1);
                var end = start.AddMonths(1).AddTicks(-1);
                return (start, end);
            }

            // Daily default
            var d = (date ?? DateTime.Today).Date;
            var dailyStart = d;
            var dailyEnd = d.AddDays(1).AddTicks(-1);
            return (dailyStart, dailyEnd);
        }
    }
}
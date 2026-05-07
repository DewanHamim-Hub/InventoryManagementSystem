using System.Collections.Generic;
using System.Linq;
using IMS.Data;
using IMS.Models;

namespace IMS.Services
{
    public class LowStockService
    {
        private readonly ApplicationDbContext _context;

        public LowStockService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Checks a single product and prepares low-stock notification logic
        /// </summary>
        public void CheckAndNotify(Product product)
        {
            if (product == null)
                return;

            if (product.Quantity <= product.Threshold)
            {
                // 🔔 Notification logic placeholder
                // Currently internal-only (no DB, no UI yet)

                // Example future use:
                // - Dashboard badge
                // - Manager alert panel
                // - Email / log (optional)

                // Intentionally left lightweight for now
            }
        }

        /// <summary>
        /// Returns all low-stock products (Manager use)
        /// </summary>
        public List<Product> GetLowStockProducts()
        {
            return _context.Products
                .Where(p => p.Quantity <= p.Threshold)
                .ToList();
        }
    }
}
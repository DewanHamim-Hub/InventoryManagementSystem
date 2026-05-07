using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IMS.Data;
using IMS.Models;
using IMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMS.Controllers
{
    [Authorize] // Manager & Staff
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LowStockService _lowStockService;

        public SalesController(ApplicationDbContext context, LowStockService lowStockService)
        {
            _context = context;
            _lowStockService = lowStockService;
        }

        private int? CurrentUserId()
        {
            var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idValue, out var id) ? id : null;
        }

        // GET: Sales
        public async Task<IActionResult> Index()
        {
            var sales = _context.Sales
                .Include(s => s.Product)
                .Include(s => s.User);

            return View(await sales.ToListAsync());
        }

        // GET: Sales/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _context.Sales
                .Include(s => s.Product)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.SaleID == id);

            if (sale == null) return NotFound();

            return View(sale);
        }

        // GET: Sales/Create
        public IActionResult Create()
        {
            ViewData["ProductID"] = new SelectList(_context.Products, "ProductID", "Name");
            return View();
        }

        // POST: Sales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("QuantitySold,ProductID")] Sale sale)
        {
            var userId = CurrentUserId();
            if (userId == null) return Unauthorized();

            var product = await _context.Products.FindAsync(sale.ProductID);
            if (product == null) return NotFound();

            if (sale.QuantitySold <= 0)
                ModelState.AddModelError("", "Quantity must be greater than zero.");
            else if (sale.QuantitySold > product.Quantity)
                ModelState.AddModelError("", "Insufficient stock available.");

            if (!ModelState.IsValid)
            {
                ViewData["ProductID"] = new SelectList(_context.Products, "ProductID", "Name", sale.ProductID);
                return View(sale);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                sale.UserID = userId.Value;
                sale.SaleDate = DateTime.Now;

                product.Quantity -= sale.QuantitySold;

                _context.Sales.Add(sale);
                _context.Products.Update(product);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Low-stock check
                _lowStockService.CheckAndNotify(product);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // GET: Sales/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _context.Sales.FindAsync(id);
            if (sale == null) return NotFound();

            ViewData["ProductID"] = new SelectList(_context.Products, "ProductID", "Name", sale.ProductID);
            return View(sale);
        }

        // POST: Sales/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SaleID,QuantitySold,ProductID")] Sale updatedSale)
        {
            if (id != updatedSale.SaleID) return NotFound();

            if (updatedSale.QuantitySold <= 0)
                ModelState.AddModelError("", "Quantity must be greater than zero.");

            var existingSale = await _context.Sales
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SaleID == id);

            if (existingSale == null) return NotFound();

            var product = await _context.Products.FindAsync(updatedSale.ProductID);
            if (product == null) return NotFound();

            int difference = updatedSale.QuantitySold - existingSale.QuantitySold;

            if (difference > 0 && difference > product.Quantity)
                ModelState.AddModelError("", "Insufficient stock for this update.");

            if (!ModelState.IsValid)
            {
                ViewData["ProductID"] = new SelectList(_context.Products, "ProductID", "Name", updatedSale.ProductID);
                return View(updatedSale);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                product.Quantity -= difference;

                updatedSale.UserID = existingSale.UserID;
                updatedSale.SaleDate = existingSale.SaleDate;

                _context.Products.Update(product);
                _context.Sales.Update(updatedSale);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _lowStockService.CheckAndNotify(product);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // GET: Sales/Delete/5 (Manager only)
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _context.Sales
                .Include(s => s.Product)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.SaleID == id);

            if (sale == null) return NotFound();

            return View(sale);
        }

        // POST: Sales/Delete/5 (Manager only)
        [Authorize(Roles = "Manager")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale == null) return NotFound();

            var product = await _context.Products.FindAsync(sale.ProductID);
            if (product != null)
            {
                product.Quantity += sale.QuantitySold;
                _context.Products.Update(product);

                _lowStockService.CheckAndNotify(product);
            }

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
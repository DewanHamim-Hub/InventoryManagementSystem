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
    public class RestocksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LowStockService _lowStockService;

        public RestocksController(ApplicationDbContext context, LowStockService lowStockService)
        {
            _context = context;
            _lowStockService = lowStockService;
        }

        private int? CurrentUserId()
        {
            var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idValue, out var id) ? id : null;
        }

        // GET: Restocks
        public async Task<IActionResult> Index()
        {
            var restocks = _context.Restocks
                .Include(r => r.Product)
                .Include(r => r.User);

            return View(await restocks.ToListAsync());
        }

        // GET: Restocks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var restock = await _context.Restocks
                .Include(r => r.Product)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.RestockID == id);

            if (restock == null) return NotFound();

            return View(restock);
        }

        // GET: Restocks/Create
        public IActionResult Create()
        {
            ViewData["ProductID"] = new SelectList(_context.Products, "ProductID", "Name");
            return View();
        }

        // POST: Restocks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("QuantityAdded,ProductID")] Restock restock)
        {
            var userId = CurrentUserId();
            if (userId == null) return Unauthorized();

            var product = await _context.Products.FindAsync(restock.ProductID);
            if (product == null) return NotFound();

            if (restock.QuantityAdded <= 0)
                ModelState.AddModelError("", "Quantity must be greater than zero.");

            if (!ModelState.IsValid)
            {
                ViewData["ProductID"] = new SelectList(_context.Products, "ProductID", "Name", restock.ProductID);
                return View(restock);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                restock.UserID = userId.Value;
                restock.RestockDate = DateTime.Now;

                product.Quantity += restock.QuantityAdded;

                _context.Restocks.Add(restock);
                _context.Products.Update(product);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Low-stock check (may remove low-stock status or keep it)
                _lowStockService.CheckAndNotify(product);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // GET: Restocks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var restock = await _context.Restocks.FindAsync(id);
            if (restock == null) return NotFound();

            ViewData["ProductID"] = new SelectList(_context.Products, "ProductID", "Name", restock.ProductID);
            return View(restock);
        }

        // POST: Restocks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RestockID,QuantityAdded,ProductID")] Restock updatedRestock)
        {
            if (id != updatedRestock.RestockID) return NotFound();

            if (updatedRestock.QuantityAdded <= 0)
                ModelState.AddModelError("", "Quantity must be greater than zero.");

            var existingRestock = await _context.Restocks
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RestockID == id);

            if (existingRestock == null) return NotFound();

            var product = await _context.Products.FindAsync(updatedRestock.ProductID);
            if (product == null) return NotFound();

            int difference = updatedRestock.QuantityAdded - existingRestock.QuantityAdded;

            if (!ModelState.IsValid)
            {
                ViewData["ProductID"] = new SelectList(_context.Products, "ProductID", "Name", updatedRestock.ProductID);
                return View(updatedRestock);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                product.Quantity += difference;

                updatedRestock.UserID = existingRestock.UserID;
                updatedRestock.RestockDate = existingRestock.RestockDate;

                _context.Products.Update(product);
                _context.Restocks.Update(updatedRestock);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Low-stock check (might clear warning)
                _lowStockService.CheckAndNotify(product);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // GET: Restocks/Delete/5 (Manager only)
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var restock = await _context.Restocks
                .Include(r => r.Product)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.RestockID == id);

            if (restock == null) return NotFound();

            return View(restock);
        }

        // POST: Restocks/Delete/5 (Manager only)
        [Authorize(Roles = "Manager")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var restock = await _context.Restocks.FindAsync(id);
            if (restock == null) return NotFound();

            var product = await _context.Products.FindAsync(restock.ProductID);
            if (product != null)
            {
                product.Quantity -= restock.QuantityAdded;
                _context.Products.Update(product);
            }

            _context.Restocks.Remove(restock);
            await _context.SaveChangesAsync();

            if (product != null)
            {
                // Re-evaluate low-stock after reversing a restock
                _lowStockService.CheckAndNotify(product);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
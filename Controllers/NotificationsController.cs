using IMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS.Controllers
{
    [Authorize(Roles = "Manager")]
    public class NotificationsController : Controller
    {
        private readonly LowStockService _lowStockService;

        public NotificationsController(LowStockService lowStockService)
        {
            _lowStockService = lowStockService;
        }

        // GET: /Notifications
        public IActionResult Index()
        {
            var lowStockProducts = _lowStockService.GetLowStockProducts();
            return View(lowStockProducts);
        }
    }
}
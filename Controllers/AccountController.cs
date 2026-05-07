using IMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Username and password are required.";
                return View();
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(
                userName: username,
                password: password,
                isPersistent: true,
                lockoutOnFailure: true
            );

            if (!result.Succeeded)
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            // Role-based redirect
            if (await _userManager.IsInRoleAsync(user, "Manager"))
                return RedirectToAction("Index", "Products");

            return RedirectToAction("Index", "Sales");
        }

        // GET: /Account/Logout
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
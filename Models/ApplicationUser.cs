using Microsoft.AspNetCore.Identity;

namespace IMS.Models
{
    /// <summary>
    /// Application user for ASP.NET Identity.
    /// Uses int as primary key instead of string.
    /// </summary>
    public class ApplicationUser : IdentityUser<int>
    {
        // You can add custom fields here later if required
        // Example:
        // public string FullName { get; set; } = string.Empty;
    }
}
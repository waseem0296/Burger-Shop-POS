// ============================================================
// FILE: Pages/Account/Logout.cshtml.cs
// Signs the user out and redirects to login page.
// ============================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BurgerShopPOS.Pages.Account
{
    public class LogoutModel : PageModel
    {
        // GET: Automatically log out and redirect
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Remove the login cookie from the browser
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logout error: {ex.Message}");
            }

            // Always redirect to login page after logout
            return RedirectToPage("/Account/Login");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace project1.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // If user is not authenticated, redirect to login page
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToPage("/Account/Login");
            }

            // If user is authenticated, redirect based on their role
            if (User.IsInRole("SuperAdmin"))
            {
                return RedirectToPage("/Admin/Dashboard");
            }

            // All other authenticated users go to the POS screen
            return RedirectToPage("/Sales/Index");
        }
    }
}

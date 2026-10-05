// ============================================================
// FILE: Pages/Account/Login.cshtml.cs
// This is the "brain" behind the Login page.
// It handles what happens when the user clicks "Login".
// ============================================================

using BurgerShopPOS.Data;
using BurgerShopPOS.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace BurgerShopPOS.Pages.Account
{
    public class LoginModel : PageModel
    {
        // AppDbContext lets us query the database
        private readonly AppDbContext _db;

        // Constructor: ASP.NET automatically injects the database context
        public LoginModel(AppDbContext db)
        {
            _db = db;
        }

        // [BindProperty] means this is filled from the HTML form automatically
        [BindProperty]
        public LoginViewModel Input { get; set; } = new();

        // Error message to show if login fails
        public string? ErrorMessage { get; set; }

        // GET: Just show the login page (nothing special here)
        public IActionResult OnGet()
        {
            // If already logged in, redirect away from login page
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectBasedOnRole();
            }
            return Page();
        }

        // POST: Called when the user clicks the "Login" button
        public async Task<IActionResult> OnPostAsync()
        {
            // Step 1: Check if the form data is valid (Required fields filled, etc.)
            if (!ModelState.IsValid)
            {
                return Page(); // Show the page again with validation errors
            }

            try
            {
                // Step 2: Look up the user in the database by username
                var user = _db.Users
                    .FirstOrDefault(u => u.Username == Input.Username);

                // Step 3: Check if user exists and password matches
                // NOTE: In production, use BCrypt.Net to hash/verify passwords.
                // For simplicity here, we compare plain text. Replace with:
                // BCrypt.Net.BCrypt.Verify(Input.Password, user.PasswordHash)
                if (user == null || user.PasswordHash != Input.Password)
                {
                    ErrorMessage = "Invalid username or password. Please try again.";
                    return Page();
                }

                // Step 4: Convert the role number to a readable string
                // 0 = Customer, 1 = Admin, 2 = SuperAdmin
                string roleName = user.Role switch
                {
                    0 => "Customer",
                    1 => "Admin",
                    2 => "SuperAdmin",
                    _ => "Cashier" // Default to Cashier if unknown role
                };

                // Step 5: Create "claims" — these are facts stored in the login cookie
                // Think of claims like data written on an ID card
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("FullName", user.FullName),
                    new Claim(ClaimTypes.Role, roleName)
                };

                // Step 6: Package the claims into an identity object
                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Step 7: Sign in — this creates the login cookie in the browser
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                // Step 8: Redirect based on their role
                return RedirectBasedOnRole(user.Role);
            }
            catch (Exception ex)
            {
                // If anything goes wrong (database error, etc.), show a friendly message
                ErrorMessage = $"Login failed: {ex.Message}";
                return Page();
            }
        }

        // Helper: Returns the correct page redirect based on role number
        private IActionResult RedirectBasedOnRole(int role = -1)
        {
            // If called without a role (already logged in), read from cookie
            if (role == -1)
            {
                return User.IsInRole("SuperAdmin")
                    ? RedirectToPage("/Admin/Dashboard")
                    : RedirectToPage("/Sales/Index");
            }

            return role == 2
                ? RedirectToPage("/Admin/Dashboard")
                : RedirectToPage("/Sales/Index");
        }
    }
}

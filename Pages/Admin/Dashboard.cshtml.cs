// ============================================================
// FILE: Pages/Admin/Dashboard.cshtml.cs
// The "brain" for the Super Admin dashboard page.
// Loads all analytics and provides CRUD for products/users.
// ============================================================

using BurgerShopPOS.Data;
using BurgerShopPOS.Models;
using BurgerShopPOS.Services;
using BurgerShopPOS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BurgerShopPOS.Pages.Admin
{
    // Only SuperAdmins can access this page
    [Authorize(Roles = "SuperAdmin")]
    public class DashboardModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly InvoiceService _invoiceService;

        public DashboardModel(AppDbContext db, InvoiceService invoiceService)
        {
            _db = db;
            _invoiceService = invoiceService;
        }

        // ---------- Analytics Data Properties ----------

        // Today's total revenue (since midnight)
        public decimal TodayRevenue { get; set; }

        // Number of orders today
        public int TodayOrderCount { get; set; }

        // Products with zero or low stock
        public List<Product> LowStockProducts { get; set; } = new();

        // Top-selling products today (by quantity sold)
        public List<TopProductItem> TopProducts { get; set; } = new();

        // Products expiring within 48 hours
        public List<Product> ExpiringProducts { get; set; } = new();

        // All products (for the menu management table)
        public List<Product> AllProducts { get; set; } = new();

        // All users (for user management)
        public List<User> AllUsers { get; set; } = new();

        // All orders for viewing/downloading invoices
        public List<Order> AllOrders { get; set; } = new();

        // Form for adding/editing products
        [BindProperty]
        public ProductFormViewModel ProductForm { get; set; } = new();

        // ---------- GET: Load the dashboard page ----------
        public async Task OnGetAsync()
        {
            try
            {
                // The start of today (midnight)
                DateTime todayStart = DateTime.Today;

                // 1. Today's revenue: sum TotalAmount for orders since midnight
                TodayRevenue = await _db.Orders
                    .Where(o => o.OrderDate >= todayStart)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

                // 2. Today's order count
                TodayOrderCount = await _db.Orders
                    .Where(o => o.OrderDate >= todayStart)
                    .CountAsync();

                // 3. Low stock: items with fewer than 10 in stock
                LowStockProducts = await _db.Products
                    .Where(p => p.StockQuantity < 10)
                    .OrderBy(p => p.StockQuantity)
                    .ToListAsync();

                // 4. Top products today: group order details by product, sum quantities
                TopProducts = await _db.OrderDetails
                    .Include(od => od.Order)
                    .Include(od => od.Product)
                    .Where(od => od.Order!.OrderDate >= todayStart)
                    .GroupBy(od => new { od.ProductId, od.Product!.ItemName })
                    .Select(g => new TopProductItem
                    {
                        ProductId   = g.Key.ProductId,
                        ItemName    = g.Key.ItemName,
                        TotalSold   = g.Sum(od => od.Quantity),
                        TotalRevenue = g.Sum(od => od.SubTotal)
                    })
                    .OrderByDescending(t => t.TotalSold)
                    .Take(5) // Top 5 only
                    .ToListAsync();

                // 5. Expiring products: expiry date within the next 48 hours
                DateTime in48Hours = DateTime.Now.AddHours(48);
                ExpiringProducts = await _db.Products
                    .Where(p => p.ExpiryDate <= in48Hours && p.ExpiryDate >= DateTime.Now)
                    .OrderBy(p => p.ExpiryDate)
                    .ToListAsync();

                // 6. All products for CRUD management
                AllProducts = await _db.Products
                    .OrderBy(p => p.Category)
                    .ThenBy(p => p.ItemName)
                    .ToListAsync();

                // 7. All users for user management
                AllUsers = await _db.Users
                    .OrderBy(u => u.Role)
                    .ToListAsync();

                // 8. All orders with customer info for invoice download
                AllOrders = await _db.Orders
                    .Include(o => o.Customer)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dashboard load error: {ex.Message}");
                // Data stays empty — page still shows with no results
            }
        }

        // ---------- POST: Save a new product or update existing ----------
        public async Task<IActionResult> OnPostSaveProductAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // Reload dashboard data
                return Page();
            }

            try
            {
                if (ProductForm.ProductId == 0)
                {
                    // ProductId = 0 means it's a new product (Add)
                    var newProduct = new Product
                    {
                        ItemName      = ProductForm.ItemName,
                        Category      = ProductForm.Category,
                        Price         = ProductForm.Price,
                        StockQuantity = ProductForm.StockQuantity,
                        ExpiryDate    = ProductForm.ExpiryDate
                    };
                    _db.Products.Add(newProduct);
                }
                else
                {
                    // Existing product — find and update it
                    var existing = await _db.Products.FindAsync(ProductForm.ProductId);
                    if (existing != null)
                    {
                        existing.ItemName      = ProductForm.ItemName;
                        existing.Category      = ProductForm.Category;
                        existing.Price         = ProductForm.Price;
                        existing.StockQuantity = ProductForm.StockQuantity;
                        existing.ExpiryDate    = ProductForm.ExpiryDate;
                    }
                }

                await _db.SaveChangesAsync();
                TempData["Success"] = "Product saved successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error saving product: {ex.Message}";
            }

            return RedirectToPage(); // Reload the page
        }

        // ---------- POST: Delete a product ----------
        public async Task<IActionResult> OnPostDeleteProductAsync(int id)
        {
            try
            {
                var product = await _db.Products.FindAsync(id);
                if (product != null)
                {
                    _db.Products.Remove(product);
                    await _db.SaveChangesAsync();
                    TempData["Success"] = $"'{product.ItemName}' deleted.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Cannot delete: {ex.Message}";
            }

            return RedirectToPage();
        }

        // ---------- POST: Delete a user ----------
        public async Task<IActionResult> OnPostDeleteUserAsync(int id)
        {
            try
            {
                var user = await _db.Users.FindAsync(id);
                if (user != null)
                {
                    _db.Users.Remove(user);
                    await _db.SaveChangesAsync();
                    TempData["Success"] = $"User '{user.Username}' deleted.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Cannot delete user: {ex.Message}";
            }

            return RedirectToPage();
        }

        // ---------- GET: Download invoice PDF ----------
        public async Task<IActionResult> OnGetDownloadInvoiceAsync(int orderId)
        {
            try
            {
                // Find the order
                var order = await _db.Orders
                    .Include(o => o.Customer)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    return NotFound("Order not found.");
                }

                // Check if invoice path exists
                if (string.IsNullOrEmpty(order.InvoicePath))
                {
                    return BadRequest("Invoice not available for this order.");
                }

                // Get absolute path
                string invoicePath = _invoiceService.GetInvoiceAbsolutePath(order.InvoicePath);

                // Check if file exists
                if (!System.IO.File.Exists(invoicePath))
                {
                    return NotFound("Invoice file not found.");
                }

                // Read file and return as download
                var fileBytes = System.IO.File.ReadAllBytes(invoicePath);
                string fileName = $"Invoice_{orderId:D6}.pdf";

                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error downloading invoice: {ex.Message}");
            }
        }
    }
}

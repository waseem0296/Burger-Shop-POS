// ============================================================
// FILE: Pages/Sales/Index.cshtml.cs
// The "brain" for the POS customer screen.
// Handles: loading products, search, and processing checkout.
// ============================================================

using BurgerShopPOS.Data;
using BurgerShopPOS.Models;
using BurgerShopPOS.Services;
using BurgerShopPOS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace BurgerShopPOS.Pages.Sales
{
    // Only logged-in users can access this page
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly InvoiceService _invoiceService;

        public IndexModel(AppDbContext db, InvoiceService invoiceService)
        {
            _db = db;
            _invoiceService = invoiceService;
        }

        // All products to show in the food grid
        public List<Product> Products { get; set; } = new();

        // The last completed order (shown in success receipt panel)
        public OrderSummaryViewModel? LastOrder { get; set; }

        // GET: Load the page and fetch all products from DB
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Load all products from database, ordered by category then name
                Products = await _db.Products
                    .OrderBy(p => p.Category)
                    .ThenBy(p => p.ItemName)
                    .ToListAsync();

                return Page();
            }
            catch (Exception ex)
            {
                // If DB load fails, show page with empty product list
                Console.WriteLine($"Error loading products: {ex.Message}");
                Products = new List<Product>();
                return Page();
            }
        }

        // GET AJAX: Search products by name or category
        // Called from JavaScript: /Sales/Index?handler=Search&query=burger
        public async Task<IActionResult> OnGetSearchAsync(string query)
        {
            try
            {
                // If search is empty, return all products
                if (string.IsNullOrWhiteSpace(query))
                {
                    var all = await _db.Products.ToListAsync();
                    return new JsonResult(all);
                }

                // Filter products by name or category (case-insensitive)
                string q = query.ToLower();
                var results = await _db.Products
                    .Where(p => p.ItemName.ToLower().Contains(q)
                             || p.Category.ToLower().Contains(q))
                    .ToListAsync();

                return new JsonResult(results);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }
        }

        // POST: Process checkout when customer clicks "Complete Order"
        // Receives the cart as a JSON string from JavaScript
        public async Task<IActionResult> OnPostCheckoutAsync([FromBody] List<CartItemViewModel> cartItems)
        {
            try
            {
                // Validate: cart must not be empty
                if (cartItems == null || cartItems.Count == 0)
                {
                    return new JsonResult(new { success = false, message = "Cart is empty." });
                }

                // Get the customer's user ID from their login cookie
                string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdStr, out int customerId))
                {
                    return new JsonResult(new { success = false, message = "Session expired. Please log in again." });
                }

                // ---------- Use a transaction: all DB changes succeed or all fail together ----------
                using var transaction = await _db.Database.BeginTransactionAsync();

                try
                {
                    decimal orderTotal = 0;
                    var orderDetails = new List<OrderDetail>();

                    // Loop through each item in the cart
                    foreach (var cartItem in cartItems)
                    {
                        // Fetch the product from DB to verify and update stock
                        var product = await _db.Products.FindAsync(cartItem.ProductId);

                        if (product == null)
                        {
                            return new JsonResult(new
                            {
                                success = false,
                                message = $"Product ID {cartItem.ProductId} not found."
                            });
                        }

                        // Check if enough stock is available
                        if (product.StockQuantity < cartItem.Quantity)
                        {
                            return new JsonResult(new
                            {
                                success = false,
                                message = $"Not enough stock for '{product.ItemName}'. Available: {product.StockQuantity}"
                            });
                        }

                        // Deduct stock quantity
                        product.StockQuantity -= cartItem.Quantity;

                        // Calculate the subtotal for this line item
                        decimal subTotal = product.Price * cartItem.Quantity;
                        orderTotal += subTotal;

                        // Create an order detail record for this item
                        orderDetails.Add(new OrderDetail
                        {
                            ProductId = product.ProductId,
                            Quantity = cartItem.Quantity,
                            UnitPrice = product.Price,
                            SubTotal = subTotal
                        });
                    }

                    // Create the main order record
                    var newOrder = new Order
                    {
                        OrderDate = DateTime.Now,
                        TotalAmount = orderTotal,
                        CustomerId = customerId,
                        InvoicePath = string.Empty // Will set after generating invoice
                    };

                    _db.Orders.Add(newOrder);

                    // Save to DB to get the new OrderId
                    await _db.SaveChangesAsync();

                    // Link each order detail to the new order
                    foreach (var detail in orderDetails)
                    {
                        detail.OrderId = newOrder.OrderId;
                    }

                    _db.OrderDetails.AddRange(orderDetails);

                    // Save all changes (stock updates + order details)
                    await _db.SaveChangesAsync();

                    // Build order summary for the PDF invoice
                    string customerName = User.FindFirstValue("FullName") ?? "Customer";
                    var summary = new OrderSummaryViewModel
                    {
                        OrderId = newOrder.OrderId,
                        OrderDate = newOrder.OrderDate,
                        TotalAmount = orderTotal,
                        CashierName = customerName, // Keep for compatibility
                        Items = cartItems
                    };

                    // Generate the PDF invoice
                    string invoicePath = _invoiceService.GenerateInvoice(summary, customerName);

                    // Save invoice path to order record
                    newOrder.InvoicePath = invoicePath;
                    _db.Orders.Update(newOrder);
                    await _db.SaveChangesAsync();

                    // Commit transaction — everything worked!
                    await transaction.CommitAsync();

                    return new JsonResult(new
                    {
                        success = true,
                        orderId = newOrder.OrderId,
                        total = orderTotal,
                        invoicePath = invoicePath,
                        message = "Order completed successfully!"
                    });
                }
                catch (Exception innerEx)
                {
                    // If anything in the transaction fails, undo all changes
                    await transaction.RollbackAsync();
                    return new JsonResult(new { success = false, message = $"Order failed: {innerEx.Message}" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // GET: Download invoice PDF
        public async Task<IActionResult> OnGetDownloadInvoiceAsync(int orderId)
        {
            try
            {
                // Find the order
                var order = await _db.Orders.FindAsync(orderId);
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

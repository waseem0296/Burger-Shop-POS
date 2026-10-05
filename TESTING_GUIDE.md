# 🍔 Burger Shop POS - Complete Testing Guide

## ✅ Implementation Complete

Your Burger Shop POS application is now fully configured and running with:
- ✅ Authentication & Authorization (Cookie-based)
- ✅ Role-Based Access Control (SuperAdmin, Cashier)
- ✅ Database Seeding with Test Data
- ✅ Session Management for Cart
- ✅ Complete Checkout Workflow with Inventory Management
- ✅ Admin Dashboard with Analytics
- ✅ Invoice Generation

---

## 🚀 Quick Start - Test the Application

### Step 1: Access the Application
Open your browser and navigate to:
```
http://localhost:5289
```

**Expected:** You will be redirected to `/Account/Login` (authenticated users go to their role-specific dashboard)

---

## 🔐 Test Accounts

Use these credentials to test different roles:

### SuperAdmin Account
```
Username: admin
Password: admin123
```
**Access:** Admin Dashboard (/Admin/Dashboard) - Full analytics, product CRUD, user management

### Cashier Accounts
```
Username: cashier1
Password: cashier123
```
OR
```
Username: cashier2
Password: cashier123
```
**Access:** POS Sales Screen (/Sales/Index) - Browse products, manage cart, checkout

---

## 📋 End-to-End Workflow Test

### Test 1: Cashier Login & Product Browsing
1. Go to http://localhost:5289
2. Enter: `Username: cashier1` and `Password: cashier123`
3. Click **Login**
4. **Expected Result:** Redirected to `/Sales/Index` (POS Screen)
5. **Verify:** Product grid displays with categories: Burgers, Sides, Drinks
6. **Sample Products:**
   - Classic Cheeseburger ($5.99) - 50 in stock
   - French Fries ($2.99) - 100 in stock
   - Soft Drink ($1.99) - 200 in stock

---

### Test 2: Add Items to Cart
1. Click on "Classic Cheeseburger" product button
2. Enter quantity (e.g., 2)
3. Click "Add to Cart"
4. **Expected Result:** Item appears in cart table below
5. Repeat for other items (French Fries, Soft Drink)
6. **Verify Cart Shows:**
   - Product Name
   - Price
   - Quantity
   - Subtotal (Price × Quantity)

---

### Test 3: Checkout Process
1. Review all items in cart
2. See **Total Bill Amount** calculated correctly
3. Click "Complete Order" button
4. **Expected Result:** 
   - JSON response shows success
   - Cart clears
   - "Order completed successfully!" message appears
   - Receipt/Invoice details display

---

### Test 4: Inventory Deduction
1. After checkout, note the quantities purchased
2. Logout (click "Logout")
3. Login again as cashier1
4. **Expected Result:** Product stock quantities are reduced by purchased amounts
   - Classic Cheeseburger: 50 - 2 = **48** in stock
   - French Fries: 100 - 1 = **99** in stock
   - Soft Drink: 200 - 1 = **199** in stock

---

### Test 5: Admin Dashboard Access
1. Logout (click "Logout")
2. Login with: `Username: admin` and `Password: admin123`
3. **Expected Result:** Redirected to `/Admin/Dashboard`
4. **Dashboard Shows:**
   - **Today's Revenue:** Total amount from all orders today
   - **Today's Order Count:** Number of orders placed
   - **Top Products:** Best-selling items with quantities and revenue
   - **Low Stock Alerts:** Products with < 10 items
   - **Expiring Products:** Items expiring within 48 hours
   - **All Products Table:** Full inventory CRUD management

---

### Test 6: Logout Functionality
1. Click "Logout" button
2. **Expected Result:** 
   - Authentication cookie removed
   - Redirected to `/Account/Login`
   - Previous session cleared
   - Must login again to access protected pages

---

## 🔒 Security Tests

### Test 7: Unauthorized Access Prevention
1. Logout completely
2. Try to access sales page directly: `http://localhost:5289/Sales/Index`
3. **Expected Result:** Redirected to `/Account/Login`

---

### Test 8: Role-Based Access Control
1. Login as cashier1
2. Try to access admin page: `http://localhost:5289/Admin/Dashboard`
3. **Expected Result:** Access denied or redirected to sales page

---

### Test 9: Invalid Credentials
1. Go to login page
2. Enter: `Username: admin` and `Password: wrongpassword`
3. Click **Login**
4. **Expected Result:** Error message: "Invalid username or password. Please try again."

---

## 📊 Analytics & Data Integrity

### Test 10: Revenue Tracking
1. Cashier makes multiple orders with different items
2. Admin views dashboard
3. **Expected Result:** 
   - TodayRevenue = sum of all order totals
   - Order details match placed orders

### Test 11: Stock Control
1. Check product stock before checkout
2. Process checkout with multiple items
3. Check admin dashboard inventory
4. **Expected Result:** Stock quantities decrease by purchased amounts

---

## 🗄️ Database-Backed Persistence

All data is stored in **SQL Server (BurgerShopDB)**:

### Tables Created
- **Users** - Login credentials, roles (0=Cashier, 1=Admin, 2=SuperAdmin)
- **Products** - Menu items with price, stock, expiry date
- **Orders** - Transaction records with total amount and cashier ID
- **OrderDetails** - Line items for each order (product, quantity, price)

### Seed Data Included
- **3 Users:** admin, cashier1, cashier2
- **9 Products:** 3 Burgers, 3 Sides, 3 Drinks
- All products have realistic pricing and stock levels

---

## 🛠️ Troubleshooting

### Issue: Login Not Working
**Solution:** 
- Verify database connection string in `appsettings.Development.json`
- Check SQL Server is running
- Confirm users exist in database before running application

### Issue: Products Not Displaying
**Solution:**
- Ensure products table is seeded
- Check database connection
- Verify EF Core migrations applied

### Issue: Checkout Error
**Solution:**
- Verify inventory levels > 0
- Check database transaction support
- Review error logs in Output window

### Issue: Admin Dashboard Shows No Data
**Solution:**
- Ensure user is SuperAdmin role (Role = 2)
- Verify orders exist in database
- Check date filters (today's orders only)

---

## 📝 Requirements Checklist

- ✅ **Authentication:** Login redirects unauthenticated users to /Account/Login
- ✅ **Authorization:** [Authorize] attributes protect pages
- ✅ **Role-Based Access:** [Authorize(Roles="SuperAdmin")] restricts admin pages
- ✅ **Model Validation:** [Required], [DataType], [Range] attributes validate input
- ✅ **Interactive Grid:** Cart table with inline quantity editing
- ✅ **Real-time Calculation:** JavaScript updates totals without page refresh
- ✅ **Database Synchronization:** Orders saved with EF Core transactions
- ✅ **Stock Control:** Inventory decremented after checkout
- ✅ **Invoice Generation:** Receipts created and saved
- ✅ **Analytics:** Daily revenue, top products, low stock alerts
- ✅ **State Management:** Session used for cart operations
- ✅ **Dependency Injection:** DbContext injected into PageModels

---

## 🎓 Key Concepts Review (For Viva)

### 1. **Dependency Injection**
```csharp
public class LoginModel : PageModel
{
	private readonly AppDbContext _db;

	public LoginModel(AppDbContext db)  // Injected automatically
	{
		_db = db;
	}
}
```
**Why?** Loose coupling, testability, automatic lifecycle management

### 2. **Model Binding**
```html
<input asp-for="Input.Username" />  <!-- Automatically binds to LoginViewModel.Username -->
```
**How?** ASP.NET Core maps form data to PageModel properties

### 3. **State Management**
```csharp
HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cartItems));
```
**Purpose:** Persist cart data across requests without database (temporary)

### 4. **Claims-Based Authorization**
```csharp
var claims = new List<Claim>
{
	new Claim(ClaimTypes.Role, "cashier"),
	new Claim(ClaimTypes.Name, "John")
};
```
**Purpose:** Encode user information in authentication cookie

---

## 🚀 Production Ready Improvements

When deploying, consider:
1. **Password Hashing:** Replace plain text with BCrypt
   ```csharp
   var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
   ```

2. **Connection String Security:** Move to Azure Key Vault or environment variables

3. **HTTPS Enforcement:** Already enabled in Startup

4. **Antiforgery Tokens:** Already included in forms: `@Html.AntiForgeryToken()`

5. **CORS:** Configure if APIs called from different domain

6. **Logging:** Implement Serilog for comprehensive logging

---

## 📞 Support Notes

- **Framework:** ASP.NET Core 10 with Razor Pages
- **Database:** SQL Server (LocalDB or Express)
- **ORM:** Entity Framework Core 10
- **Authentication:** Cookie-based with Claims
- **Session:** In-memory (configure Redis for scalability)
- **UI:** Bootstrap 5 + vanilla JavaScript

---

**Prepared for Viva Presentation & Production Deployment** ✅

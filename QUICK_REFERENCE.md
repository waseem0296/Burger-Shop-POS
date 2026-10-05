# 🍔 Burger Shop POS - Quick Reference Card

## 🎯 Current Status
✅ **RUNNING** on `http://localhost:5289`

---

## 👤 Test Credentials

### Admin Account
```
Username: admin
Password: admin123
→ Access: Admin Dashboard (/Admin/Dashboard)
```

### Cashier Account
```
Username: cashier1  (or cashier2)
Password: cashier123
→ Access: POS Sales Screen (/Sales/Index)
```

---

## 🔐 Workflow Overview

```
┌─────────────────┐
│   Login Page    │
│   /Account/     │
│    Login        │
└────────┬────────┘
		 │
	┌────▼────┐
	│ Validate│
	│ Creds   │
	└────┬────┘
		 │
	┌────▼──────────────────┐
	│ Check User Role       │
	│ Create Auth Cookie    │
	└────┬─────────┬────────┘
		 │         │
	┌────▼──┐  ┌───▼────────────────┐
	│Cashier│  │  SuperAdmin        │
	│       │  │                    │
	│/Sales/│  │ /Admin/            │
	│Index  │  │ Dashboard          │
	└────┬──┘  └───┬────────────────┘
		 │         │
		 │    ┌────▼─────────────┐
		 │    │ Analytics       │
		 │    │ Inventory       │
		 │    │ Revenue Reports │
		 │    └────────────────┘
		 │
	┌────▼──────────────────┐
	│ Browse Products       │
	│ Add to Cart (Session) │
	│ Review Shopping Cart  │
	└────┬─────────────────┘
		 │
	┌────▼──────────────────┐
	│ Checkout             │
	│ - Validate inventory │
	│ - Create orders      │
	│ - Deduct stock (DB)  │
	│ - Generate invoice   │
	└────┬─────────────────┘
		 │
	┌────▼──────────────────┐
	│  Order Confirmation  │
	│  & Receipt Display   │
	└────┬─────────────────┘
		 │
	┌────▼──────────────────┐
	│  Logout              │
	│  /Account/Logout     │
	└──────────────────────┘
```

---

## 📱 URLs to Test

| Feature | URL |
|---------|-----|
| Home (Auto-redirect) | `http://localhost:5289/` |
| Login | `http://localhost:5289/Account/Login` |
| Logout | `http://localhost:5289/Account/Logout` |
| POS Sales | `http://localhost:5289/Sales/Index` |
| Admin Dashboard | `http://localhost:5289/Admin/Dashboard` |
| Error | `http://localhost:5289/Error` |

---

## 🗄️ Database

**Server:** WIN-GK0KIT4BA43\SQLEXPRESS  
**Database:** BurgerShopPOS  
**Connection:** Trusted_Connection=True

### Tables
- `Users` (3 seed records)
- `Products` (9 seed records)
- `Orders` (grows with each transaction)
- `OrderDetails` (line items)

---

## 🔑 Key Files Modified

| File | Purpose |
|------|---------|
| `Program.cs` | Added session, authentication, InvoiceService |
| `Data/AppDbContext.cs` | Added seed data (users & products) |
| `ViewModels/ViewModels.cs` | Added TopProductItem ViewModel |
| `Pages/Account/Login.cshtml.cs` | Cookie-based authentication |
| `Pages/Admin/Dashboard.cshtml.cs` | Analytics with [Authorize(SuperAdmin)] |
| `Pages/Sales/Index.cshtml.cs` | POS workflow with transactions |

---

## 🎯 Test Checklist

- [ ] Login with cashier1/cashier123
- [ ] See POS screen with products
- [ ] Add items to cart
- [ ] Review cart with correct totals
- [ ] Checkout successfully
- [ ] See inventory reduced
- [ ] Logout
- [ ] Login with admin/admin123
- [ ] View admin dashboard
- [ ] See today's revenue & top products
- [ ] Try accessing sales page as admin (should deny)
- [ ] Try accessing admin page as cashier (should deny)

---

## ⚙️ Configuration

**Session:** 30-minute timeout  
**Auth Cookie:** 7-day expiration with sliding refresh  
**HTTPS:** Required (enforced)  
**Static Files:** Enabled  
**HSTS:** Enabled  

---

## 🆘 Quick Fixes

### Database Connection Issues
```bash
# Verify connection in appsettings.Development.json
# Format: Server=name;Database=BurgerShopPOS;Trusted_Connection=True;
```

### Port Already in Use
```bash
# Change in Properties/launchSettings.json
"applicationUrl": "http://localhost:5290"  # Use different port
```

### Rebuild Database
```bash
# In Package Manager Console
Update-Database -Force
```

---

## 📊 Sample Test Order

1. Login as `cashier1` / `cashier123`
2. Add to Cart:
   - 2x Classic Cheeseburger ($5.99 each = $11.98)
   - 1x French Fries ($2.99)
   - 3x Soft Drink ($1.99 each = $5.97)
3. **Total:** $20.94
4. Checkout
5. Verify inventory reduced in admin dashboard

---

## 🎓 Architecture Summary

```
┌─────────────────────────────────────────────┐
│          ASP.NET Core Web App               │
├─────────────────────────────────────────────┤
│  Razor Pages (UI Layer)                     │
│  ├─ Pages/Account/Login.cshtml              │
│  ├─ Pages/Sales/Index.cshtml                │
│  └─ Pages/Admin/Dashboard.cshtml            │
├─────────────────────────────────────────────┤
│  PageModels (Business Logic)                │
│  ├─ LoginModel (Authentication)             │
│  ├─ SalesModel (Checkout, Inventory)        │
│  └─ DashboardModel (Analytics)              │
├─────────────────────────────────────────────┤
│  Services (Application Logic)               │
│  └─ InvoiceService (PDF/Text generation)    │
├─────────────────────────────────────────────┤
│  Data Layer (EF Core)                       │
│  ├─ AppDbContext                            │
│  └─ Models (User, Product, Order, etc.)     │
├─────────────────────────────────────────────┤
│  SQL Server Database                        │
│  └─ BurgerShopPOS (4 tables)                │
└─────────────────────────────────────────────┘
```

---

**Created:** After fixing authentication  
**Status:** ✅ Ready for demonstration  
**Last Update:** Today

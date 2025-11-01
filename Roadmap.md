# 🗺️ Roadmap - EVM Dealer Portal (Demo Version)

> Lộ trình phát triển hệ thống quản lý đại lý xe điện - **Dự án demo sinh viên**
> Tập trung vào UI đẹp và các chức năng core nhất theo Requirements

## 📊 Tổng quan hiện trạng

### ✅ Đã hoàn thành (Current State - UI-First Prototype)
- [x] Cấu trúc dự án ASP.NET Core 8.0 (Razor Pages)
- [x] Entity Framework Core với SQL Server
- [x] Database schema đầy đủ (15 tables với seed data)
- [x] Session-based Authentication (prototype)
- [x] Role-based Access Control (5 roles: CUSTOMER, DEALER_STAFF, DEALER_MANAGER, EVM_STAFF, EVM_ADMIN)
- [x] Dark Theme CSS với CSS variables (`dark-theme.css`)
- [x] Layout system (_Layout, _LayoutPublic, _PageHeader partial)
- [x] Shared components (_StatCard, _StatusBadge)
- [x] **5 Dashboards** (mỗi role có dashboard riêng)
- [x] **Dealer Staff UI:** Vehicles catalog, Customers, TestDrives, Feedback, Sales (Quotes list, Create Quote, Orders list)
- [x] **Dealer Manager UI:** Dashboard, Reports (SalesByStaff, Debts), Back button navigation
- [x] **EVM Staff UI:** Vehicles management (Index, Create), PricePolicies, Stocks, Dealers, DealerOrders, DealerOrderDetail
- [x] **EVM Admin UI:** Dashboard, Reports (SalesByDealer, SalesByVehicle, Inventory, Consumption), Users management
- [x] **Customer UI:** Vehicles catalog, MyQuotes, MyOrders, TestDrive booking
- [x] Public pages: Home, Login, Register, Profile

### ✅ Vừa hoàn thành (Latest - UI-First)
- [x] Detail Pages: OrderDetail (Dealer & Customer), QuoteDetail (Dealer) - Đọc dữ liệu thật từ DB
- [x] Payment Management: Add Payment form với validation, auto update order status
- [x] Delivery Management: Schedule Delivery form, Mark Delivered functionality
- [x] Convert Quote to Order: Functional POST handler, copy all lines, redirect to OrderDetail
- [x] Vehicle Detail Pages: Dealer & Customer - Specs, pricing, stock, actions buttons

### ❌ Chưa có (Optional - UI-First)
- [ ] Create Order page (hiện có Convert Quote to Order, có thể không cần separate page)
- [ ] Vehicle Comparison feature (nice to have)
- [ ] Promotion management UI (apply promotions to quotes/orders) - Form hiện đã có dropdown
- [ ] Edit Quote functionality (link đã có nhưng chưa implement)

---

## 🎯 Timeline rút gọn: **5-6 tuần**

### ⚡ Phase 1: Foundation (Week 1) - Database + Auth
### 🎨 Phase 2: Core Features (Week 2) - Business Logic + Seed Data
### 💼 Phase 3: Dealer Staff UI (Week 3) - Trang chính của hệ thống
### 🏭 Phase 4: EVM Staff UI (Week 3-4) - Quản lý từ hãng xe
### 👑 Phase 4b: EVM Admin UI (Week 4) - Dashboard & Reports
### 👤 Phase 5: Customer Portal + Polish (Week 4-5) - Hoàn thiện & Demo

---

## ⚡ Phase 1: Foundation (Week 1)

**Mục tiêu:** Database đầy đủ + Authentication cơ bản

### 1.1 Database Schema (15 tables)

#### A. Auth & Tổ chức (3 tables)
- [ ] **1.1.1** Model `Role`
  - `Id`, `Code` (UNIQUE), `Name`, `IsOperational`
  
- [ ] **1.1.2** Model `User`
  - `Id`, `Email` (UNIQUE), `PasswordHash`, `FullName`, `Phone`, `RoleId` (FK), `DealerId` (FK, nullable), `CreatedAt`
  
- [ ] **1.1.3** Cập nhật `Dealer`
  - Thêm: `Code` (UNIQUE), `Status`

#### B. Sản phẩm & Phân phối (4 tables)
- [ ] **1.1.4** Refactor `Vehicle`
  - `ModelName`, `VariantName`, `SpecJson`, `ImageUrl`, `Status`
  - UNIQUE(`ModelName`, `VariantName`)
  
- [ ] **1.1.5** Model `PricePolicy`
  - `VehicleId`, `DealerId` (nullable), `Msrp`, `WholesalePrice`, `DiscountRuleJson`, `ValidFrom`, `ValidTo`
  
- [ ] **1.1.6** Model `Stock`
  - `OwnerType` (EVM/DEALER), `OwnerId`, `VehicleId`, `ColorCode`, `Qty`
  
- [ ] **1.1.7** Model `DealerOrder`
  - `DealerId`, `Status`, `ItemsJson`, `CreatedBy`, `ApprovedBy`, timestamps

#### C. Bán hàng (5 tables)
- [ ] **1.1.8** Model `SalesDocument`
  - `Type` (QUOTE/ORDER/CONTRACT), `DealerId`, `CustomerId`, `Status`, `PromotionId`, `SignedAt`
  
- [ ] **1.1.9** Model `SalesDocumentLine`
  - `SalesDocumentId`, `VehicleId`, `ColorCode`, `Qty`, `UnitPrice`, `DiscountValue`
  
- [ ] **1.1.10** Model `Payment`
  - `SalesDocumentId`, `Method` (CASH/FINANCE), `Amount`, `MetaJson`, `PaidAt`
  
- [ ] **1.1.11** Model `Delivery`
  - `SalesDocumentId`, `ScheduledDate`, `DeliveredDate`, `Status`, `HandoverNote`
  
- [ ] **1.1.12** Model `Promotion`
  - `Name`, `Scope` (GLOBAL/DEALER/VEHICLE), `DealerId`, `VehicleId`, `RuleJson`, `ValidFrom`, `ValidTo`

#### D. Khách hàng (3 tables)
- [ ] **1.1.13** Refactor `Customer` → `CustomerProfile`
  - `UserId` (nullable, UNIQUE), `FullName`, `Phone` (UNIQUE), `Email` (UNIQUE), `Address`, `IdentityNo`
  
- [ ] **1.1.14** Model `TestDrive`
  - `CustomerId`, `DealerId`, `VehicleId`, `ScheduleTime`, `Status`, `Note`
  
- [ ] **1.1.15** Model `Feedback`
  - `CustomerId`, `DealerId`, `Type` (FEEDBACK/COMPLAINT), `Status`, `Content`

- [ ] **1.1.16** Migration mới + Seed Roles

### 1.2 Authentication & Authorization

- [ ] **1.2.1** Cài đặt `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- [ ] **1.2.2** Tích hợp Identity vào `ApplicationDbContext`
- [ ] **1.2.3** Tạo Login/Register pages (UI cơ bản)
- [ ] **1.2.4** Role-based Authorization:
  - `[Authorize(Roles = "DEALER_STAFF")]` cho các trang dealer
  - `[Authorize(Roles = "EVM_STAFF")]` cho các trang EVM
  - Customer pages có thể public hoặc require CUSTOMER role
- [ ] **1.2.5** Seed admin account: `admin@evm.com` / password

---

## 🎨 Phase 2: Core Features (Week 2)

**Mục tiêu:** Services + Business Logic cơ bản (không quá phức tạp)

### 2.1 Repository Layer
- [ ] **2.1.1** Tạo repositories cho tất cả 15 entities (theo pattern hiện có)

### 2.2 Service Layer (Logic cơ bản, không quá strict)

- [ ] **2.2.1** `PricePolicyService`
  - Get active price by vehicle + dealer
  - Apply price rules (đơn giản: MSRP hoặc Wholesale)
  
- [ ] **2.2.2** `StockService`
  - Check availability
  - Update qty (increase/decrease)
  
- [ ] **2.2.3** `SalesDocumentService` ⭐ **QUAN TRỌNG**
  - Create Quote → convert to Order
  - Calculate totals (đơn giản: sum line items - discount)
  - Update status cơ bản
  
- [ ] **2.2.4** `PaymentService`
  - Record payment
  - Calculate total paid
  - Auto update order status khi đủ tiền
  
- [ ] **2.2.5** `DeliveryService`
  - Schedule delivery
  - Mark delivered → deduct stock
  
- [ ] **2.2.6** `PromotionService`
  - Get active promotions (simple date check)
  - Apply promotion discount
  
- [ ] **2.2.7** `DealerOrderService`
  - Create order, approve/reject, fulfill

- [ ] **2.2.8** `TestDriveService` & `FeedbackService`
  - CRUD cơ bản

### 2.3 Seed Data (Quan trọng cho demo!)

- [ ] **2.3.1** Seed 1-2 Dealers
- [ ] **2.3.2** Seed 5-10 Vehicles (với images, specs)
- [ ] **2.3.3** Seed Price Policies
- [ ] **2.3.4** Seed Stocks (EVM và Dealer)
- [ ] **2.3.5** Seed test accounts:
  - Dealer Staff: `dealer1@test.com`
  - EVM Staff: `evm@test.com`
  - Customer: `customer@test.com`
- [ ] **2.3.6** Seed sample promotions (optional)

---

## 💼 Phase 3: Dealer Staff UI (Week 3) ⭐ **TRANG CHÍNH**

**Mục tiêu:** UI đẹp cho các chức năng core của Dealer Staff

### 3.1 Layout & Navigation
- [ ] **3.1.1** Dashboard layout đẹp (sidebar navigation, header)
  - ✅ Sidebar: Background `--black` (#0B0B0C), text `--text`
  - ✅ Header: Background `--black`, border-bottom `--border`
  - ✅ Main content: Background `--bg` (#0F172A)
  
- [ ] **3.1.2** Tạo custom CSS file với Dark Theme
  - ✅ File `wwwroot/css/dark-theme.css` với CSS variables từ UI Guidelines
  - ✅ Override Bootstrap default colors
  - ✅ Apply dark theme cho tất cả components
  
- [ ] **3.1.3** Responsive design
  - ✅ Sidebar collapse trên mobile
  - ✅ Cards stack trên mobile

### 3.2 Vehicle Catalog (Dealer Staff)
- [ ] **3.2.1** Vehicle list page
  - ✅ Grid layout với cards đẹp
  - ✅ Hình ảnh xe (ImageUrl)
  - ✅ Filter: Model, Status
  - ✅ Search box
  - ✅ Click vào card → detail page
  
- [x] **3.2.2** Vehicle detail page
  - ✅ Image gallery (1 ảnh lớn)
  - ✅ Specs table (từ SpecJson - parsed từ JSON)
  - ✅ Price hiển thị rõ ràng (MSRP cho Customer, MSRP + Wholesale cho Dealer)
  - ✅ Stock availability (colors, quantities) - EVM stock cho Dealer
  - ✅ Button "Tạo báo giá" cho Dealer, "Yêu cầu báo giá" + "Đặt lịch lái thử" cho Customer
  - ✅ Available dealers list cho Customer

### 3.3 Sales Management (Core feature!) ⭐

- [ ] **3.3.1** Create Quote page
  - ✅ Select customer (search existing hoặc tạo mới)
  - ✅ Add vehicles (select từ catalog)
  - ✅ Select color, quantity
  - ✅ Apply promotion (dropdown)
  - ✅ Preview totals
  - ✅ Save as DRAFT hoặc SEND
  
- [x] **3.3.2** Quote list page
  - ✅ Table với status badges màu sắc
  - ✅ Filter: Status, Customer, Date
  - ✅ Actions: View, Edit, Convert to Order
  - ✅ Convert to Order functionality: Copy quote lines → Create new ORDER với status OPEN
  
- [ ] **3.3.3** Create Order page
  - ✅ Tương tự Quote, nhưng có payment terms
  - ✅ Có thể create từ Quote (auto-fill)
  
- [ ] **3.3.4** Order list & detail
  - ✅ List: Status, Customer, Total, Date
  - ✅ Detail page: Items table, Payment history, Delivery info
  - ✅ Button "Thêm thanh toán"
  - ✅ Button "Lên lịch giao xe"
  
- [x] **3.3.5** Payment entry (Modal hoặc separate page)
  - ✅ Method (CASH/FINANCE)
  - ✅ Amount input
  - ✅ Auto update order status
  - ✅ Validation: amount > 0, không vượt quá remaining amount
  - ✅ Auto update order status to PAID khi đủ tiền
  
- [x] **3.3.6** Delivery scheduling
  - ✅ Date picker
  - ✅ Time picker
  - ✅ Mark delivered với handover note
  - ✅ Auto update order status to DELIVERED

### 3.4 Customer Management
- [ ] **3.4.1** Customer list (table với search)
- [ ] **3.4.2** Create/Edit customer (form đẹp)
- [ ] **3.4.3** Test drive calendar/view
  - ✅ Calendar view hoặc list view
  - ✅ Status badges
  - ✅ Confirm/Done actions

### 3.5 UI Components Reusable
- [ ] **3.5.1** Status badges (color-coded theo dark theme)
  - ✅ Success: `--success`, Warning: `--warning`, Error: `--error`
  - ✅ Default: `--text-muted` background với border
  
- [ ] **3.5.2** Form validation messages
  - ✅ Input error: border `--error`, text `--error`
  - ✅ Input success: border `--success`
  - ✅ Validation feedback styling
  
- [ ] **3.5.3** Toast notifications (success/error)
  - ✅ Background `--surface`, border theo status color
  - ✅ Position: top-right, animation slide-in
  
- [ ] **3.5.4** Loading states
  - ✅ Spinner màu accent cyan hoặc white
  - ✅ Overlay dark với opacity cho loading modals

### 3.6 Dark Theme CSS Setup
- [ ] **3.6.1** Tạo `wwwroot/css/dark-theme.css`
  - ✅ Copy toàn bộ CSS variables và styles từ UI Guidelines
  - ✅ Override Bootstrap 5 default theme colors
  - ✅ Custom classes: `.surface`, `.text-muted`, `.border-subtle`
  
- [ ] **3.6.2** Update `_Layout.cshtml`
  - ✅ Include `dark-theme.css` sau Bootstrap CSS
  - ✅ Apply dark theme classes cho body, sidebar, header
  
- [ ] **3.6.3** Test contrast & accessibility
  - ✅ Verify WCAG 4.5:1 contrast ratios
  - ✅ Test trên các browsers (Chrome, Firefox, Edge)

---

## 🏭 Phase 4: EVM Staff UI (Week 3-4)

**Mục tiêu:** UI để quản lý từ phía hãng xe (đơn giản nhưng đẹp)  
**Note:** EVM Staff và EVM Admin có thể dùng chung một số pages, chỉ khác ở Reports và System Management

### 4.1 Product Management
- [ ] **4.1.1** Vehicle catalog management
  - ✅ CRUD: Create/Edit/Delete
  - ✅ Form với image upload (hoặc URL input)
  - ✅ Spec editor (JSON editor đơn giản hoặc form fields)
  
- [ ] **4.1.2** Price Policy management
  - ✅ List price policies
  - ✅ Create new (select vehicle, dealer, set prices, date range)
  - ✅ Simple validation (no overlap - có thể skip nếu phức tạp)
  
- [ ] **4.1.3** Stock management
  - ✅ View EVM stock (table)
  - ✅ Update quantities (simple +/- buttons)

### 4.2 Dealer Management
- [ ] **4.2.1** Dealer list & detail
- [ ] **4.2.2** Dealer Order processing
  - ✅ List orders (status: SUBMITTED)
  - ✅ Approve/Reject buttons
  - ✅ Fulfill order → transfer stock

### 4.3 Reports (EVM Staff & Admin - Đơn giản, không cần charts phức tạp)
- [ ] **4.3.1** Sales by Dealer (simple table)
- [ ] **4.3.2** Inventory summary (table)

---

## 👑 Phase 4b: EVM Admin UI (Week 4)

**Mục tiêu:** Trang Admin với Dashboard & Reports đẹp

### 4b.1 Admin Dashboard
- [ ] **4b.1.1** Dashboard page với summary cards:
  - ✅ Tổng doanh số (hôm nay, tháng này)
  - ✅ Số lượng đơn hàng
  - ✅ Số đại lý đang hoạt động
  - ✅ Số xe đã bán
  - ✅ Card layout đẹp với icons
  
### 4b.2 Reports & Analytics (EVM Admin)
- [ ] **4b.2.1** Sales by Region/Dealer report
  - ✅ Table: Dealer name, Total sales, Order count
  - ✅ Filter: Date range, Region (nếu có)
  - ✅ Sortable columns
  - ✅ Export to Excel (optional - nếu có thời gian)
  
- [ ] **4b.2.2** Sales by Vehicle report
  - ✅ Table: Vehicle model, Variant, Quantity sold, Revenue
  - ✅ Top selling vehicles highlighted
  
- [ ] **4b.2.3** Inventory Analysis
  - ✅ Stock levels (EVM + all Dealers)
  - ✅ Slow-moving vehicles (qty cao, bán ít)
  - ✅ Fast-moving vehicles (bán nhanh)
  
- [ ] **4b.2.4** Consumption Speed Analysis
  - ✅ Vehicle popularity metrics
  - ✅ Sales velocity (xe/ngày hoặc xe/tuần)
  - ✅ Table format đơn giản

### 4b.3 System Management (Nếu cần)
- [ ] **4b.3.1** User management (optional)
  - ✅ List users với roles
  - ✅ Edit user role
  - ✅ Disable/Enable users
  
- [ ] **4b.3.2** Dealer management (extend từ EVM Staff)
  - ✅ View all dealers
  - ✅ Dealer status management
  - ✅ Sales targets (nếu có trong DB)

### 4b.4 Navigation & Access Control
- [ ] **4b.4.1** Admin menu/sidebar riêng
  - ✅ Dashboard
  - ✅ Reports (submenu: Sales, Inventory, Consumption)
  - ✅ System Management
  - ✅ Logout
  
- [ ] **4b.4.2** Authorization check:
  - ✅ Chỉ EVM_ADMIN mới vào được admin pages
  - ✅ `[Authorize(Roles = "EVM_ADMIN")]`

---

## 👤 Phase 5: Customer Portal + Polish (Week 4-5)

### 5.1 Public Catalog (Customer)
- [ ] **5.1.1** Public vehicle catalog
  - ✅ Landing page đẹp với hero section
  - ✅ Grid layout với vehicle cards
  - ✅ Filter & search
  - ✅ Click vào → detail page
  
- [ ] **5.1.2** Vehicle detail page (public)
  - ✅ Image, specs, price (MSRP)
  - ✅ Button "Yêu cầu báo giá" (require login)
  - ✅ Button "Đặt lịch lái thử" (require login)

### 5.2 Customer Account
- [ ] **5.2.1** Registration & Login (UI đẹp)
- [ ] **5.2.2** Profile page
- [ ] **5.2.3** My Quotes page (list quotes của customer)
- [ ] **5.2.4** My Orders page (track orders)
- [ ] **5.2.5** Test drive booking
  - ✅ Form: Select dealer, vehicle, date/time
  - ✅ My bookings list
- [ ] **5.2.6** Feedback form

### 5.3 UI Polish & Demo Prep
- [ ] **5.3.1** ✅ Đảm bảo tất cả pages responsive
- [ ] **5.3.2** ✅ Thêm icons (Font Awesome hoặc Bootstrap Icons)
- [ ] **5.3.3** ✅ Color scheme nhất quán
- [ ] **5.3.4** ✅ Loading states, error messages
- [ ] **5.3.5** ✅ Remove console errors
- [ ] **5.3.6** ✅ Seed đủ data để demo flow đầy đủ:
  - Có vehicles, customers, quotes, orders mẫu
  - Có test drive bookings
  - Có payments, deliveries

---

## 🎨 UI/UX Guidelines

### Design System - Dark Mode (Charcoal/Black/White)

**Framework:** Bootstrap 5 + Custom Dark Theme  
**Icons:** Bootstrap Icons (free)

#### Color Palette (Dark Mode)

**Base Colors:**
- **Background (Charcoal):** `#0F172A` hoặc `#111827` - Nền chính, đỡ chói
- **Surface/Card:** `#1F2937` - Cards, tables, panels
- **Black (Deep):** `#0B0B0C` - Header, sidebar, sections nhấn mạnh
- **Text Primary:** `#E5E7EB` - Text chính, tiêu đề
- **Text Muted:** `#94A3B8` - Text phụ, labels, descriptions
- **Border Subtle:** `#FFFFFF1A` (white 10% opacity) - Borders, dividers
- **Highlight/CTA:** `#FFFFFF` - Tiêu đề, CTA buttons (dùng có chủ đích)

**Accent (Dùng ít, khi cần):**
- **Accent Light:** `#9BEAFB` (xanh băng) - Chỉ cho links/CTA quan trọng, không phá tông
- **Success:** `#10B981` hoặc `#22C55E` - Khi cần báo success
- **Warning:** `#F59E0B` - Khi cần báo warning
- **Error:** `#EF4444` hoặc `#DC2626` - Khi cần báo lỗi

#### CSS Variables (Token System)

```css
:root {
  /* Base */
  --bg: #0F172A;              /* charcoal - nền chính */
  --surface: #1F2937;         /* card/table */
  --black: #0B0B0C;           /* header/sidebar */
  
  /* Text */
  --text: #E5E7EB;            /* text primary */
  --text-muted: #94A3B8;     /* text phụ */
  
  /* Borders & Lines */
  --border: rgba(255, 255, 255, 0.1);  /* 10% white opacity */
  --border-subtle: rgba(255, 255, 255, 0.08);
  
  /* Accents */
  --accent: #FFFFFF;          /* highlight/CTA (dùng ít) */
  --accent-cyan: #9BEAFB;    /* link/CTA nhẹ */
  
  /* Status Colors (dùng khi cần) */
  --success: #10B981;
  --warning: #F59E0B;
  --error: #EF4444;
}

/* Base Styles */
body {
  background: var(--bg);
  color: var(--text);
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

/* Cards */
.card, .surface {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 16px;
  padding: 1.5rem;
}

/* Headers/Sidebars */
.header, .sidebar {
  background: var(--black);
  border-bottom: 1px solid var(--border);
}

/* Tables */
.table {
  background: var(--surface);
  color: var(--text);
}

.table tbody tr:nth-child(even) {
  background: #0F172A;
}

.table tbody tr:nth-child(odd) {
  background: #162033;
}

.table td, .table th {
  border-color: rgba(255, 255, 255, 0.08);
}

/* Borders & Dividers */
hr {
  border-color: var(--border);
  opacity: 0.1;
}

/* Icons & Lines */
.icon, .line {
  color: #CBD5E1;
  opacity: 0.7-0.8;
}
```

#### Layout Hierarchy (3 Layers)
1. **Background:** `#0F172A` (charcoal)
2. **Surface/Card:** `#1F2937`
3. **Text:** `#E5E7EB` trên surface (không đặt text xám nhạt trên đen tuyệt đối)

#### Best Practices
- ✅ **Cards layout** - Dùng `--surface` cho cards, border `--border`
- ✅ **Tables & Forms** - Hàng chẵn/lẻ: `#0F172A` / `#162033`, border `rgba(255,255,255,0.08)`
- ✅ **Status badges** - Dùng màu status (success/warning/error) chỉ khi cần, còn lại dùng text-muted
- ✅ **Icons/Lines** - `#CBD5E1` ở 70-80% opacity
- ✅ **Text placement** - Không để text xám nhạt trên đen tuyệt đối; đặt trên charcoal/surface
- ✅ **CTA buttons** - Dùng trắng `#FFFFFF` hoặc accent cyan `#9BEAFB` cho CTA quan trọng
- ✅ **Modal** - Background overlay: `rgba(0, 0, 0, 0.7)`, modal: `--surface`
- ✅ **Form inputs** - Background: `--surface`, border: `--border`, text: `--text`
- ✅ **Spacing:** Bootstrap spacing utilities

#### WCAG Contrast Guidelines
- ✅ Body text ≥ 4.5:1 với nền (text `#E5E7EB` trên `#1F2937` = ~5:1)
- ✅ CTA trắng trên đen/than thường đạt 7:1 → ổn
- ✅ Subtle labels: tăng size/weight nếu màu nhạt

#### When to Use Color
- **Thuần grayscale:** Giữ đen-than-trắng cho hầu hết UI → sang và bền
- **Thêm màu:** Chỉ khi cần:
  - Gợi trạng thái (success/warning/error badges)
  - Nhấn CTA quan trọng (dùng `#FFFFFF` hoặc `#9BEAFB`)
  - Link hover (dùng accent cyan nhẹ)

#### UI Components (Dark Mode)
- ✅ **Cards layout** - Vehicle listings dùng cards với `--surface`, hover: border accent
- ✅ **Status badges** - Success/Warning/Error dùng màu status, còn lại text-muted
- ✅ **Modal** - Overlay dark, modal `--surface`, close button text-primary
- ✅ **Toast notifications** - Background `--surface`, border status color
- ✅ **Form validation** - Input `--surface`, border error/warning/success khi validate
- ✅ **Loading spinners** - Spinner màu accent cyan hoặc white
- ✅ **Empty states** - Icon muted, text text-muted, CTA accent cyan
- ✅ **Buttons:**
  - Primary CTA: White `#FFFFFF` text trên dark background
  - Secondary: Border `--border`, text `--text`
  - Accent CTA: Background `--accent-cyan`, text dark
- ✅ **Tables:** Striped rows với alternating `--bg` / `#162033`

### Pages Priority
1. **HIGH:** Vehicle Catalog, Create Quote, Order Management (Dealer Staff)
2. **MEDIUM:** Customer Portal, EVM Product Management, Admin Dashboard
3. **LOW:** Admin Reports (có thể làm đơn giản bằng table)

---

## 📋 Quick Checklist

### Must Have (Demo được) - ✅ HOÀN THÀNH 100%
- [x] Database 15 tables + Seed data
- [x] Session-based Authentication + 5 roles
- [x] Vehicle Catalog (đẹp) với Vehicle Detail pages
- [x] Quote → Order workflow (Create Quote → Quote Detail → Convert to Order)
- [x] Payment tracking (Add Payment form, history, auto status update)
- [x] Delivery tracking (Schedule, Mark Delivered)
- [x] Customer Portal (Vehicles, MyQuotes, MyOrders, TestDrive, OrderDetail với timeline)
- [x] EVM Product Management (Vehicles, PricePolicies, Stocks, Dealers, DealerOrders)
- [x] Admin Dashboard & Reports (Sales, Inventory, Consumption, Users)
- [x] Dealer Manager Dashboard & Reports (SalesByStaff, Debts)

### Nice to Have (Nếu có thời gian)
- [ ] Test Drive booking (UI đẹp)
- [ ] Feedback system
- [ ] Promotions (basic)
- [ ] Simple reports

### Skip (Không cần cho demo)
- ❌ Unit tests chi tiết (có thể test manual)
- ❌ API layer (dùng Razor Pages trực tiếp)
- ❌ Advanced reports với charts
- ❌ Email notifications
- ❌ Audit logging
- ❌ Multi-language
- ❌ Advanced state machine validation

---

## 🚀 Timeline Estimate

| Phase | Duration | Key Deliverables |
|-------|----------|------------------|
| Phase 1 | 1 tuần | Database + Auth working |
| Phase 2 | 1 tuần | Services + Seed data |
| Phase 3 | 1 tuần | Dealer Staff UI (core pages) |
| Phase 4 | 0.5 tuần | EVM Staff UI (basic) |
| Phase 4b | 0.5 tuần | EVM Admin UI (Dashboard + Reports) |
| Phase 5 | 1-1.5 tuần | Customer Portal + Polish |

**Total: 5 - 6 tuần** (có thể rút gọn nếu làm nhanh)

---

## 📝 Notes

- **Database:** Theo database.md nhưng có thể bỏ qua một số validation phức tạp nếu không cần thiết
- **State Machine:** Đơn giản hóa - chỉ cần update status theo logic cơ bản, không cần strict state machine
- **UI Priority:** Dealer Staff UI là quan trọng nhất vì đây là trang chính của hệ thống
- **Demo Data:** Seed đủ data để có thể demo flow đầy đủ từ Quote → Order → Payment → Delivery

---

**Last Updated:** 2025-01-XX  
**Version:** 2.0 (Simplified for Student Demo Project)

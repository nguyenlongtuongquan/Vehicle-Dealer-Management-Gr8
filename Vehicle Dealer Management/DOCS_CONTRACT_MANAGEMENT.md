# ?? Qu?n l? H?p Ð?ng (Contract Management)

## ?? T?ng quan

Ch?c nãng qu?n l? h?p ð?ng cho phép ð?i l? t?o, theo d?i và qu?n l? h?p ð?ng mua bán xe ði?n v?i khách hàng. H?p ð?ng ðý?c t?o t? ð?ng t? ðõn hàng ð? thanh toán ð?y ð?.

---

## ?? Quy tr?nh t?o h?p ð?ng

```
QUOTE (Báo giá)
    ?
ORDER (Ðõn hàng)
    ? (Thanh toán ð? 100% + Giao xe hoàn thành)
CONTRACT (H?p ð?ng)
```

### Ði?u ki?n t?o h?p ð?ng:
1. ? Ðõn hàng ph?i ? tr?ng thái `PAID` ho?c `DELIVERED`
2. ? Ð? thanh toán **ð? 100%** giá tr? ðõn hàng
3. ? Không ðý?c trùng l?p (1 ðõn hàng = 1 h?p ð?ng)

---

## ?? Các tr?ng thái h?p ð?ng

| Tr?ng thái | Mô t? | Hành ð?ng |
|-----------|-------|-----------|
| `ACTIVE` | H?p ð?ng ðang ho?t ð?ng | Hoàn thành / H?y |
| `COMPLETED` | H?p ð?ng ð? hoàn thành | - |
| `TERMINATED` | H?p ð?ng b? h?y | - |

---

## ?? Các tính nãng

### 1. Danh sách h?p ð?ng (`/Dealer/Sales/Contracts`)
- ? Xem t?t c? h?p ð?ng c?a ð?i l?
- ? L?c theo tr?ng thái (T?t c?, Ðang ho?t ð?ng, Hoàn thành, Ð? h?y)
- ? Hi?n th? th?ng kê:
  - T?ng h?p ð?ng
  - Ðang ho?t ð?ng
  - Hoàn thành
  - Ð? h?y

### 2. Chi ti?t h?p ð?ng (`/Dealer/Sales/ContractDetail`)
- ? Xem thông tin chi ti?t h?p ð?ng
- ? Thông tin bên mua (Khách hàng)
- ? Thông tin bên bán (Ð?i l?)
- ? Danh sách xe trong h?p ð?ng
- ? Ngày k?, ngày t?o
- ? T?ng giá tr? h?p ð?ng
- ? **Hoàn thành h?p ð?ng** (n?u ðang ACTIVE)
- ? **H?y h?p ð?ng** (n?u ðang ACTIVE)

### 3. T?o h?p ð?ng t? ðõn hàng
- ? Nút "T?o h?p ð?ng" trong trang `OrderDetail`
- ? T? ð?ng copy t?t c? thông tin t? ORDER ? CONTRACT
- ? Validation ð?y ð? (thanh toán, trùng l?p, tr?ng thái)

---

## ??? API & Services

### `ISalesDocumentService`
```csharp
Task<SalesDocument> ConvertOrderToContractAsync(int orderId);
```

**Ch?c nãng:**
- T?o h?p ð?ng m?i t? ðõn hàng
- Copy t?t c? `SalesDocumentLine` t? ORDER ? CONTRACT
- Set tr?ng thái CONTRACT = `ACTIVE`
- Set `SignedAt` = th?i gian hi?n t?i

**Validation:**
- Ðõn hàng ph?i t?n t?i
- Type ph?i là `ORDER`
- Status ph?i là `PAID` ho?c `DELIVERED`
- Không ðý?c có h?p ð?ng trùng l?p (cùng customer, dealer, trong 24h)

---

## ?? Files ð? t?o/ch?nh s?a

### Backend (Services)
1. `ISalesDocumentService.cs` - Thêm `ConvertOrderToContractAsync()`
2. `SalesDocumentService.cs` - Implement logic t?o h?p ð?ng

### Frontend (Razor Pages)
1. `Contracts.cshtml` - Danh sách h?p ð?ng
2. `Contracts.cshtml.cs` - PageModel cho danh sách
3. `ContractDetail.cshtml` - Chi ti?t h?p ð?ng
4. `ContractDetail.cshtml.cs` - PageModel cho chi ti?t
5. `OrderDetail.cshtml.cs` - Thêm `OnPostCreateContractAsync()`

---

## ?? UI/UX

### Màn h?nh danh sách h?p ð?ng
- **Header:** Tiêu ð? + th?ng kê (4 cards)
- **Tabs:** L?c theo tr?ng thái
- **Table:** Hi?n th? danh sách h?p ð?ng
  - M? h?p ð?ng (CTR-XXXXXX)
  - Khách hàng (Tên + SÐT)
  - Ngày k?
  - S? xe
  - T?ng giá tr?
  - Tr?ng thái (badge màu)
  - Button "Chi ti?t"

### Màn h?nh chi ti?t h?p ð?ng
- **Header:** M? h?p ð?ng + Actions (Hoàn thành / H?y)
- **Stats:** Tr?ng thái, Ngày k?, T?ng giá tr?
- **2 C?t:**
  - **Trái:** Thông tin h?p ð?ng, Khách hàng, Ð?i l?
  - **Ph?i:** Danh sách xe (table)
- **Modal:** H?y h?p ð?ng (yêu c?u nh?p l? do)

---

## ?? Navigation

### T? Orders ? Contracts
```
/Dealer/Sales/OrderDetail?id=X
    ? (Button "T?o h?p ð?ng")
/Dealer/Sales/ContractDetail?id=Y
```

### Menu chính
```
Dealer Dashboard
    ??? Báo giá (/Dealer/Sales/Quotes)
    ??? Ðõn hàng (/Dealer/Sales/Orders)
    ??? H?p ð?ng (/Dealer/Sales/Contracts) ? NEW!
```

---

## ? Checklist hoàn thành

- [x] Model `SalesDocument` v?i Type = "CONTRACT"
- [x] Service method `ConvertOrderToContractAsync()`
- [x] Trang danh sách h?p ð?ng
- [x] Trang chi ti?t h?p ð?ng
- [x] Nút t?o h?p ð?ng trong OrderDetail
- [x] Validation ð?y ð?
- [x] UI/UX hoàn ch?nh
- [x] Build thành công

---

## ?? Hý?ng d?n s? d?ng

### 1. T?o h?p ð?ng m?i
1. Vào **Qu?n l? ðõn hàng** (`/Dealer/Sales/Orders`)
2. Ch?n ðõn hàng ð? **thanh toán ð? 100%** và **giao xe xong**
3. Click **"T?o h?p ð?ng"**
4. H? th?ng t? ð?ng t?o h?p ð?ng và chuy?n ð?n trang chi ti?t

### 2. Qu?n l? h?p ð?ng
1. Vào **Qu?n l? h?p ð?ng** (`/Dealer/Sales/Contracts`)
2. L?c theo tr?ng thái n?u c?n
3. Click **"Chi ti?t"** ð? xem thông tin ð?y ð?
4. **Hoàn thành** ho?c **H?y** h?p ð?ng n?u c?n

### 3. Xem chi ti?t h?p ð?ng
1. Vào chi ti?t h?p ð?ng
2. Xem:
   - Thông tin khách hàng (bên mua)
   - Thông tin ð?i l? (bên bán)
   - Danh sách xe trong h?p ð?ng
   - Ngày k?, t?ng giá tr?
3. Th?c hi?n hành ð?ng (n?u h?p ð?ng ðang ACTIVE):
   - **Hoàn thành:** Set status = COMPLETED
   - **H?y:** Set status = TERMINATED (c?n nh?p l? do)

---

## ?? Notes

- H?p ð?ng **ch? ðý?c t?o** khi ðõn hàng ð? thanh toán ð? 100%
- M?t ðõn hàng **ch? có th? t?o 1 h?p ð?ng** (không trùng l?p)
- H?p ð?ng **không th? xóa** (ch? có th? h?y)
- H?p ð?ng ð? **COMPLETED** ho?c **TERMINATED** không th? thay ð?i tr?ng thái

---

## ?? Các tính nãng có th? m? r?ng

1. ? **In h?p ð?ng PDF** (Export to PDF)
2. ? **G?i h?p ð?ng qua email** cho khách hàng
3. ? **Ch? k? ði?n t?** (E-signature)
4. ? **Upload file ðính kèm** (h?p ð?ng scan, gi?y t?)
5. ? **L?ch s? thay ð?i** (audit log)
6. ? **Báo cáo h?p ð?ng** theo tháng/nãm
7. ? **T? ð?ng gia h?n** h?p ð?ng b?o hành

---

**Phát tri?n b?i:** GitHub Copilot  
**Ngày:** 2024  
**Version:** 1.0

# System Memory & Learning Log

Tài liệu này lưu vết các vấn đề gặp phải và bài học kinh nghiệm dưới dạng nhật ký. 

*(Lưu ý: Các mục đã được định hình thành cấu trúc bắt buộc hoặc quy chuẩn hệ thống trong `rules.md` đã được lược bỏ để tập trung vào các vấn đề/kinh nghiệm cụ thể).*

---

## [2026-04-28]

### Vấn đề:
Lỗi 500 Internal Server Error khi chạy Integration Tests diện rộng.

### Nguyên nhân:
Schema SQLite trong `CustomWebApplicationFactory` không khớp với POCO entities. Thiếu các trường audit (`is_active`, `created_at`, `updated_at`) và trường `normalized_name`. Ngoài ra, SQLite yêu cầu ánh xạ chính xác giữa PascalCase (C#) và snake_case (SQL).

### Giải pháp:
- Cập nhật script khởi tạo SQLite trong bộ test để bao gồm đầy đủ các cột.
- Thực hiện chuẩn hóa dữ liệu (case normalization) ngay trong logic seeding của test.
- Đảm bảo mapping `DefaultTypeMap.MatchNamesWithUnderscores = true` được kích hoạt trong môi trường test.

---

## [2026-04-29]

### Chủ đề:
Tối ưu hóa tìm kiếm với Name Normalization.

### Tags:
[sql] [performance] [search] [database]

### Context:
Tăng tốc độ truy vấn và giải quyết vấn đề Case-Sensitivity trong tìm kiếm chuỗi.

### Rule rút ra:
- Luôn chuẩn bị sẵn, lưu trữ và truy vấn trên cột `normalized_name` (đã trim và lowercase).
- Thực hiện chuẩn hóa dữ liệu đầu vào (search term) trước khi thực hiện câu lệnh `LIKE`.

### Áp dụng khi:
- Tìm kiếm Game, GamePackage, User theo tên.
- Các tính năng lọc (Filter) dữ liệu dựa trên chuỗi ký tự.

### Tránh:
- Tuyệt đối tránh sử dụng hàm SQL `LOWER()` hoặc `UPPER()` trong mệnh đề `WHERE` vì sẽ làm vô hiệu hóa Database Index trên cột tương ứng.

### Ví dụ:
```sql
SELECT * FROM game_packages WHERE normalized_name LIKE @SearchTerm;
```

---

## [2026-04-29]

### Chủ đề:
Coding Style: Explicit Blocks vs Expression-bodied Members.

### Tags:
[coding-style] [readability] [maintenance]

### Context:
Duy trì tính minh bạch và khả năng mở rộng của mã nguồn.

### Rule rút ra:
- Ưu tiên sử dụng khối mã tường minh `{ ... }` thay vì lambda `=>` cho các logic nghiệp vụ.
- Bắt buộc thêm chú thích `Rationale` cho các quyết định xử lý quan trọng.

### Áp dụng khi:
- Logic có độ phức tạp trung bình trở lên hoặc chứa nhiều bước.
- Cần đặt breakpoint để debug hoặc cần giải thích lý do xử lý.

### Tránh:
- Viết các method dài hoặc phức tạp dưới dạng expression-bodied (`=>`), gây khó đọc và khó bảo trì.

### Ví dụ:
```csharp
public async Task<CheckoutResponseDTO> ProcessOrderAsync(...)
{
    // Rationale: Kiểm tra số dư trước khi tạo đơn để tránh lỗi transaction
    if (balance < total) throw new BusinessException("Không đủ số dư");
    
    // logic...
}
```

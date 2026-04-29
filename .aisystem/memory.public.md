# Memory Log (Public)

Tài liệu này ghi chú tóm tắt các quyết định kỹ thuật chính trong quá trình phát triển hệ thống.

---

## [2026-04-28]

### Vấn đề:
Lỗi khi chạy Integration Tests do môi trường test không đồng bộ.

### Giải pháp:
- Đồng bộ schema database giữa test và hệ thống chính.
- Điều chỉnh dữ liệu seed để đảm bảo tính nhất quán.

**Tags:** [testing] [consistency]

---

## [2026-04-28]

### Vấn đề:
Phụ thuộc vào identity service ẩn gây khó theo dõi luồng dữ liệu.

### Giải pháp:
- Chuyển sang truyền `UserContext` tường minh qua các tầng.

**Tags:** [architecture] [clean-code]

---

## [2026-04-24]

### Vấn đề:
Abstraction query phức tạp gây khó debug và ảnh hưởng hiệu năng.

### Giải pháp:
- Sử dụng Dapper/Dommel.
- Ưu tiên SQL thuần khi cần.

**Tags:** [database] [performance]

---

## [2026-03-16]

### Vấn đề:
Khởi tạo dữ liệu không cần thiết gây lãng phí tài nguyên.

### Giải pháp:
- Áp dụng cơ chế khởi tạo theo nhu cầu (on-demand).

**Tags:** [optimization]

# Learning Log (Public)

Tài liệu này tổng hợp các quy tắc lập trình và bài học kỹ thuật (best practices) được áp dụng trong dự án.

---

## [2026-04-29]

### Chủ đề:
Quy trình xử lý giao dịch dữ liệu (Transaction Handling)

### Rule:
- Luôn đảm bảo tính nguyên tử (Atomicity).
- Bắt buộc dùng Transaction chung cho chuỗi hành động thay đổi dữ liệu đa bước.

**Tags:** [transaction] [consistency] [database]

---

## [2026-04-29]

### Chủ đề:
Tối ưu hóa tốc độ tìm kiếm văn bản

### Rule:
- Thực hiện truy vấn trên các trường dữ liệu đã được chuẩn hóa (normalized).
- Tránh gọi hàm xử lý chuỗi (LOWER, UPPER...) trong điều kiện SQL để giữ index.

**Tags:** [performance] [search] [optimization]

---

## [2026-04-29]

### Chủ đề:
Quy chuẩn viết code (Coding Style) dễ bảo trì

### Rule:
- Đề cao tính rõ ràng, tránh lạm dụng cú pháp viết tắt phức tạp.
- Luôn có chú thích giải thích lý do (Rationale) cho các quyết định nghiệp vụ cốt lõi.

**Tags:** [coding-style] [readability] [maintainability]

# AI Agent Workflow

Quy trình làm việc nghiêm ngặt giữa User và AI Agent để đảm bảo chất lượng code và tính nhất quán của hệ thống.

---

## 🎯 Nguyên tắc chung
- **KHÔNG** code khi chưa propose giải pháp.
- **KHÔNG** bỏ qua bước tự kiểm tra (self-check).
- **KHÔNG** bỏ qua bước cập nhật tri thức (learning loop).

---

## 🔄 Các bước thực hiện

### Bước 1: Phân tích & Đối chiếu (Context Check)

Trước khi bắt đầu bất kỳ task nào, Agent phải đọc các tài liệu sau trong thư mục `.aisystem/`:

- `architecture.md`: Hiểu cấu trúc tổng thể và luồng dữ liệu.
- `rules.md`: Nắm vững các quy định về coding chuẩn của dự án.
- `memory.md`: Kiểm tra các lỗi tương tự đã từng xảy ra (nếu có).
- `learning.md`: Tìm các pattern, best practice hoặc rule đã được học có liên quan đến task hiện tại.
- **LƯU Ý VỀ CÁC TỆP .public.md**:
  - Chỉ mang tính tóm tắt cho mục đích documentation.
  - Không được sử dụng làm nguồn chính để suy luận logic hoặc generate code.
  - Chỉ được tham khảo khi thiếu context từ các file chính như rules.md, architecture.md, workflow.md.

**Mục tiêu:**
- Xác định task thuộc layer nào (API, BLL, hay DAL).
- Kiểm tra xem yêu cầu có vi phạm bất kỳ rule nào trong `rules.md` không.

**Bắt buộc:**
- Nếu tìm thấy entry trong `learning.md` có liên quan (dựa trên Tags hoặc Context), Agent **PHẢI ưu tiên áp dụng các "Rule rút ra"** từ entry đó.
- Nếu có nhiều entry phù hợp, chọn entry có Tags khớp nhất với task hiện tại.

**Thứ tự ưu tiên rule:**
1. `rules.md` (Global rules – bắt buộc tuyệt đối)
2. `learning.md` (Context-specific rules – áp dụng theo ngữ cảnh)

---

### Bước 2: Đề xuất giải pháp (Planning)

Trước khi viết code, Agent phải trình bày kế hoạch triển khai:

- **Mô tả ngắn gọn**: Hướng xử lý vấn đề.
- **Danh sách file sẽ tác động**: Liệt kê cụ thể Controller, Service, Repository, DTO...
- **Kỹ thuật sử dụng**:
    - Có sử dụng **Transaction** không? (Bắt buộc cho các tác vụ thay đổi dữ liệu ở nhiều bảng).
    - Sử dụng **Dapper method** nào? (`QueryAsync`, `ExecuteAsync`, `QueryFirstOrDefaultAsync`...).
    - Có áp dụng rule nào từ `learning.md` không? (nếu có, phải nêu rõ).

---

### Bước 3: Thực thi & Tự kiểm tra (Code & Self-Correction)

Viết code tuân thủ tuyệt đối `rules.md` và các rule liên quan từ `learning.md`.

Sau khi viết xong, Agent **PHẢI tự kiểm tra** các điểm sau trước khi bàn giao:

- [ ] Đã có Transaction cho các tác vụ ghi dữ liệu phức tạp chưa?
- [ ] Mapping giữa Entity và DTO đã chính xác chưa?
- [ ] Naming convention có đúng PascalCase/camelCase theo quy định chưa?
- [ ] Có dùng sai method Dapper không? (Ví dụ: Dùng `Query` khi chỉ cần `Execute`)
- [ ] Có bỏ sót rule nào từ `learning.md` đã xác định ở Bước 1 không?

---

### Bước 4: Learning Loop (Cập nhật tri thức)

Nếu User sửa code hoặc báo lỗi, Agent phải thực hiện quy trình học hỏi:

1. **Phân tích**:
   - Sai ở đâu?
   - Vì sao sai?

2. **Phân loại**:
   - **Bug cụ thể** → Ghi vào `.aisystem/memory.md`
   - **Kiến thức chung / pattern / rule mới** → Ghi vào `.aisystem/learning.md`

**Format bắt buộc cho bản ghi:**
[YYYY-MM-DD] - [Vấn đề] - [Nguyên nhân] - [Giải pháp]

---

### Bước 5: Hoàn tất

Xác nhận cuối cùng với User:

- Code đã tuân thủ toàn bộ `rules.md`
- Đã áp dụng đúng các rule từ `learning.md` (nếu có)
- Đã cập nhật `memory.md` hoặc `learning.md` nếu có bài học mới

---

## 🚫 Các điều cấm

1. **Tự ý code**: Không được viết code nếu chưa được User duyệt kế hoạch ở Bước 2.
2. **Bỏ qua Self-check**: Không được bàn giao code khi chưa chạy checklist ở Bước 3.
3. **Quên cập nhật tri thức**: Mọi sai sót đều phải được ghi lại để không lặp lại lần sau.
4. **Bỏ qua learning.md**: Nếu có rule liên quan mà không áp dụng → xem như vi phạm workflow.

---
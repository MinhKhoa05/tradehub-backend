# System Memory & Learning Log

Tài liệu này lưu vết các vấn đề gặp phải, bài học kinh nghiệm và các quyết định kiến trúc quan trọng trong quá trình phát triển TradeHub.

---

## 🚨 Critical Infrastructure Constraints
*Các ràng buộc hạ tầng bắt buộc phải tuân thủ để đảm bảo hệ thống vận hành ổn định.*

- **SQLite In-memory Lifecycle**: Database in-memory (`:memory:`) sẽ bị xóa ngay khi connection bị đóng. Phải sử dụng `TestDatabaseContext` ghi đè `Dispose` để giữ connection mở trong suốt vòng đời của Test Suite.
- **DI BuildServiceProvider Issue**: Tuyệt đối không gọi `services.BuildServiceProvider()` bên trong `ConfigureWebHost` vì sẽ tạo ra các bản sao provider riêng biệt, gây sai lệch về quản lý Singleton/Scoped services.
- **Audit Schema Sync**: Mọi thay đổi về Audit Fields (created_at, updated_at, is_active) trong Entity phải được cập nhật tương ứng vào script khởi tạo SQLite trong bộ test.

---

## 📅 Chronological Logs

### [2026-04-28]
**Topic: Schema Synchronization & Audit Columns**
- **Type**: Bug Fix
- **Tags**: [testing] [sqlite] [sql] [audit]
- **Scope**: Integration Tests, DAL
- **Impact**: Loại bỏ hoàn toàn lỗi 500 (No such column/table) trong môi trường CI/CD và Integration Test.

**Bài học rút ra**:
- Script `CREATE TABLE` trong `CustomWebApplicationFactory.cs` phải là "Single Source of Truth" cho schema in-memory.
- Đảm bảo mapping `MatchNamesWithUnderscores = true` để đồng bộ PascalCase và snake_case.

---

### [2026-04-29]
**Topic: Search Optimization & Coding Style**
- **Type**: Architecture Decision / Best Practice
- **Tags**: [sql] [performance] [search] [coding-style]
- **Scope**: DAL, BLL (Service Layer)
- **Impact**: Tối ưu tốc độ tìm kiếm, tránh table scan trong production và tăng khả năng bảo trì mã nguồn.

**Bài học rút ra**:
- **Normalization**: Luôn truy vấn trên cột `normalized_name` đã được xử lý (lowercase, no accent). Tuyệt đối không dùng `LOWER()` trong `WHERE` để bảo toàn Database Index.
- **Explicit Logic**: Ưu tiên khối mã `{ ... }` và `Rationale` cho logic phức tạp. Tránh lạm dụng expression-bodied (`=>`) cho các tác vụ quan trọng.

---

### [2026-05-02]
**Topic: User Management, Mapster & Test Suite Refactor**
- **Type**: Architecture Decision / Best Practice
- **Tags**: [mapster] [user-management] [soft-delete] [testing] [api-response]
- **Scope**: API Layer, Service Layer, Integration Tests
- **Impact**: Thống nhất hành vi toàn hệ thống, bảo toàn dữ liệu giao dịch và tăng độ tin cậy (reliability) của bộ test lên 100%.

**Bài học rút ra**:
- **Soft Delete**: Luôn sử dụng `is_active = 0` cho các thực thể chứa lịch sử giao dịch (như User).
- **Mapster Pattern**: Duy trì cấu trúc 3 dòng (**Retrieve -> Adapt -> Save**) và cấu hình `IgnoreNullValues(true)`.
- **Exception Handling**: Chuyển đổi từ `BusinessException` sang `NotFoundException` cho các trường hợp 404 để API trả về status code chính xác.
- **ApiResponse Wrapper**: Trong Test, luôn sử dụng `ApiResponseTestWrapper<T>` để giải mã dữ liệu từ property `.Data`.
- **Verification Strategy**: Test case phải xác minh sự thay đổi thực tế trong Database (State Verification) thay vì chỉ kiểm tra HTTP Status Code.

---

### [2026-05-02]
**Topic: Documentation Standardization & Knowledge Management**
- **Type**: Best Practice
- **Tags**: [documentation] [workflow] [knowledge-management]
- **Scope**: Entire Project
- **Impact**: Tăng tính chuyên nghiệp, dễ dàng onboarding và bảo trì hệ thống tri thức dự án lâu dài.

**Bài học rút ra**:
- **Source Sync**: Tài liệu (README, memory, rules) phải luôn được đồng bộ hóa với thực tế codebase (kiểm tra .csproj, packages và cấu trúc filesystem) thay vì chỉ viết dựa trên giả định.
- **Semantic Naming**: Chuyển đổi naming convention từ PLAN-XXX sang PLAN-{topic}-{subtopic}.md giúp việc phân loại và tìm kiếm kế hoạch trở nên trực quan và có tính ngữ nghĩa cao hơn.
- **Elite Documentation Structure**: Việc áp dụng metadata (Type, Impact, Scope) vào nhật ký tri thức giúp định vị nhanh chóng phạm vi ảnh hưởng của các quyết định kiến trúc cũ.
- **Tone Balance**: Duy trì tông giọng kỹ thuật trung lập, súc tích (plain technical) giúp tài liệu trở nên đáng tin cậy và tập trung vào giá trị thực thi thay vì mô tả bóng bẩy.

**Tránh**:
- Tránh việc liệt kê các công nghệ không còn sử dụng (như SqlKata) trong tài liệu chính thức.
- Tránh đặt tên file kế hoạch theo số thứ tự đơn thuần, gây khó khăn khi số lượng task tăng lên.

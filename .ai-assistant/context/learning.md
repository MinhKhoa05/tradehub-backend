# Learning Log

Tài liệu này lưu vết các vấn đề gặp phải, bài học kinh nghiệm và các quyết định kiến trúc quan trọng trong quá trình phát triển GameTopUp.

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

---

### [2026-05-02]
**Topic: Exception Architecture Simplification**
- **Type**: Architecture Decision
- **Tags**: [exception] [refactor] [clean-code]
- **Scope**: BLL Exceptions
- **Impact**: Giảm sự phức tạp của hệ thống Exception, giúp việc ném lỗi trở nên linh hoạt và dễ bảo trì hơn.

**Bài học rút ra**:
- **Simplification over Structure**: Thay vì cố gắng cấu trúc hóa tham số cho NotFoundException (item, field, value), việc sử dụng một chuỗi message duy nhất giúp Agent và Developer dễ dàng tùy biến thông báo lỗi mà không bị ràng buộc bởi constructor phức tạp.
- **Unified Interface**: Tất cả các Business Exception (NotFound, Unauthorized, Forbidden) hiện đã có chung một mẫu constructor nhận message, giúp đồng bộ hóa cách xử lý tại Middleware.
- **Test Alignment**: Khi thay đổi cấu trúc Exception, bắt buộc phải cập nhật cả Unit Tests và Integration Tests để khớp với nội dung Message mới.

**Thay đổi**:
- NotFoundException constructor: (item, field, value) -> (message).
- Cập nhật toàn bộ Service gọi và Test assertions tương ứng.

---

### [2026-05-02]
**Topic: Database Indexing & Scope Discipline**
- **Type**: Best Practice
- **Tags**: [database] [indexing] [workflow]
- **Scope**: Entire Project
- **Impact**: Đảm bảo hiệu năng truy vấn tối ưu mà không phá vỡ cấu trúc dữ liệu hiện tại.

**Bài học rút ra**:
- **Scope Discipline**: Tuyệt đối không tự ý bổ sung cột mới (normalized_name) vào schema nếu không có trong Entity hiện tại, ngay cả khi nó tuân thủ quy tắc chung (rules.md). Việc này gây lỗi NOT NULL và làm hỏng bộ test.
- **Index-to-DAL Mapping**: Việc thiết lập Index phải dựa trên bằng chứng thực tế từ các câu lệnh WHERE và ORDER BY trong lớp DAL để đạt hiệu quả cao nhất.
- **Selectivity Principle**: Ưu tiên Composite Index với cột High Selectivity lên trước để Query Optimizer hoạt động hiệu quả.
- **Collateral Damage Awareness**: Một thay đổi nhỏ ở schema có thể ảnh hưởng đến toàn bộ các lớp (DAL Entities, BLL Services, Tests). Luôn đồng bộ hóa tất cả các lớp khi có thay đổi schema.

**Tránh**:
- Tránh thêm cột NOT NULL mà không cập nhật code khởi tạo tương ứng.
- Tránh đánh Index đơn lẻ trên các cột độ chọn lọc thấp (như is_active).

---

### [2026-05-02]
**Topic: Wallet Decoupling, Race Conditions & Service Isolation**
- **Type**: Architecture Decision
- **Tags**: [wallet] [refactor] [race-condition] [service-isolation] [usecase]
- **Scope**: Service Layer (BLL), Data Access Layer (DAL)
- **Impact**: Tách biệt hoàn toàn trách nhiệm giữa User và Wallet, đảm bảo an toàn dữ liệu trước các cuộc tấn công Race Condition và tăng tính module hóa của hệ thống.

**Bài học rút ra**:
- **Race Condition Awareness**: Luôn giả định Race Condition có thể xảy ra ngay cả khi đã có kiểm tra logic (Logic Check). Sử dụng `try-catch` để bắt lỗi vi phạm ràng buộc `UNIQUE`/`Duplicate` từ Database và chuyển đổi thành `BusinessException` thân thiện với người dùng.
- **Service Isolation (Data Ownership)**: Mỗi Service chỉ được phép truy cập vào Repository của chính nó ("dữ liệu của ai người đó giữ"). Tuyệt đối không inject Repository của thực thể khác vào Service.
- **UseCase Pattern**: Khi một nghiệp vụ cần tương tác giữa nhiều Service (ví dụ: tạo User và Wallet), phải sử dụng lớp `UseCase` hoặc `ApplicationService` để điều phối (orchestrate) thay vì gọi chéo giữa các Service.
- **Lazy Initialization (Manual)**: Việc tách biệt quy trình đăng ký tài khoản và kích hoạt ví giúp hệ thống linh hoạt hơn, cho phép trì hoãn việc cấp phát tài nguyên cho đến khi thực sự cần thiết.

---

- [2026-05-03] **Implemented Admin Order Completion**: Added `CompleteOrderAsync` with idempotency support and atomic DB updates. Enforced rules: Only assigned admin can complete "Processing" orders. Added audit logging.
- [2026-05-03] **Implemented Admin Order Cancellation**: Standardized idempotent cancellation flow with atomic check-and-act patterns. Added integration tests for race conditions.

---

### [2026-05-03]
**Topic: Admin Order Cancellation, Idempotency & API Semantics**
- **Type**: Architecture Decision / Best Practice
- **Tags**: [order-management] [idempotency] [api-design] [transaction]
- **Scope**: API Layer, Service Layer (BLL), UseCase
- **Impact**: Tăng tính ổn định của hệ thống trước các thao tác trùng lặp của Admin và chuẩn hóa giao tiếp API.

**Bài học rút ra**:
- **Idempotency (Tính giao hoán)**: Đối với các thao tác quan trọng như Hủy đơn/Hoàn tiền, hệ thống phải xử lý được các request trùng lặp (spam API). Nếu tài nguyên đã ở trạng thái mục tiêu (Cancelled), trả về thành công ngay lập tức thay vì báo lỗi.
- **API Semantics (POST vs DELETE)**: Sử dụng `POST` cho các hành động thay đổi trạng thái (state transition) thay vì `DELETE`. `DELETE` chỉ nên dùng khi thực sự xóa resource khỏi hệ thống.
- **Service Isolation Discipline**: Duy trì quy tắc UseCase chỉ gọi Service, Service chỉ gọi Repo của chính nó. Việc này giúp code testable hơn (có thể mock từng lớp) và giảm coupling.
- **Generic Transaction Mocking**: Khi mock `DatabaseContext` trong unit test, cần chú ý mock đủ cả các overload generic `Task<T>` và non-generic `Task` của `ExecuteInTransactionAsync` để tránh lỗi null reference khi thực thi service logic.

---

### [2026-05-03]
**Topic: Admin Order Completion, Status Constraints & Atomic Updates**
- **Type**: Implementation / Best Practice
- **Tags**: [order-management] [status-transition] [concurrency] [audit-log]
- **Scope**: DAL, BLL (OrderService), API
- **Impact**: Đảm bảo quy trình xử lý đơn hàng chặt chẽ, ngăn chặn việc Admin can thiệp vào đơn hàng của người khác và duy trì lịch sử trạng thái minh bạch.

**Bài học rút ra**:
- **Status Guardrails**: Khi chuyển trạng thái đơn hàng sang "Hoàn thành" (Completed), bắt buộc phải kiểm tra hai điều kiện song song: Trạng thái hiện tại phải là "Đang xử lý" (Processing) và người thực hiện phải là Admin đã nhận đơn (`AssignTo`).
- **Atomic Update Principle**: Sử dụng câu lệnh `UPDATE` kèm điều kiện `WHERE` chặt chẽ cho cả `status` và `assign_to` là cách tốt nhất để chống Race Condition mà không cần lock bảng phức tạp.
- **Idempotency in Success States**: Tương tự như Hủy đơn, nếu đơn hàng đã ở trạng thái Completed, việc gửi lại request hoàn thành phải trả về thành công (Idempotent) thay vì lỗi 400, giúp UX của Admin mượt mà hơn khi gặp sự cố mạng hoặc click đúp.
- **Audit Consistency**: Mỗi lần thay đổi trạng thái quan trọng (Pending -> Processing -> Completed) đều phải đi kèm with một bản ghi `OrderHistory`. Điều này cực kỳ quan trọng cho việc giải quyết tranh chấp và kiểm toán sau này.

---

### [2026-05-04]
**Topic: Strict Transaction Boundaries & Service Agnosticism**
- **Type**: Architecture Decision / Best Practice
- **Tags**: [transaction] [usecase] [service-layer] [clean-architecture]
- **Scope**: Entire System (BLL, UseCase)
- **Impact**: Tách biệt hoàn toàn trách nhiệm Orchestration và Business Logic, giúp Service dễ tái sử dụng, dễ test và ngăn ngừa các lỗi lồng ghép transaction phức tạp.

**Bài học rút ra**:
- **Transaction Responsibility**: Transaction MUST ONLY be created at the UseCase layer. UseCase chịu trách nhiệm mở transaction, đảm bảo tính nguyên tử (atomicity) và điều phối workflow.
- **Service Agnosticism**: Service layer MUST NOT create, commit, or rollback transactions. Service methods phải được thiết kế để không phụ thuộc vào vòng đời của transaction (transaction-agnostic) và giả định rằng chúng đang chạy trong một context transaction đã được UseCase mở sẵn.
- **Cohesive Service Flow**: Service chịu trách nhiệm hoàn toàn về Business Logic (validate, gọi repo, update entity, ghi log audit). UseCase không nên thao tác trực tiếp với Repository mà phải thông qua Service.
- **One-line Rule**: Transaction là responsibility của UseCase, Service chỉ thực thi business logic và không được phép kiểm soát lifecycle của transaction.

---

### [2026-05-04]
**Topic: The Evolution from Conditional Updates to Pessimistic Locking (`FOR UPDATE`)**
- **Type**: Architecture Evolution / Concurrency Strategy
- **Tags**: [concurrency] [pessimistic-lock] [conditional-update] [repository-pattern]
- **Scope**: DAL (Repository), BLL (UseCase/Service)
- **Impact**: Giữ cho Repository thuần túy (chỉ CRUD), gom toàn bộ logic nghiệp vụ về Service, và xử lý triệt để Race Conditions kèm side-effects (như hoàn tiền ví) mà không cần code phức tạp.

**Bài học rút ra từ quá trình Refactor**:
- **Sai lầm ban đầu (Conditional Updates)**: Ban đầu, hệ thống dùng Atomic Updates (Ví dụ: `UPDATE Orders SET Status = Processing WHERE Id = X AND Status = Pending`). 
  - *Hậu quả 1*: Phải đẻ ra hàng loạt hàm Repository cho từng trạng thái (`UpdateToProcessingIfPendingAsync`, `CancelIfPendingAsync`...). Repository bị phình to và chứa business logic.
  - *Hậu quả 2*: Khi `Update` trả về 0 dòng ảnh hưởng, UseCase không biết lý do (do đơn đã hủy? hay do Admin khác đã nhận?). Phải tốn thêm lượt query để check lại trạng thái nhằm xử lý Idempotency, gây rườm rà.
  - *Hậu quả 3*: Khó kết hợp với các side-effects chéo domain (như hoàn tiền Wallet).
- **Giải pháp tối ưu (Pessimistic Locking - `SELECT ... FOR UPDATE`)**: 
  - Thay vì để Database check điều kiện (qua mệnh đề `WHERE`), ta khóa cứng dòng dữ liệu ngay từ đầu bằng `LockAndGetByIdAsync` (gọi `FOR UPDATE` trong DAL).
  - Đưa toàn bộ việc check logic (`if status == ...`) về lớp Service bằng C# thuần.
  - Sau khi validation thành công trong bộ nhớ, chỉ cần gọi hàm `UpdateAsync(Order)` đơn giản của Repository.
- **Rules for Future Agents**:
  1. **Không dùng Conditional Updates cho nghiệp vụ phức tạp**: Tuyệt đối không tạo các hàm Update chứa logic trạng thái trong Repository. Repository chỉ nên có 1 hàm `UpdateAsync` thuần túy.
  2. **Luôn dùng `FOR UPDATE` cho luồng nhạy cảm phức tạp**: Tại UseCase, ngay sau khi mở Transaction đối với các nghiệp vụ có side-effect chéo (như hoàn tiền), BẮT BUỘC gọi `LockAndGetByIdAsync`.
  3. **Truyền object đã khóa**: Truyền nguyên object Entity đã khóa xuống Service. Service thực hiện business logic (check status, assign_to, throws Exception) và thay đổi state trực tiếp trên object đó.
  4. **Idempotency Synergy**: Với Pessimistic Lock, các check idempotency (`if (status == Cancelled) return;`) trở nên chính xác 100% tại thời điểm thực thi.
- **Trade-off (Khi nào KHÔNG NÊN lạm dụng `FOR UPDATE`)**: 
  - Đừng lạm dụng khóa bi quan nếu việc cập nhật chỉ là những thao tác đơn giản, nguyên tử trên 1 bảng duy nhất (Ví dụ: `UPDATE Users SET LastLogin = NOW() WHERE Id = X`).
  - Nếu nghiệp vụ không có side-effects phức tạp và Conditional Update (Atomic Update với mệnh đề `WHERE`) giải quyết tốt mà không làm rườm rà codebase, thì **VẪN NÊN DÙNG Conditional Update** để tối ưu hiệu năng và giảm nguy cơ deadlock. Pessimistic Locking chỉ tỏa sáng ở các luồng logic phức tạp cần Orchestration.

---

### [2026-05-06]
**Topic: Order Fulfillment Refactoring, Inventory Reservation & Direct Order Flow**
- **Type**: Architecture Decision / Business Logic Refactor
- **Tags**: [order-flow] [inventory-reservation] [clean-architecture] [direct-order]
- **Scope**: Order Domain (UseCase, Service, Controller, DTOs)
- **Impact**: Tối ưu hóa quy trình đặt hàng, đảm bảo tính công bằng trong giữ chỗ sản phẩm (inventory fairness) và đơn giản hóa API bằng cách loại bỏ cơ chế Giỏ hàng (Cart) không cần thiết.

**Bài học rút ra**:
- **Inventory Reservation (Giữ chỗ ngay)**: Thực hiện trừ tồn kho (`DecreaseStock`) ngay tại bước `PlaceOrder` (khi đơn hàng còn là `Pending`). Điều này đảm bảo khi người dùng đã đặt hàng thành công, họ chắc chắn sẽ có hàng khi thanh toán, tránh trường hợp thanh toán xong mới phát hiện hết hàng (Overselling).
- **Decoupled Payment Flow**: Tách biệt hoàn toàn `PlaceOrder` (tạo đơn + giữ kho) và `PayOrder` (trừ tiền ví + chuyển trạng thái `Paid`). Việc này giúp hệ thống linh hoạt hơn, cho phép người dùng đặt hàng trước và thanh toán sau (trong một khoảng thời gian quy định).
- **Symmetric Cancellation Logic**: Luồng hủy đơn (`CancelOrder`) phải đối xứng với luồng đặt hàng:
    - **Luôn hoàn kho**: Vì kho đã bị trừ ngay từ lúc đặt hàng.
    - **Hoàn tiền có điều kiện**: Chỉ hoàn tiền ví nếu đơn hàng đã ở trạng thái `Paid` hoặc `Processing`.
- **KISS (Keep It Simple, Stupid)**: Loại bỏ hoàn toàn cơ chế Giỏ hàng (`CheckoutRequest/Response`) và thay bằng đặt hàng trực tiếp (`PlaceOrderRequest`). Điều này giúp giảm độ phức tạp của logic, giảm số lượng DTO và làm API trở nên trực quan hơn cho Frontend.
- **UseCase Orchestration**: `OrderUseCase` đóng vai trò là nhạc trưởng điều phối giữa `OrderService` (trạng thái), `GamePackageService` (tồn kho) và `WalletService` (tiền tệ). Mỗi Service chỉ lo trách nhiệm đơn lẻ (Single Responsibility), giúp code dễ bảo trì và mở rộng.

**Thay đổi quan trọng**:
- Thay thế `CheckoutAsync` bằng `PlaceOrderAsync` nhận `PlaceOrderRequestDTO`.
- Chuyển logic trừ tồn kho từ bước xử lý Admin về bước `PlaceOrder` của người dùng.
- Thêm endpoint `POST /orders/{id}/pay` để người dùng chủ động thanh toán đơn hàng.
- Cập nhật `OrderUseCase.CancelOrderAsync` để tự động khôi phục tồn kho và hoàn tiền dựa trên trạng thái.
- Xóa bỏ các file DTO không còn sử dụng: `CheckoutRequest.cs`, `CheckoutResponseDTO.cs`.

---

### [2026-05-06]
**Topic: Wallet Concurrency, Audit Trail & Payment Flow Refactoring**
- **Type**: Architecture Decision / Infrastructure Upgrade
- **Tags**: [wallet] [concurrency] [audit-trail] [locking] [payment-flow]
- **Scope**: Entire System (BLL, UseCase, DAL, Tests)
- **Impact**: Đảm bảo an toàn tài chính, tính minh bạch trong đối soát số dư và chuẩn hóa quy trình thanh toán theo nghiệp vụ thực tế.

**Bài học rút ra**:
- **Atomic Balance Updates**: Không bao giờ cập nhật số dư ví bằng cách gán thủ công (`wallet.Balance = newBalance`). Bắt buộc sử dụng các hàm nguyên tử (`IncreaseBalanceAsync`/`DecreaseBalanceAsync`) tại Repository để tránh Race Condition ở mức độ Database.
- **Audit Trail (Balance Before/After)**: Mọi biến động số dư phải ghi lại trạng thái trước và sau giao dịch (`balance_before`, `balance_after`). Việc tính toán này phải được thực hiện trong Service và đồng bộ hóa lại đối tượng trong bộ nhớ (Memory Sync) sau khi update DB thành công để đảm bảo tính nhất quán dữ liệu cho các bước xử lý tiếp theo trong cùng một request.
- **Payment Flow Orchestration**: Quy trình thanh toán phải tuân thủ trình tự: **Lock Order -> Validate -> Lock Wallet -> Debit Wallet -> Mark Order Paid**. Tuyệt đối không cập nhật trạng thái đơn hàng sang `Paid` trước khi trừ tiền ví thành công.
- **UseCase Responsibility**: UseCase là nơi quyết định loại giao dịch (`WalletTransactionType`) và nội dung mô tả (`Description`). Service chỉ cung cấp các hàm trừ/nạp tiền nguyên tử (`DebitAsync`/`CreditAsync`) để đảm bảo tính tái sử dụng cao.
- **Integration Test Schema Sync**: Khi thay đổi schema Database (thêm/xóa cột), phải cập nhật ngay lập tức script khởi tạo SQLite trong `CustomWebApplicationFactory.cs` và các hàm seeding trong integration tests (ví dụ: `SeedGamePackageAsync`, `SeedOrderAsync`) để tránh lỗi mapping của Dapper.

**Thay đổi quan trọng**:
- Thêm cột `balance_before` vào bảng `wallet_transactions`.
- Tách `OrderService.PayOrderAsync` thành `ValidateForPayment` (Logic) và `MarkAsPaidAsync` (State update).
- Xóa bỏ các cột không sử dụng: `package_budget` (game_packages), `wallet_transaction_id` (orders).
- Đồng bộ hóa logic "Lock-And-Get" trên cả Order và Wallet trong các luồng thanh toán phức tạp.

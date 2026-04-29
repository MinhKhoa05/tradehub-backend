# Hệ thống TradeHub Backend - Tài liệu Kiến trúc

Tài liệu này mô tả chi tiết kiến trúc của hệ thống TradeHub (Backend), được thiết kế để hỗ trợ việc nạp tiền game và quản lý đơn hàng. Tài liệu này dành cho các AI Agent hoặc Developer muốn hiểu nhanh cấu trúc dự án.

## 1. Tổng quan hệ thống
- **Mục đích**: Hệ thống Backend cung cấp API cho việc quản lý người dùng, giỏ hàng, nạp tiền game (Game Recharge), thanh toán qua ví điện tử nội bộ và theo dõi đơn hàng.
- **Tech Stack chính**:
  - **Ngôn ngữ**: C# (.NET 8)
  - **ORM**: Dapper & Dommel (Micro-ORM để tối ưu hiệu năng)
  - **Database**: MySQL
  - **Authentication**: JWT (JSON Web Token)
- **Kiểu kiến trúc**: **Layered Architecture (Kiến trúc phân tầng)** kết hợp với các nguyên tắc **Clean Architecture** (tách biệt Concern giữa Domain và Infrastructure).

---

## 2. Cấu trúc thư mục & Layer

Hệ thống được chia thành 3 Project chính tương ứng với các Layer:

### 2.1 Presentation Layer (`TradeHub.API`)
- **Vai trò**: Cung cấp các RESTful API endpoints.
- **Thành phần**:
  - `Controllers/`: Tiếp nhận request, xử lý validate cơ bản và gọi xuống tầng BLL.
  - `Middlewares/`: Xử lý Global Exception, Logging.
  - `Filters/`: `ValidationFilter` để xử lý lỗi ModelView tự động.
  - `Program.cs`: Cấu hình DI, Middleware, Authentication và Database.

### 2.2 Application Layer (`TradeHub.BLL`)
- **Vai trò**: Chứa logic nghiệp vụ (Business Logic).
- **Thành phần**:
  - `Services/`: Các service xử lý logic cho từng Entity (Game, Cart, Wallet...).
  - `ApplicationServices/` (UseCases): Các luồng nghiệp vụ phức tạp phối hợp nhiều service (ví dụ `OrderUseCase`).
  - `DTOs/`: Data Transfer Objects để trao đổi dữ liệu với API.
  - `Common/`: Chứa các class dùng chung như `ServiceResult`, `UserContext`.

### 2.3 Data Access Layer (`TradeHub.DAL`)
- **Vai trò**: Quản lý dữ liệu và giao tiếp với Database.
- **Thành phần**:
  - `Entities/`: Các POCO class ánh xạ 1-1 với bảng trong DB.
  - `Repositories/`: Triển khai các câu lệnh SQL (Dapper) hoặc CRUD đơn giản (Dommel).
  - `DatabaseContext.cs`: Quản lý kết nối (`DbConnection`) và giao dịch (`DbTransaction`).

**Dependency Flow**: `TradeHub.API` -> `TradeHub.BLL` -> `TradeHub.DAL`

---

## 3. Luồng xử lý chính (Request Flow)

Một request thông thường sẽ đi qua các bước sau:
1. **Request** đến API Endpoint.
2. **Middleware/Filter**: Kiểm tra JWT, validate dữ liệu đầu vào (`ValidationFilter`).
3. **Controller**: Trích xuất thông tin người dùng vào `UserContext` và gọi Service/UseCase tương ứng.
4. **Service/UseCase**: Thực thi logic nghiệp vụ. Nếu có lỗi nghiệp vụ (ví dụ: không đủ tiền), ném `BusinessException`. 
5. **Repository**: Thực thi SQL query qua Dapper/Dommel bằng connection từ `DatabaseContext`.
6. **Response**: Nếu thành công, dữ liệu được trả về Controller để format thành `ApiResponse` qua các helper method (`ApiOk`, `ApiCreated`). Nếu thất bại, `GlobalExceptionMiddleware` bắt Exception và format thành `ApiResponse` lỗi.

---

## 4. Database & Data Access
- **Database**: MySQL.
- **Mapping Strategy**: 
  - DB dùng `snake_case` (ví dụ: `created_at`).
  - C# dùng `PascalCase` (ví dụ: `CreatedAt`).
  - Hệ thống tự động mapping thông qua `DefaultTypeMap.MatchNamesWithUnderscores = true` (Dapper) và `SnakeCaseResolver` (Dommel).
- **Entities**: Nằm trong `TradeHub.DAL.Entities`, kế toán đầy đủ các thuộc tính từ DB.

---

## 5. DatabaseContext & Unit of Work
Hệ thống không dùng EF Core mà triển khai một **Custom Unit of Work** thông qua `DatabaseContext`:

- **DatabaseContext**:
  - **Lifecycle**: `Scoped` (mỗi request một instance).
  - **Trách nhiệm**: Đảm bảo tất cả Repository trong cùng một request sử dụng chung một `DbConnection` và `DbTransaction`.
- **Transaction Management**:
  - Sử dụng phương thức `ExecuteInTransactionAsync(Func<Task> action)`.
  - Nếu `action` ném ngoại lệ, hệ thống tự động `Rollback`. Nếu thành công, tự động `Commit`.
- **Repository & Context**: Repositories nhận `DatabaseContext` qua DI và sử dụng các extension methods trong `DatabaseContextExtension.cs` để thực thi query.

---

## 6. Dependency Injection (DI)
- **Service Lifetime**: 
  - `DatabaseContext`: Scoped (Quan trọng để quản lý Transaction).
  - `Repositories`: Scoped.
  - `Services / UseCases`: Scoped.
- **Đăng ký**: Toàn bộ được đăng ký tập trung tại `Program.cs` trong project `TradeHub.API`.

---

## 7. Các pattern & nguyên tắc thiết kế
- **Repository Pattern**: Tách biệt logic truy vấn dữ liệu khỏi logic nghiệp vụ.
- **UseCase Pattern**: Đóng gói các luồng nghiệp vụ phức tạp (như Checkout) vào các class riêng biệt để tránh làm Service bị phình to.
- **Exception-based Flow**: Sử dụng `BusinessException` và các lớp kế thừa để thông báo lỗi nghiệp vụ. Điều này giúp code sạch hơn, tránh việc kiểm tra `if (result.IsSuccess)` lặp đi lặp lại.
- **Standard ApiResponse**: Sử dụng `ApiResponse` duy nhất để trả về cho Client, đảm bảo tính nhất quán giữa thành công và thất bại.
- **SOLID**: 
  - *Single Responsibility*: Mỗi Service/Repository chỉ làm một nhiệm vụ.
  - *Dependency Inversion*: Controllers phụ thuộc vào Abstraction (Service).

---

## 8. Cấu hình & môi trường
- **File cấu hình**: `appsettings.json` và `appsettings.Development.json`.
- **Các cấu hình quan trọng**:
  - `ConnectionStrings:Default`: Chuỗi kết nối MySQL.
  - `Jwt`: Cấu hình Issuer, Audience, Key cho Token.
  - `AllowedOrigins`: Cấu hình CORS.

---

## 9. Sơ đồ kiến trúc (Architecture Summary)

```text
[ Client (React App) ]
       |
       v (HTTP / JWT)
+-----------------------+
|     TradeHub.API      | <--- Middlewares, Filters, Controllers
+-----------------------+
       |
       v (Call)
+-----------------------+
|     TradeHub.BLL      | <--- UseCases, Services, DTOs
+-----------------------+
       |
       v (DI: DatabaseContext)
+-----------------------+
|     TradeHub.DAL      | <--- Repositories, Entities, DBContext
+-----------------------+
       |
       v (SQL / Dapper)
[ MySQL Database ]
```

**Ghi chú cho AI Agent**:
- Khi cần thêm logic nghiệp vụ, hãy sửa ở `TradeHub.BLL`.
- Khi cần thêm bảng hoặc câu lệnh SQL, hãy sửa ở `TradeHub.DAL`.
- Luôn sử dụng `UserContext` để lấy ID người dùng hiện tại trong tầng BLL/API.
- Mọi thao tác thay đổi dữ liệu liên quan đến nhiều bảng PHẢI được bọc trong `ExecuteInTransactionAsync`.

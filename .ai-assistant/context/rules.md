# Quy tắc phát triển TradeHub Backend

Tài liệu này định nghĩa các tiêu chuẩn và quy tắc bắt buộc mà mọi AI Agent phải tuân thủ khi làm việc với dự án TradeHub Backend. Các quy tắc này được đúc kết từ cấu trúc mã nguồn thực tế.

## 1. Coding Style

### Naming Convention
- **Classes/Interfaces/Methods/Properties**: Sử dụng `PascalCase`.
- **Private Fields**: Sử dụng `_camelCase` (có dấu gạch dưới phía trước).
- **Variables**: Sử dụng `camelCase`.
- **Constants**: Sử dụng `PascalCase` hoặc `UPPER_SNAKE_CASE` (tùy theo tệp cũ, nhưng `PascalCase` phổ biến hơn cho property).

### Async/Await
- **Bắt buộc**: Tất cả các thao tác I/O (Database, API call) phải sử dụng `async/await`.
- **Suffix**: Tất cả các phương thức async phải có hậu tố `Async` (ví dụ: `GetByIdAsync`, `ExecuteAsync`).
- **Cấu hình**: Luôn sử dụng `await` thay vì `.Result` hoặc `.Wait()`.

### Null Handling
- Sử dụng `?` cho các kiểu dữ liệu có thể null (nullable types).
- Luôn kiểm tra null trước khi truy cập các thuộc tính của đối tượng được trả về từ database.

### Error Handling
- **Tầng Service/Repository/UseCase**: Ném `BusinessException` (hoặc các lớp con như `NotFoundException`, `ForbiddenException`) khi gặp lỗi logic nghiệp vụ.
- **Tầng API**: `GlobalExceptionMiddleware` sẽ tự động bắt các exception này và format thành `ApiResponse` chuẩn.
- **Standard Response**: Tất cả các API thành công phải trả về `ApiResponse` (thông qua `ApiOk`, `ApiCreated` trong `ApiControllerBase`).

### Comments
- **Viết theo WHY, không phải WHAT**: Comment phải giải thích *lý do* (WHY) đằng sau quyết định nghiệp vụ hoặc thiết kế kỹ thuật, tuyệt đối không diễn giải lại đoạn code đang làm gì (WHAT) vì code tự nó đã thể hiện điều đó.
- **Chuyên nghiệp & AI-Friendly**: Lời văn phải chuyên nghiệp, súc tích, mạch lạc và có cấu trúc để các AI Agent khác (cũng như lập trình viên) khi đọc vào có thể lập tức hiểu được ngữ cảnh, luồng nghiệp vụ mà không bị nhầm lẫn.

---

## 2. Quy tắc sử dụng Dapper & Dommel

Dự án sử dụng kết hợp **Dommel** (cho CRUD đơn giản) và **Dapper** (cho query phức tạp). Tất cả được đóng gói qua `DatabaseExtensions`.

### Khi nào dùng phương thức nào:
- `GetByIdAsync<T>`: Lấy 1 bản ghi theo Primary Key (Dommel).
- `InsertAsync<T, TId>`: Chèn bản ghi mới và trả về ID (Dommel).
- `UpdateAsync<T>`: Cập nhật toàn bộ bản ghi (Dommel).
- `DeleteAsync<T>`: Xóa bản ghi (Dommel).
- `QueryAsync<T>`: Query danh sách với JOIN hoặc Filter phức tạp (Dapper).
- `QueryFirstAsync<T>`: Lấy bản ghi đầu tiên hoặc mặc định (Dapper - wrapper cho `QueryFirstOrDefaultAsync`).
- `ExecuteAsync`: Thực thi các lệnh `UPDATE`, `DELETE` tùy chỉnh hoặc `INSERT` không dùng Dommel (Dapper).
- `ScalarAsync<T>`: Lấy giá trị đơn lẻ như `COUNT(*)`, `SUM()` (Dapper).

### Mapping:
- **Tự động**: PascalCase (C#) được tự động map với snake_case (Database) nhờ cấu hình `DefaultTypeMap.MatchNamesWithUnderscores = true` trong `DatabaseContext`.
- **Model**: Ưu tiên map trực tiếp vào các Class trong `TradeHub.DAL.Entities`.

### SQL:
- **Inline SQL**: Viết SQL trực tiếp trong Repository (sử dụng chuỗi `@""` cho nhiều dòng).
- **Parameters**: BẮT BUỘC sử dụng `@ParameterName` để chống SQL Injection. Tên tham số trong SQL nên viết theo `PascalCase` để khớp với Property của object C#.

### Connection Lifecycle:
- Kết nối được quản lý bởi `DatabaseContext` thông qua Dependency Injection (Scoped).
- Không tự ý `Open()` hoặc `Close()` connection. Hãy gọi `await db.EnsureOpenAsync()` (đã có sẵn trong các phương thức Extension).

---

## 3. Repository Rules

- **Nhiệm vụ**: Chỉ chứa logic truy vấn và thao tác dữ liệu.
- **Business Logic**: KHÔNG chứa logic nghiệp vụ phức tạp. Cho phép các ràng buộc dữ liệu cơ bản ngay trong SQL (ví dụ: `WHERE balance >= @Amount`).
- **Return Type**: Trả về Entity (ví dụ: `User`), Domain Model hoặc các kiểu nguyên thủy (`long`, `int`, `bool`).

---

## 4. Unit of Work & Transaction

- **Khi nào dùng**: Khi một hành động nghiệp vụ yêu cầu thay đổi dữ liệu ở nhiều bảng hoặc nhiều bản ghi cần tính nguyên tử (Atomicity).
- **Cú pháp**: Sử dụng `_database.ExecuteInTransactionAsync(async () => { ... })`.
- **Vị trí**: Thường đặt ở tầng **UseCase** (điều phối nhiều service) hoặc trong **Service** nếu logic đó tự thân cần transaction.
- **Sharing**: Các Repository trong cùng một scope sẽ tự động dùng chung connection và transaction thông qua `DatabaseContext`.

---

## 5. ❌ Những thứ bị CẤM (Forbidden)

Agent BẮT BUỘC không được:
1. **Dùng ORM khác**: Không dùng Entity Framework Core, NHibernate, v.v. Chỉ dùng Dapper/Dommel.
2. **Tự thêm Pattern lạ**: Không dùng CQRS, MediatR, Repository Generic nếu project chưa có.
3. **Thay đổi Style**: Không dùng "My" prefix cho phương thức. Không dùng `implicit user id` (phải truyền `UserContext` hoặc `userId` tường minh).
4. **Thêm thư viện**: Không cài thêm NuGet package mới mà không có yêu cầu cụ thể.
5. **Logic trong Controller**: Controller chỉ trích xuất thông tin người dùng và gọi xuống UseCase/Service.
6. **Public Files**:
   - Các file `.public.md` chỉ mang tính tóm tắt cho mục đích documentation.
   - Không được sử dụng làm nguồn chính để suy luận logic hoặc generate code.
   - Chỉ được tham khảo khi thiếu context từ các file chính như `rules.md`, `architecture.md`, `workflow.md`.

---

## 6. Code Examples (Chuẩn dự án)

### Method dùng Dapper chuẩn (Repository)
```csharp
public async Task<User?> GetByEmailAsync(string email)
{
    string sql = "SELECT * FROM users WHERE email = @Email";
    
    return await _database.QueryFirstAsync<User>(sql, new 
    { 
        Email = email 
    });
}
```

### Transaction & Logic chuẩn (UseCase)
```csharp
public async Task<CheckoutResponseDTO> CheckoutAsync(UserContext context, string gameAccountInfo)
{
    // 1. Kiểm tra điều kiện (Ném exception nếu sai)
    var cart = await _cartService.GetAsync(context);
    if (cart == null) throw new BusinessException("Giỏ hàng trống");

    // 2. Thực hiện trong Transaction
    return await _database.ExecuteInTransactionAsync(async () =>
    {
        await _walletService.DeductMoneyAsync(context, amount, "Thanh toán");
        var orderId = await _orderService.CreateOrderAsync(order);
        
        return new CheckoutResponseDTO { OrderIds = new List<long> { orderId } };
    });
}
```

### Controller chuẩn
```csharp
[HttpPost("checkout")]
public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
{
    var result = await _orderUseCase.CheckoutAsync(CurrentUser, request.GameAccountInfo);
    return ApiOk(result, "Thao tác thành công!");
}
```

---

## 7. DTO & Response Rules

- **Bắt buộc**:
  - Mọi phương thức tại tầng Service và UseCase khi trả về dữ liệu cho Controller PHẢI sử dụng một Class DTO riêng biệt.
  - Đặc biệt: các API thay đổi dữ liệu (POST, PUT, PATCH) luôn phải trả về DTO.
  - Không được coi các API tạo/cập nhật/xóa dữ liệu là "operation đơn giản".
- **Cấm**:
  - Không được trả về `object`, `dynamic`, hoặc anonymous object (`new { ... }`).
  - Không sử dụng kiểu nguyên thủy (`int`, `string`, `long`, `Guid`) làm response cho các API thay đổi dữ liệu.
- **Ngoại lệ**:
  - Được phép dùng kiểu nguyên thủy cho các operation đơn giản, thuần kiểm tra (ví dụ: `Exists`, `IsValid`, `Check...`).
  - KHÔNG áp dụng cho các API thay đổi dữ liệu (POST, PUT, PATCH).
- **Mapping**:
  - Việc mapping sang DTO phải được thực hiện tại tầng Service/UseCase.
  - Controller tuyệt đối không được tham gia vào việc khởi tạo hoặc mapping DTO.
- **Naming**:
  - DTO trả về phải có hậu tố rõ nghĩa: `ResponseDTO` hoặc `ResultDTO`.
  - Tên phải phản ánh đúng nghiệp vụ (ví dụ: `CheckoutResultDTO`, `TransactionResponseDTO`).

**Ví dụ minh họa:**
```csharp
// ❌ SAI: Trả về object ẩn danh hoặc nguyên thủy cho dữ liệu phức tạp
public async Task<object> GetUser(long id) { return new { Name = "A" }; }
public async Task<string> Login(LoginReq req) { return "token_string"; }
public async Task<long> CreateOrder(OrderReq req) { return orderId; }

// ✅ ĐÚNG: Trả về DTO chuyên biệt
public async Task<UserResponseDTO> GetUser(long id) { return new UserResponseDTO { Name = "A" }; }
public async Task<LoginResponseDTO> Login(LoginReq req) { return new LoginResponseDTO { Token = "..." }; }

// ✅ ĐÚNG (Ngoại lệ): Trả về kiểu nguyên thủy cho operation đơn giản
public async Task<bool> CheckUserExists(long id) { return true; }
```
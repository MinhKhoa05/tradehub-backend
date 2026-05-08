using Xunit;

// Lý do:
// SQLite In-Memory Database cực nhanh nhưng rất nhạy cảm với các thay đổi cấu trúc đồng thời.
// Vì chúng ta luôn xóa toàn bộ dữ liệu trước MỖI test (thông qua InitializeAsync -> ResetDatabaseAsync),
// việc chạy song song các test class sẽ gây mất dữ liệu nghiêm trọng giữa chừng (ví dụ: Test B xóa DB
// ngay sau khi Test A seed dữ liệu), dẫn đến các lỗi không ổn định như 404 hoặc NullReferenceException
// trong các test runner như Visual Studio Test Explorer.
// Attribute cấp assembly này sẽ vô hiệu hóa hoàn toàn việc chạy song song test,
// đảm bảo các test chạy tuần tự an toàn tuyệt đối.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace GameTopUp.Tests.IntegrationTests
{
    // Lý do:
    // Việc khởi tạo WebHost, DI container và giữ kết nối SQLite in-memory dùng chung là khá tốn tài nguyên.
    // Bằng cách định nghĩa một collection duy nhất và cho tất cả Integration Tests dùng chung thông qua
    // [Collection("IntegrationTests")], xUnit đảm bảo CustomWebApplicationFactory chỉ được khởi tạo DUY NHẤT MỘT LẦN
    // cho toàn bộ bộ test.
    // Điều này giúp giảm đáng kể thời gian setup test và tiết kiệm tài nguyên hệ thống.
    [CollectionDefinition("IntegrationTests")]
    public class SharedTestCollection : ICollectionFixture<CustomWebApplicationFactory<Program>>
    {
        /*
         * THÀNH PHẦN                             | NHIỆM VỤ
         * --------------------------------------|---------------------------------------
         * CollectionBehavior (tắt chạy song song) | Chạy tuần tự, tránh xung đột dữ liệu.
         * CollectionDefinition                   | Dùng chung 1 WebFactory (giảm tốn RAM).
         * IAsyncLifetime (ResetDatabase)         | Mỗi [Fact] bắt đầu với database sạch.
         */
    }
}
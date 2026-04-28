namespace TradeHub.BLL.Common
{
    /// <summary>
    /// UserContext lưu trữ thông tin định danh của người dùng hiện tại trong luồng xử lý.
    /// Sử dụng UserContext thay vì truy cập trực tiếp vào Claims qua IdentityService để đảm bảo 
    /// logic nghiệp vụ có thể tái sử dụng linh hoạt trong nhiều môi trường khác nhau như:
    /// - HTTP Request (từ Controller)
    /// - Background Jobs (không có HttpContext)
    /// - Admin Dashboard hoặc Unit Tests
    /// </summary>
    public class UserContext
    {
        public long UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public UserContext()
        {
        }

        public UserContext(long userId, string username, string role)
        {
            UserId = userId;
            Username = username;
            Role = role;
        }
    }
}

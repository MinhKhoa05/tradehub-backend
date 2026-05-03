using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using GameTopUp.BLL.Config;

namespace GameTopUp.API.Extensions
{
    /// <summary>
    /// Các phương thức mở rộng để cấu hình cơ chế xác thực JWT.
    /// Việc tách ra Extension giúp file Program.cs gọn gàng và dễ bảo trì hơn.
    /// </summary>
    public static class JwtExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Cấu hình các tham số để xác thực tính hợp lệ của Token.
                options.TokenValidationParameters = GetTokenValidationParameters(jwtSettings!);

                // Đăng ký các sự kiện tùy chỉnh (ví dụ: lấy Token từ Cookie).
                options.Events = GetJwtBearerEvents();
            });

            return services;
        }

        private static TokenValidationParameters GetTokenValidationParameters(JwtSettings jwtSettings)
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                
                // ClockSkew = Zero để đảm bảo Token hết hạn ngay lập tức khi đến thời điểm Expire,
                // tránh việc Token vẫn có hiệu lực thêm vài phút mặc định của Server.
                ClockSkew = TimeSpan.Zero
            };
        }

        private static JwtBearerEvents GetJwtBearerEvents()
        {
            return new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Hỗ trợ lấy Token từ cả Header (mặc định) và HttpOnly Cookie.
                    // Việc dùng Cookie giúp tăng khả năng bảo mật chống tấn công XSS.
                    var token = context.Request.Cookies["accessToken"];
                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Token = token;
                    }
                    return Task.CompletedTask;
                },

                OnChallenge = async context =>
                {
                    // Tùy chỉnh phản hồi khi người dùng truy cập tài nguyên yêu cầu đăng nhập nhưng chưa có Token.
                    context.HandleResponse();
                    await WriteErrorResponse(context.HttpContext, StatusCodes.Status401Unauthorized, "Yêu cầu đăng nhập để thực hiện hành động này.");
                },

                OnAuthenticationFailed = async context =>
                {
                    // Xử lý khi Token bị sai định dạng, bị sửa đổi hoặc đã hết hạn.
                    await WriteErrorResponse(context.HttpContext, StatusCodes.Status401Unauthorized, "Phiên đăng nhập không hợp lệ hoặc đã hết hạn.");
                },

                OnForbidden = async context =>
                {
                    // Xử lý khi người dùng không có quyền truy cập endpoint có phân quyền
                    await WriteErrorResponse(context.HttpContext, StatusCodes.Status403Forbidden, "Bạn không có quyền truy cập tài nguyên này.");
                }
            };
        }

        private static async Task WriteErrorResponse(HttpContext context, int statusCode,  string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            
            // Trả về cấu trúc lỗi thống nhất với ApiResponse của hệ thống.
            await context.Response.WriteAsJsonAsync(new 
            { 
                success = false,
                message = message 
            });
        }
    }
}

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TradeHub.BLL.Configurations;

namespace TradeHub.API.Extensions
{
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
                // Tách 1: Cấu hình tham số validation
                options.TokenValidationParameters = GetTokenValidationParameters(jwtSettings!);

                // Tách 2: Cấu hình các sự kiện (Events)
                options.Events = GetJwtBearerEvents();
            });

            return services;
        }

        // --- HELPER STATIC METHODS ---

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
                ClockSkew = TimeSpan.Zero
            };
        }

        private static JwtBearerEvents GetJwtBearerEvents()
        {
            return new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var token = context.Request.Cookies["accessToken"];
                    if (!string.IsNullOrEmpty(token)) context.Token = token;
                    return Task.CompletedTask;
                },

                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    await WriteErrorResponse(context.HttpContext, "Bạn chưa đăng nhập");
                },

                OnAuthenticationFailed = async context =>
                {
                    await WriteErrorResponse(context.HttpContext, "Token không hợp lệ hoặc đã hết hạn");
                }
            };
        }

        private static async Task WriteErrorResponse(HttpContext context, string message)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { message });
        }
    }
}
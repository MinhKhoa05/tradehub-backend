using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using TradeHub.BLL.Configurations;
using TradeHub.BLL.DTOs.Auths;

namespace TradeHub.BLL.Common
{
    /// <summary>
    /// TokenService quản lý việc tạo định danh điện tử (JWT) cho người dùng.
    /// JWT được sử dụng để duy trì trạng thái đăng nhập một cách Stateless, giúp hệ thống 
    /// dễ dàng mở rộng và giảm tải cho Database.
    /// </summary>
    public class TokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly SymmetricSecurityKey _securityKey;
        private readonly JwtSecurityTokenHandler _tokenHandler = new();

        public TokenService(IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;

            _securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Key));
        }

        /// <summary>
        /// Tạo Access Token dựa trên thông tin định danh của người dùng.
        /// Các Claims được nhúng vào Token để các tầng khác có thể trích xuất thông tin
        /// mà không cần truy vấn lại Database.
        /// </summary>
        public string GenerateAccessToken(TokenRequest tokenRequest)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, tokenRequest.UserId.ToString()),
                new Claim(ClaimTypes.Name, tokenRequest.Name),
                new Claim(JwtRegisteredClaimNames.Email, tokenRequest.Email),
                new Claim(ClaimTypes.Role, tokenRequest.Role),

                // JTI giúp định danh duy nhất mỗi Token, hỗ trợ việc Revoke Token nếu cần
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            var credentials = new SigningCredentials(
                _securityKey,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
                signingCredentials: credentials
            );

            return _tokenHandler.WriteToken(token);
        }
    }
}
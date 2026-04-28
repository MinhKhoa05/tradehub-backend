using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using TradeHub.BLL.Configurations;
using TradeHub.BLL.DTOs.Auths;

namespace TradeHub.BLL.Common
{
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

        public string GenerateAccessToken(TokenRequest tokenRequest)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, tokenRequest.UserId.ToString()),
            new Claim(ClaimTypes.Name, tokenRequest.Name),
            new Claim(JwtRegisteredClaimNames.Email, tokenRequest.Email),
            new Claim(ClaimTypes.Role, tokenRequest.Role),

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
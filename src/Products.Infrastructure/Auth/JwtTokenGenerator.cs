using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Products.Infrastructure.Auth;

public class JwtTokenGenerator
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenMinutes;
    private readonly int _refreshTokenDays;

    public JwtTokenGenerator(string secret, string issuer, string audience, int accessTokenMinutes = 15, int refreshTokenDays = 7)
    {
        _secret = secret;
        _issuer = issuer;
        _audience = audience;
        _accessTokenMinutes = accessTokenMinutes;
        _refreshTokenDays = refreshTokenDays;
    }

    public (string Token, DateTime ExpiresAt) GenerateAccessToken(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(_accessTokenMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expiresAt);
    }

    public (string Token, DateTime ExpiresAt) GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var token = Convert.ToBase64String(randomBytes);
        var expiresAt = DateTime.UtcNow.AddDays(_refreshTokenDays);
        return (token, expiresAt);
    }

    // Keep backward compat for tests
    public (string Token, DateTime ExpiresAt) GenerateToken(string username) => GenerateAccessToken(username);
}

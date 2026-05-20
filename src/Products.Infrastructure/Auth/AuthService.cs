using System.Collections.Concurrent;
using Products.Application.DTOs;
using Products.Application.Interfaces;

namespace Products.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private const string DemoUsername = "admin";
    private const string DemoPassword = "password123";

    private readonly JwtTokenGenerator _tokenGenerator;

    private static readonly ConcurrentDictionary<string, (string Username, DateTime ExpiresAt)> _refreshTokens = new();

    public AuthService(JwtTokenGenerator tokenGenerator)
    {
        _tokenGenerator = tokenGenerator;
    }

    public LoginResponse Authenticate(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (!string.Equals(request.Username, DemoUsername, StringComparison.OrdinalIgnoreCase) ||
            request.Password != DemoPassword)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        return GenerateTokens(request.Username);
    }

    public LoginResponse RefreshToken(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        if (!_refreshTokens.TryRemove(refreshToken, out var stored))
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        if (stored.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token expired.");
        }

        return GenerateTokens(stored.Username);
    }

    public void RevokeRefreshToken(string refreshToken)
    {
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            _refreshTokens.TryRemove(refreshToken, out _);
        }
    }

    private LoginResponse GenerateTokens(string username)
    {
        var (accessToken, accessExpiresAt) = _tokenGenerator.GenerateAccessToken(username);
        var (refreshToken, refreshExpiresAt) = _tokenGenerator.GenerateRefreshToken();

        _refreshTokens[refreshToken] = (username, refreshExpiresAt);

        // Cleanup expired tokens periodically
        CleanupExpiredTokens();

        return new LoginResponse(accessToken, accessExpiresAt, refreshToken, refreshExpiresAt);
    }

    private static void CleanupExpiredTokens()
    {
        var now = DateTime.UtcNow;
        foreach (var kvp in _refreshTokens)
        {
            if (kvp.Value.ExpiresAt < now)
            {
                _refreshTokens.TryRemove(kvp.Key, out _);
            }
        }
    }
}

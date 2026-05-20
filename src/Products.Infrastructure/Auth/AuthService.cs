using System.Collections.Concurrent;
using Products.Application.DTOs;
using Products.Application.Interfaces;
using Products.Domain.Entities;

namespace Products.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly JwtTokenGenerator _tokenGenerator;
    private readonly IUserRepository _userRepository;

    // In-memory refresh token store: token -> (username, expiresAt)
    private static readonly ConcurrentDictionary<string, (string Username, DateTime ExpiresAt)> _refreshTokens = new();

    public AuthService(JwtTokenGenerator tokenGenerator, IUserRepository userRepository)
    {
        _tokenGenerator = tokenGenerator;
        _userRepository = userRepository;
    }

    public async Task<LoginResponse> AuthenticateAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var user = await _userRepository.GetByUsernameAsync(request.Username, ct);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        return GenerateTokens(user.Username);
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length < 3)
        {
            throw new ArgumentException("Username must be at least 3 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            throw new ArgumentException("Password must be at least 6 characters.");
        }

        var exists = await _userRepository.ExistsAsync(request.Username, ct);
        if (exists)
        {
            throw new ArgumentException("Username is already taken.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User(request.Username.ToLower(), passwordHash);
        await _userRepository.AddAsync(user, ct);

        return GenerateTokens(user.Username);
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

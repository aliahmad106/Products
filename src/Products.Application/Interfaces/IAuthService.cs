using Products.Application.DTOs;

namespace Products.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> AuthenticateAsync(LoginRequest request, CancellationToken ct = default);
    Task<LoginResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    LoginResponse RefreshToken(string refreshToken);
    void RevokeRefreshToken(string refreshToken);
}

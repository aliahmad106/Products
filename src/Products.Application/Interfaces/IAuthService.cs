using Products.Application.DTOs;

namespace Products.Application.Interfaces;

public interface IAuthService
{
    LoginResponse Authenticate(LoginRequest request);
    LoginResponse RefreshToken(string refreshToken);
    void RevokeRefreshToken(string refreshToken);
}

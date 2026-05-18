using Products.Application.DTOs;
using Products.Application.Interfaces;

namespace Products.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private const string DemoUsername = "admin";
    private const string DemoPassword = "password123";

    private readonly JwtTokenGenerator _tokenGenerator;

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

        var (token, expiresAt) = _tokenGenerator.GenerateToken(request.Username);
        return new LoginResponse(token, expiresAt);
    }
}

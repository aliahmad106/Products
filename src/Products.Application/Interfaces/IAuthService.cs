using Products.Application.DTOs;

namespace Products.Application.Interfaces;

public interface IAuthService
{
    LoginResponse Authenticate(LoginRequest request);
}

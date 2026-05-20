namespace Products.Application.DTOs;

public record LoginResponse(string Token, DateTime ExpiresAt, string RefreshToken, DateTime RefreshTokenExpiresAt);

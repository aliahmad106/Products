namespace Products.Application.DTOs;

public record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string Colour,
    DateTime CreatedAt
);

namespace Products.Application.DTOs;

public record CreateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    string Colour
);

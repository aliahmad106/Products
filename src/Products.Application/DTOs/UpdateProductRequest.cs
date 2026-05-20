namespace Products.Application.DTOs;

public record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    string Colour
);

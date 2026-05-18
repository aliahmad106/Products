using Products.Application.DTOs;

namespace Products.Application.Interfaces;

public interface IProductService
{
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ProductResponse>> GetAllAsync(string? colour, CancellationToken ct = default);
}

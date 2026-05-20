using Products.Application.DTOs;

namespace Products.Application.Interfaces;

public interface IProductService
{
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ProductResponse>> GetAllAsync(string? colour, CancellationToken ct = default);
    Task<PagedResponse<ProductResponse>> GetPagedAsync(string? colour, int page, int pageSize, CancellationToken ct = default);
    Task<ProductResponse?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}

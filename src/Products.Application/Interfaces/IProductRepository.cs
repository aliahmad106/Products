using Products.Domain.Entities;

namespace Products.Application.Interfaces;

public interface IProductRepository
{
    Task<Product> AddAsync(Product product, CancellationToken ct = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetByColourAsync(string colour, CancellationToken ct = default);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(string? colour, int page, int pageSize, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}

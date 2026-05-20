using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Products.Application.Interfaces;
using Products.Domain.Entities;
using Products.Infrastructure.Data;

namespace Products.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductsDbContext _context;
    private readonly ResiliencePipeline _retryPipeline;

    public ProductRepository(ProductsDbContext context)
    {
        _context = context;
        _retryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromMilliseconds(200),
                ShouldHandle = new PredicateBuilder().Handle<DbUpdateException>()
            })
            .Build();
    }

    public async Task<Product> AddAsync(Product product, CancellationToken ct = default)
    {
        return await _retryPipeline.ExecuteAsync(async token =>
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync(token);
            return product;
        }, ct);
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Products.FindAsync(new object[] { id }, ct);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Products
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetByColourAsync(string colour, CancellationToken ct = default)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.Colour.ToLower() == colour.ToLower())
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        string? colour, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(colour))
        {
            query = query.Where(p => p.Colour.ToLower() == colour.ToLower());
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _context.Products.FindAsync(new object[] { id }, ct);
        if (product == null) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}

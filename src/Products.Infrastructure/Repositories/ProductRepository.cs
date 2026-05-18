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

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
    {
        return await _retryPipeline.ExecuteAsync(async token =>
        {
            return (IReadOnlyList<Product>)await _context.Products
                .AsNoTracking()
                .ToListAsync(token);
        }, ct);
    }

    public async Task<IReadOnlyList<Product>> GetByColourAsync(string colour, CancellationToken ct = default)
    {
        return await _retryPipeline.ExecuteAsync(async token =>
        {
            return (IReadOnlyList<Product>)await _context.Products
                .AsNoTracking()
                .Where(p => p.Colour.ToLower() == colour.ToLower())
                .ToListAsync(token);
        }, ct);
    }
}

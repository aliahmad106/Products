using FluentValidation;
using Products.Application.DTOs;
using Products.Application.Interfaces;
using Products.Domain.Entities;

namespace Products.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IValidator<CreateProductRequest> _validator;

    public ProductService(IProductRepository repository, IValidator<CreateProductRequest> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var product = new Product(request.Name, request.Description, request.Price, request.Colour);
        var created = await _repository.AddAsync(product, ct);

        return MapToResponse(created);
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(id, ct);
        return product == null ? null : MapToResponse(product);
    }

    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(string? colour, CancellationToken ct = default)
    {
        var products = string.IsNullOrWhiteSpace(colour)
            ? await _repository.GetAllAsync(ct)
            : await _repository.GetByColourAsync(colour, ct);

        return products.Select(MapToResponse).ToList().AsReadOnly();
    }

    public async Task<PagedResponse<ProductResponse>> GetPagedAsync(string? colour, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, totalCount) = await _repository.GetPagedAsync(colour, page, pageSize, ct);
        var responses = items.Select(MapToResponse).ToList().AsReadOnly();

        return new PagedResponse<ProductResponse>(responses, totalCount, page, pageSize);
    }

    public async Task<ProductResponse?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(id, ct);
        if (product == null) return null;

        product.Update(request.Name, request.Description, request.Price, request.Colour);
        await _repository.UpdateAsync(product, ct);

        return MapToResponse(product);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return await _repository.DeleteAsync(id, ct);
    }

    private static ProductResponse MapToResponse(Product product)
    {
        return new ProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Colour,
            product.CreatedAt
        );
    }
}

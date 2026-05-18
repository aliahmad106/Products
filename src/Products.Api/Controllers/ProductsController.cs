using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Products.Application.DTOs;
using Products.Application.Interfaces;

namespace Products.Api.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var product = await _productService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), null, product);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? colour, CancellationToken ct)
    {
        var products = await _productService.GetAllAsync(colour, ct);
        return Ok(products);
    }
}

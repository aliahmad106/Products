using System.Net;
using System.Net.Http.Json;
using FsCheck;
using FsCheck.Xunit;
using Products.Application.DTOs;
using Products.PropertyTests.Generators;
using Xunit;

namespace Products.PropertyTests;

/// <summary>
/// Feature: products-web-api, Property 3: Valid product creation returns 201 with ID
/// Validates: Requirements 3.1
/// </summary>
public class ProductCreationProperties : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;

    public ProductCreationProperties(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Property(MaxTest = 100, Arbitrary = new[] { typeof(ProductGenerators) })]
    public Property ValidProductCreation_Returns201WithId(CreateProductRequest request)
    {
        // Only test with valid products from our generator
        if (string.IsNullOrWhiteSpace(request.Name) || request.Price < 0 || string.IsNullOrWhiteSpace(request.Colour))
            return true.ToProperty();

        var token = _fixture.GetTokenAsync().GetAwaiter().GetResult();
        _fixture.SetAuth(token);

        var response = _fixture.Client.PostAsJsonAsync("/api/products", request).GetAwaiter().GetResult();
        var product = response.Content.ReadFromJsonAsync<ProductResponse>().GetAwaiter().GetResult();

        return (response.StatusCode == HttpStatusCode.Created)
            .Label("Should return 201 Created")
            .And((product != null && product.Id != Guid.Empty)
                .Label("Should have a generated ID"))
            .And((product != null && product.Name == request.Name)
                .Label("Name should match"))
            .And((product != null && product.Price == request.Price)
                .Label("Price should match"))
            .And((product != null && product.Colour == request.Colour)
                .Label("Colour should match"));
    }
}

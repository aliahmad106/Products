using System.Net;
using System.Net.Http.Json;
using FsCheck;
using FsCheck.Xunit;
using Products.Application.DTOs;
using Products.PropertyTests.Generators;
using Xunit;

namespace Products.PropertyTests;

/// <summary>
/// Feature: products-web-api, Property 7: Colour filter returns only matching products (case-insensitive)
/// Validates: Requirements 5.1
/// </summary>
public class ColourFilterProperties : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;

    public ColourFilterProperties(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Property(MaxTest = 100)]
    public Property ColourFilter_ReturnsOnlyMatchingProducts()
    {
        var colourGen = Gen.Elements("Red", "Blue", "Green", "Yellow", "Black");

        return Prop.ForAll(colourGen.ToArbitrary(), colour =>
        {
            var token = _fixture.GetTokenAsync().GetAwaiter().GetResult();
            _fixture.SetAuth(token);

            // Create products with different colours
            var targetRequest = new CreateProductRequest($"Product_{Guid.NewGuid():N}", null, 10m, colour);
            _fixture.Client.PostAsJsonAsync("/api/products", targetRequest).GetAwaiter().GetResult();

            var otherColour = colour == "Red" ? "Blue" : "Red";
            var otherRequest = new CreateProductRequest($"Other_{Guid.NewGuid():N}", null, 20m, otherColour);
            _fixture.Client.PostAsJsonAsync("/api/products", otherRequest).GetAwaiter().GetResult();

            // Query with colour filter (test case-insensitivity)
            var filterColour = colour.ToUpper();
            var response = _fixture.Client.GetAsync($"/api/products?colour={filterColour}").GetAwaiter().GetResult();
            var products = response.Content.ReadFromJsonAsync<List<ProductResponse>>().GetAwaiter().GetResult();

            var allMatch = products?.All(p =>
                string.Equals(p.Colour, colour, StringComparison.OrdinalIgnoreCase)) ?? false;

            return allMatch.Label($"All returned products should have colour '{colour}' (filtered with '{filterColour}')");
        });
    }
}

/// <summary>
/// Feature: products-web-api, Property 8: Whitespace colour filter returns all products
/// Validates: Requirements 5.3
/// </summary>
public class WhitespaceFilterProperties : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;

    public WhitespaceFilterProperties(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Property(MaxTest = 100)]
    public Property WhitespaceColourFilter_ReturnsAllProducts()
    {
        var whitespaceGen = ProductGenerators.WhitespaceString();

        return Prop.ForAll(whitespaceGen, whitespace =>
        {
            var token = _fixture.GetTokenAsync().GetAwaiter().GetResult();
            _fixture.SetAuth(token);

            // Get all products without filter
            var allResponse = _fixture.Client.GetAsync("/api/products").GetAwaiter().GetResult();
            var allProducts = allResponse.Content.ReadFromJsonAsync<List<ProductResponse>>().GetAwaiter().GetResult();

            // Get products with whitespace filter
            var filteredResponse = _fixture.Client.GetAsync($"/api/products?colour={Uri.EscapeDataString(whitespace)}").GetAwaiter().GetResult();
            var filteredProducts = filteredResponse.Content.ReadFromJsonAsync<List<ProductResponse>>().GetAwaiter().GetResult();

            return (allProducts?.Count == filteredProducts?.Count)
                .Label($"Whitespace filter '{whitespace}' should return same count as no filter ({allProducts?.Count} vs {filteredProducts?.Count})");
        });
    }
}

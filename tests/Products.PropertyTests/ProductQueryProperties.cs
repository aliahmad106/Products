using System.Net;
using System.Net.Http.Json;
using FsCheck;
using FsCheck.Xunit;
using Products.Application.DTOs;
using Products.PropertyTests.Generators;
using Xunit;

namespace Products.PropertyTests;

// Helper for deserializing paged responses in tests
public record TestPagedResponse(List<ProductResponse> Items, int TotalCount, int Page, int PageSize);

/// <summary>
/// Property: Colour filter returns only matching products (case-insensitive)
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

            var targetRequest = new CreateProductRequest($"Product_{Guid.NewGuid():N}", null, 10m, colour);
            _fixture.Client.PostAsJsonAsync("/api/products", targetRequest).GetAwaiter().GetResult();

            var otherColour = colour == "Red" ? "Blue" : "Red";
            var otherRequest = new CreateProductRequest($"Other_{Guid.NewGuid():N}", null, 20m, otherColour);
            _fixture.Client.PostAsJsonAsync("/api/products", otherRequest).GetAwaiter().GetResult();

            var filterColour = colour.ToUpper();
            var response = _fixture.Client.GetAsync($"/api/products?colour={filterColour}").GetAwaiter().GetResult();
            var paged = response.Content.ReadFromJsonAsync<TestPagedResponse>().GetAwaiter().GetResult();

            var allMatch = paged?.Items.All(p =>
                string.Equals(p.Colour, colour, StringComparison.OrdinalIgnoreCase)) ?? false;

            return allMatch.Label($"All returned products should have colour '{colour}'");
        });
    }
}

/// <summary>
/// Property: Whitespace colour filter returns all products
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

            var allResponse = _fixture.Client.GetAsync("/api/products").GetAwaiter().GetResult();
            var allPaged = allResponse.Content.ReadFromJsonAsync<TestPagedResponse>().GetAwaiter().GetResult();

            var filteredResponse = _fixture.Client.GetAsync($"/api/products?colour={Uri.EscapeDataString(whitespace)}").GetAwaiter().GetResult();
            var filteredPaged = filteredResponse.Content.ReadFromJsonAsync<TestPagedResponse>().GetAwaiter().GetResult();

            return (allPaged?.TotalCount == filteredPaged?.TotalCount)
                .Label($"Whitespace filter should return same count as no filter ({allPaged?.TotalCount} vs {filteredPaged?.TotalCount})");
        });
    }
}

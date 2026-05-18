using System.Net;
using System.Net.Http.Json;
using FsCheck;
using FsCheck.Xunit;
using Products.Application.DTOs;
using Xunit;

namespace Products.PropertyTests;

/// <summary>
/// Feature: products-web-api, Property 5: Product creation round-trip
/// Validates: Requirements 3.4, 4.1
/// </summary>
public class ProductRoundTripProperties : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;

    public ProductRoundTripProperties(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Property(MaxTest = 100)]
    public Property ProductCreationRoundTrip()
    {
        // Use printable ASCII characters only for product names
        var nameCharGen = Gen.Elements(
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -_"
                .ToCharArray());
        var nameGen = Gen.ArrayOf(Gen.Choose(1, 30).SelectMany(_ => nameCharGen))
            .Where(chars => chars.Length > 0)
            .Select(chars => new string(chars));

        var validGen = from name in nameGen
                       from price in Gen.Choose(0, 100000).Select(p => (decimal)p / 100m)
                       from colour in Gen.Elements("Red", "Blue", "Green", "Yellow", "Black")
                       select new CreateProductRequest(name, $"Desc for {name}", price, colour);

        return Prop.ForAll(validGen.ToArbitrary(), request =>
        {
            var token = _fixture.GetTokenAsync().GetAwaiter().GetResult();
            _fixture.SetAuth(token);

            // Create product
            var createResponse = _fixture.Client.PostAsJsonAsync("/api/products", request).GetAwaiter().GetResult();
            if (createResponse.StatusCode != HttpStatusCode.Created)
                return false.Label("Create should return 201");

            var created = createResponse.Content.ReadFromJsonAsync<ProductResponse>().GetAwaiter().GetResult();

            // Retrieve all products
            var getResponse = _fixture.Client.GetAsync("/api/products").GetAwaiter().GetResult();
            var products = getResponse.Content.ReadFromJsonAsync<List<ProductResponse>>().GetAwaiter().GetResult();

            var found = products?.Any(p =>
                p.Id == created!.Id &&
                p.Name == request.Name &&
                p.Price == request.Price &&
                p.Colour == request.Colour) ?? false;

            return found.Label("Created product should appear in GET /api/products");
        });
    }
}

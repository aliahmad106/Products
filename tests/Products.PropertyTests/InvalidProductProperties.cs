using System.Net;
using System.Net.Http.Json;
using FsCheck;
using FsCheck.Xunit;
using Products.Application.DTOs;
using Products.PropertyTests.Generators;
using Xunit;

namespace Products.PropertyTests;

/// <summary>
/// Feature: products-web-api, Property 4: Invalid product input returns 400
/// Validates: Requirements 3.2, 3.3
/// </summary>
public class InvalidProductProperties : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;

    public InvalidProductProperties(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Property(MaxTest = 100)]
    public Property InvalidProductInput_Returns400()
    {
        var invalidGen = ProductGenerators.InvalidProduct();

        return Prop.ForAll(invalidGen, request =>
        {
            var token = _fixture.GetTokenAsync().GetAwaiter().GetResult();
            _fixture.SetAuth(token);

            var response = _fixture.Client.PostAsJsonAsync("/api/products", request).GetAwaiter().GetResult();

            return (response.StatusCode == HttpStatusCode.BadRequest)
                .Label($"Should return 400 for invalid input: Name='{request.Name}', Price={request.Price}, Colour='{request.Colour}'");
        });
    }
}

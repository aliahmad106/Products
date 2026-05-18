using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FsCheck;
using FsCheck.Xunit;
using Products.Application.DTOs;
using Xunit;

namespace Products.PropertyTests;

/// <summary>
/// Feature: products-web-api, Property 1: Invalid or missing auth token returns 401
/// Validates: Requirements 2.1, 2.2, 2.4
/// </summary>
public class AuthenticationProperties : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;

    public AuthenticationProperties(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Property(MaxTest = 100)]
    public Property InvalidOrMissingAuthToken_Returns401()
    {
        // Generate header-safe invalid tokens (no control characters)
        var safeCharGen = Gen.Elements(
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_=+/"
                .ToCharArray());
        var randomTokenGen = Gen.ArrayOf(Gen.Choose(1, 50).SelectMany(_ => safeCharGen))
            .Select(chars => new string(chars));

        var invalidTokenGen = Gen.OneOf(
            Gen.Constant(""),
            Gen.Constant("invalid-token"),
            Gen.Constant("not.a.valid.jwt"),
            randomTokenGen,
            Gen.Constant("eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ0ZXN0In0.invalidsignature")
        );

        return Prop.ForAll(invalidTokenGen.ToArbitrary(), token =>
        {
            _fixture.ClearAuth();

            if (!string.IsNullOrEmpty(token))
            {
                _fixture.Client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            var getResponse = _fixture.Client.GetAsync("/api/products").GetAwaiter().GetResult();
            var postResponse = _fixture.Client.PostAsJsonAsync("/api/products",
                new CreateProductRequest("Test", null, 10m, "Red")).GetAwaiter().GetResult();

            return (getResponse.StatusCode == HttpStatusCode.Unauthorized)
                .Label("GET /api/products should return 401")
                .And((postResponse.StatusCode == HttpStatusCode.Unauthorized)
                    .Label("POST /api/products should return 401"));
        });
    }
}

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Products.Application.DTOs;
using Xunit;

namespace Products.IntegrationTests;

public class ProductsApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ProductsApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.SeedTestUser();
    }

    private async Task<string> GetTokenAsync(HttpClient client)
    {
        var login = new LoginRequest("admin", "password123");
        var response = await client.PostAsJsonAsync("/api/auth/login", login);
        response.EnsureSuccessStatusCode();

        // Extract access token from Set-Cookie header
        var cookies = response.Headers.GetValues("Set-Cookie");
        var accessCookie = cookies.First(c => c.StartsWith("access_token="));
        var token = accessCookie.Split('=', 2)[1].Split(';')[0];
        return token;
    }

    // Health endpoint - anonymous access
    [Fact]
    public async Task Health_ReturnsOk_WithoutAuth()
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // Auth enforcement - 401 without token
    [Fact]
    public async Task GetProducts_WithoutToken_Returns401()
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/products");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostProducts_WithoutToken_Returns401()
    {
        using var client = _factory.CreateClient();
        var request = new CreateProductRequest("Test", null, 10m, "Red");
        var response = await client.PostAsJsonAsync("/api/products", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Product CRUD flow
    [Fact]
    public async Task CreateAndGetProducts_EndToEnd()
    {
        using var client = _factory.CreateClient();
        var token = await GetTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create product
        var createRequest = new CreateProductRequest("Integration Widget", "Test", 25.50m, "Green");
        var createResponse = await client.PostAsJsonAsync("/api/products", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();
        created!.Name.Should().Be("Integration Widget");
        created.Id.Should().NotBeEmpty();

        // Get all products
        var getResponse = await client.GetAsync("/api/products");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var pagedResult = await getResponse.Content.ReadFromJsonAsync<PagedResponse<ProductResponse>>();
        pagedResult!.Items.Should().Contain(p => p.Name == "Integration Widget");
    }

    // Validation error responses
    [Fact]
    public async Task CreateProduct_InvalidData_Returns400()
    {
        using var client = _factory.CreateClient();
        var token = await GetTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateProductRequest("", null, -5m, "");
        var response = await client.PostAsJsonAsync("/api/products", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

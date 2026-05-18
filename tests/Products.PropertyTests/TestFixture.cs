using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Products.Application.DTOs;
using Products.Infrastructure.Data;

namespace Products.PropertyTests;

public class TestFixture : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    public HttpClient Client { get; }

    public TestFixture()
    {
        var dbName = $"PropertyTestDb_{Guid.NewGuid()}";

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove all DbContext-related registrations
                    var descriptors = services
                        .Where(d => d.ServiceType == typeof(DbContextOptions<ProductsDbContext>)
                                 || d.ServiceType == typeof(ProductsDbContext))
                        .ToList();
                    foreach (var d in descriptors)
                        services.Remove(d);

                    services.AddDbContext<ProductsDbContext>(options =>
                        options.UseInMemoryDatabase(dbName));
                });
                builder.UseEnvironment("Development");
            });

        Client = _factory.CreateClient();
    }

    public async Task<string> GetTokenAsync()
    {
        var login = new LoginRequest("admin", "password123");
        var response = await Client.PostAsJsonAsync("/api/auth/login", login);
        response.EnsureSuccessStatusCode();
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResponse!.Token;
    }

    public void SetAuth(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAuth()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    public void Dispose()
    {
        Client.Dispose();
        _factory.Dispose();
    }
}

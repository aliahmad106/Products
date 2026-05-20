using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Products.Application.DTOs;
using Products.Domain.Entities;
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
                builder.UseEnvironment("Testing");
            });

        Client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });

        // Seed test user
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        db.Users.Add(new User("admin", passwordHash));
        db.SaveChanges();
    }

    public async Task<string> GetTokenAsync()
    {
        var login = new LoginRequest("admin", "password123");
        var response = await Client.PostAsJsonAsync("/api/auth/login", login);
        response.EnsureSuccessStatusCode();

        // The token is set as an httpOnly cookie. Extract from Set-Cookie header.
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            var accessCookie = cookies.FirstOrDefault(c => c.StartsWith("access_token="));
            if (accessCookie != null)
            {
                var token = accessCookie.Split('=', 2)[1].Split(';')[0];
                return token;
            }
        }

        return string.Empty;
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

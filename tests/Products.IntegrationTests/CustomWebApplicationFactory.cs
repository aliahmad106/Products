using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Products.Domain.Entities;
using Products.Infrastructure.Data;

namespace Products.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
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

            // Add in-memory database for testing
            services.AddDbContext<ProductsDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });

        builder.UseEnvironment("Testing");
    }

    public void SeedTestUser()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        db.Users.Add(new User("admin", passwordHash));
        db.SaveChanges();
    }
}

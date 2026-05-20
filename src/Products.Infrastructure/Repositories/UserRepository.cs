using Microsoft.EntityFrameworkCore;
using Products.Application.Interfaces;
using Products.Domain.Entities;
using Products.Infrastructure.Data;

namespace Products.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ProductsDbContext _context;

    public UserRepository(ProductsDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username.ToLower(), ct);
    }

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);
        return user;
    }

    public async Task<bool> ExistsAsync(string username, CancellationToken ct = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Username == username.ToLower(), ct);
    }
}

using EbayClone.API.Data;
using EbayClone.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EbayClone.API.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.SingleOrDefaultAsync(
            user => user.Email == email,
            cancellationToken);
    }

    public async Task<(IReadOnlyList<User> Items, int Total)> GetPageAsync(
        string? search,
        string? role,
        string? status,
        string? sort,
        string? direction,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(user => user.Email.Contains(keyword) || user.FullName.Contains(keyword));
        }
        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(user => user.Role == role.Trim());
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(user => user.ModerationStatus == status.Trim());

        var descending = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
        var orderedQuery = (sort?.Trim().ToLowerInvariant()) switch
        {
            "name" => descending ? query.OrderByDescending(user => user.FullName).ThenByDescending(user => user.Id) : query.OrderBy(user => user.FullName).ThenBy(user => user.Id),
            "email" => descending ? query.OrderByDescending(user => user.Email).ThenByDescending(user => user.Id) : query.OrderBy(user => user.Email).ThenBy(user => user.Id),
            _ => descending ? query.OrderByDescending(user => user.Id) : query.OrderBy(user => user.Id)
        };

        var total = await orderedQuery.CountAsync(cancellationToken);
        var items = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}

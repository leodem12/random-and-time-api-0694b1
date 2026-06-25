using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class RandomService(RandomDbContext db)
{
    public async Task<RandomAttempt> CreateRandomAsync()
    {
        var attempt = new RandomAttempt
        {
            Value = Random.Shared.NextInt64(0, long.MaxValue),
            CreatedAt = DateTime.UtcNow
        };
        db.RandomAttempts.Add(attempt);
        await db.SaveChangesAsync();
        return attempt;
    }

    public async Task<IReadOnlyList<RandomAttempt>> GetHistoryAsync() =>
        await db.RandomAttempts
            .OrderByDescending(a => a.CreatedAt)
            .Take(50)
            .ToListAsync();
}

using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class TimeService(TimeDbContext db)
{
    public async Task<TimeAttempt> CreateNowAsync()
    {
        var attempt = new TimeAttempt { ServerTimeUtc = DateTime.UtcNow };
        db.TimeAttempts.Add(attempt);
        await db.SaveChangesAsync();
        return attempt;
    }

    public async Task<IReadOnlyList<TimeAttempt>> GetHistoryAsync() =>
        await db.TimeAttempts
            .OrderByDescending(a => a.ServerTimeUtc)
            .Take(50)
            .ToListAsync();
}

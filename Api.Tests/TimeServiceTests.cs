using Api.Data;
using Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Api.Tests;

// Uses EF Core InMemory provider — no PostgreSQL server or network required.
public sealed class TimeServiceTests
{
    private static TimeDbContext CreateDb(string dbName)
    {
        var opts = new DbContextOptionsBuilder<TimeDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new TimeDbContext(opts);
    }

    // R3: get-now returns the current server time (UTC) in the response.
    [Fact]
    public async Task CreateNow_ReturnsAttemptWithUtcTime()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        using var db = CreateDb(nameof(CreateNow_ReturnsAttemptWithUtcTime));
        var svc = new TimeService(db);

        var result = await svc.CreateNowAsync();

        Assert.True(result.ServerTimeUtc >= before);
        Assert.True(result.ServerTimeUtc <= DateTime.UtcNow.AddSeconds(1));
        Assert.Equal(DateTimeKind.Utc, result.ServerTimeUtc.Kind);
    }

    // R3: get-now inserts one record (timestamp) into the PostgreSQL store (simulated via InMemory).
    [Fact]
    public async Task CreateNow_PersistsOneRecord()
    {
        using var db = CreateDb(nameof(CreateNow_PersistsOneRecord));
        var svc = new TimeService(db);

        var result = await svc.CreateNowAsync();

        Assert.Equal(1, await db.TimeAttempts.CountAsync());
        Assert.NotEqual(0, result.Id);
    }

    // R3: each call is its own attempt.
    [Fact]
    public async Task CreateNow_EachCallIsAnIndependentAttempt()
    {
        using var db = CreateDb(nameof(CreateNow_EachCallIsAnIndependentAttempt));
        var svc = new TimeService(db);

        var a = await svc.CreateNowAsync();
        var b = await svc.CreateNowAsync();

        Assert.NotEqual(a.Id, b.Id);
        Assert.Equal(2, await db.TimeAttempts.CountAsync());
    }

    // R4: history returns empty list when no attempts exist.
    [Fact]
    public async Task GetHistory_ReturnsEmptyWhenNoRecordsExist()
    {
        using var db = CreateDb(nameof(GetHistory_ReturnsEmptyWhenNoRecordsExist));
        var svc = new TimeService(db);

        var result = await svc.GetHistoryAsync();

        Assert.Empty(result);
    }

    // R4: history returns records ordered most-recent first.
    [Fact]
    public async Task GetHistory_ReturnsMostRecentFirst()
    {
        using var db = CreateDb(nameof(GetHistory_ReturnsMostRecentFirst));
        var base_ = DateTime.UtcNow.AddMinutes(-5);
        db.TimeAttempts.AddRange(
            new Api.Models.TimeAttempt { ServerTimeUtc = base_ },
            new Api.Models.TimeAttempt { ServerTimeUtc = base_.AddMinutes(1) },
            new Api.Models.TimeAttempt { ServerTimeUtc = base_.AddMinutes(2) });
        await db.SaveChangesAsync();

        var svc = new TimeService(db);
        var result = await svc.GetHistoryAsync();

        Assert.Equal(3, result.Count);
        Assert.True(result[0].ServerTimeUtc >= result[1].ServerTimeUtc);
        Assert.True(result[1].ServerTimeUtc >= result[2].ServerTimeUtc);
    }

    // R4: history returns at most 50 records when more than 50 exist.
    [Fact]
    public async Task GetHistory_CapsAtFiftyRecords()
    {
        using var db = CreateDb(nameof(GetHistory_CapsAtFiftyRecords));
        var base_ = DateTime.UtcNow.AddHours(-1);
        for (int i = 0; i < 60; i++)
            db.TimeAttempts.Add(new Api.Models.TimeAttempt { ServerTimeUtc = base_.AddSeconds(i) });
        await db.SaveChangesAsync();

        var svc = new TimeService(db);
        var result = await svc.GetHistoryAsync();

        Assert.Equal(50, result.Count);
    }

    // R4 / Happy-path: after CreateNow the new record appears at the top of history.
    [Fact]
    public async Task HappyPath_NewRecordAppearsInHistory()
    {
        using var db = CreateDb(nameof(HappyPath_NewRecordAppearsInHistory));
        var svc = new TimeService(db);

        var attempt = await svc.CreateNowAsync();
        var history = await svc.GetHistoryAsync();

        Assert.Contains(history, r => r.Id == attempt.Id);
        Assert.Equal(attempt.Id, history[0].Id); // most-recent first
    }
}

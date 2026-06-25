using Api.Data;
using Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Api.Tests;

// Uses SQLite in-memory (connection kept open for lifetime) — no file, no network.
public sealed class RandomServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly RandomDbContext _db;
    private readonly RandomService _svc;

    public RandomServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var opts = new DbContextOptionsBuilder<RandomDbContext>()
            .UseSqlite(_connection)
            .Options;
        _db = new RandomDbContext(opts);
        _db.Database.EnsureCreated();
        _svc = new RandomService(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    // R1: get-random returns a random number in the response.
    [Fact]
    public async Task CreateRandom_ReturnsAttemptWithValue()
    {
        var result = await _svc.CreateRandomAsync();

        Assert.True(result.Value >= 0);
    }

    // R1: get-random inserts one record (number + timestamp) into the SQLite store.
    [Fact]
    public async Task CreateRandom_PersistsOneRecord()
    {
        var countBefore = await _db.RandomAttempts.CountAsync();

        var result = await _svc.CreateRandomAsync();

        var countAfter = await _db.RandomAttempts.CountAsync();
        Assert.Equal(countBefore + 1, countAfter);
        Assert.NotEqual(0, result.Id);
        Assert.True(result.CreatedAt > DateTime.UtcNow.AddSeconds(-5));
    }

    // R1: each call is its own attempt — two calls produce two distinct records.
    [Fact]
    public async Task CreateRandom_EachCallIsAnIndependentAttempt()
    {
        var a = await _svc.CreateRandomAsync();
        var b = await _svc.CreateRandomAsync();

        Assert.NotEqual(a.Id, b.Id);
        Assert.Equal(2, await _db.RandomAttempts.CountAsync());
    }

    // R2: history returns empty list when no attempts exist.
    [Fact]
    public async Task GetHistory_ReturnsEmptyWhenNoRecordsExist()
    {
        var result = await _svc.GetHistoryAsync();

        Assert.Empty(result);
    }

    // R2: history returns records ordered most-recent first.
    [Fact]
    public async Task GetHistory_ReturnsMostRecentFirst()
    {
        // Insert records with explicit timestamps to ensure deterministic ordering.
        var base_ = DateTime.UtcNow.AddMinutes(-5);
        _db.RandomAttempts.AddRange(
            new Api.Models.RandomAttempt { Value = 1, CreatedAt = base_ },
            new Api.Models.RandomAttempt { Value = 2, CreatedAt = base_.AddMinutes(1) },
            new Api.Models.RandomAttempt { Value = 3, CreatedAt = base_.AddMinutes(2) });
        await _db.SaveChangesAsync();

        var result = await _svc.GetHistoryAsync();

        Assert.Equal(3, 3, (a, b) => a == b); // sanity
        Assert.True(result[0].CreatedAt >= result[1].CreatedAt);
        Assert.True(result[1].CreatedAt >= result[2].CreatedAt);
        Assert.Equal(3, result[0].Value); // highest value (most recent)
    }

    // R2: history returns at most 50 records when more than 50 exist.
    [Fact]
    public async Task GetHistory_CapsAtFiftyRecords()
    {
        var base_ = DateTime.UtcNow.AddHours(-1);
        for (int i = 0; i < 60; i++)
            _db.RandomAttempts.Add(new Api.Models.RandomAttempt
            {
                Value = i,
                CreatedAt = base_.AddSeconds(i)
            });
        await _db.SaveChangesAsync();

        var result = await _svc.GetHistoryAsync();

        Assert.Equal(50, result.Count);
    }

    // R2 / Happy-path: after CreateRandom the new record appears at the top of history.
    [Fact]
    public async Task HappyPath_NewRecordAppearsInHistory()
    {
        var attempt = await _svc.CreateRandomAsync();

        var history = await _svc.GetHistoryAsync();

        Assert.Contains(history, r => r.Id == attempt.Id && r.Value == attempt.Value);
        Assert.Equal(attempt.Id, history[0].Id); // most-recent first
    }
}

using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class TimeDbContext(DbContextOptions<TimeDbContext> options) : DbContext(options)
{
    public DbSet<TimeAttempt> TimeAttempts => Set<TimeAttempt>();
}

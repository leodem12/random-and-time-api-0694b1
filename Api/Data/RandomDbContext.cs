using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class RandomDbContext(DbContextOptions<RandomDbContext> options) : DbContext(options)
{
    public DbSet<RandomAttempt> RandomAttempts => Set<RandomAttempt>();
}

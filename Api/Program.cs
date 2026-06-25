using Api.Data;
using Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RandomDbContext>(opts =>
    opts.UseSqlite(builder.Configuration.GetConnectionString("Random") ?? "Data Source=random.db"));

builder.Services.AddDbContext<TimeDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("ConnectionStrings:Default is required")));

builder.Services.AddScoped<RandomService>();
builder.Services.AddScoped<TimeService>();

// CORS: permissive in Development; restricted to the known frontend origin otherwise.
builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p =>
    {
        if (builder.Environment.IsDevelopment())
            p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        else
            p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod();
    }));

var app = builder.Build();

// Ensure SQLite schema exists before serving requests
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<RandomDbContext>().Database.EnsureCreated();

// Ensure PostgreSQL schema exists before serving requests
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<TimeDbContext>().Database.EnsureCreated();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/random", async (RandomService svc) =>
    Results.Ok(await svc.CreateRandomAsync()));

app.MapGet("/api/random/history", async (RandomService svc) =>
    Results.Ok(await svc.GetHistoryAsync()));

app.MapGet("/api/now", async (TimeService svc) =>
    Results.Ok(await svc.CreateNowAsync()));

app.MapGet("/api/now/history", async (TimeService svc) =>
    Results.Ok(await svc.GetHistoryAsync()));

app.Run();

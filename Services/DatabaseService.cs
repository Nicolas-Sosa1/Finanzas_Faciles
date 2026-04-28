using Microsoft.EntityFrameworkCore;

namespace FinanzasFaciles.Services;

public class DatabaseService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public DatabaseService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

        public async Task InitializeAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        await db.Database.EnsureCreatedAsync();
    }

        public async Task BorrarTodosLosDatosAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

        public Task<AppDbContext> CreateContextAsync() => _factory.CreateDbContextAsync();
}

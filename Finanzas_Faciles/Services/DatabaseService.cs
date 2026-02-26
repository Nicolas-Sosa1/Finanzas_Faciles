using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanzas_Faciles.Services
{
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

        public Task<AppDbContext> CreateContextAsync() => _factory.CreateDbContextAsync();
    }
}

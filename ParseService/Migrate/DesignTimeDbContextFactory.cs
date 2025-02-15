using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using ParseService.Data;

namespace ParseService.Migrate;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ParseDbContext>
{
    public ParseDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ParseDbContext>();
        optionsBuilder.UseSqlite(configuration.GetConnectionString("Sqlite"));

        return new ParseDbContext(optionsBuilder.Options);
    }
}
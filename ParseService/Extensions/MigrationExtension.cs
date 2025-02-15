using Microsoft.EntityFrameworkCore;
using ParseService.Data;

namespace ParseService.Extensions
{
    public static class MigrationExtension
    {
        public static IHost AddMigrate(this IHost host)
        {
            using(var scope = host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ParseDbContext>();
                dbContext.Database.Migrate();
            }

            return host;
        }
    }
}

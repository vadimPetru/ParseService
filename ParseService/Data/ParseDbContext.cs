using Microsoft.EntityFrameworkCore;
using ParseService.Models;
using System;

namespace ParseService.Data
{
    public class ParseDbContext : DbContext
    {
        public ParseDbContext(DbContextOptions<ParseDbContext> options)
            : base(options)
        {
        }

        public DbSet<AnnouncementItem> Announcements { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AnnouncementItem>()
                         .Property(a => a.AnnId)
                         .ValueGeneratedNever(); // Отключает автоинкремент
        }
    }
}

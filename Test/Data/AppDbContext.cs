using Microsoft.EntityFrameworkCore;
using Test.Models;

namespace Test.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Login)
                .IsUnique();
            modelBuilder.Entity<User>()
                .Property(x => x.Guid);
        }
    }
}
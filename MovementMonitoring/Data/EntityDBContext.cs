using Microsoft.EntityFrameworkCore;
using MovementMonitoring.Models;

namespace MovementMonitoring.Data
{
    public class EntityDBContext : DbContext
    {
        public DbSet<Room> Rooms { get; set; }
        public DbSet<AccessLevel> AccessLevels { get; set; }
        public DbSet<Camera> Cameras { get; set; }
        public DbSet<Movement> Movements { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Violation> Violations { get; set; }

        public EntityDBContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccessLevel>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<Camera>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<Room>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<Person>().HasIndex(p => p.Guid).IsUnique();
        }
    }
}

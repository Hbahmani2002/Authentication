using AngularAuthApi.Models;
using AngularAuthApi.Models.Api;
using Microsoft.EntityFrameworkCore;

namespace AngularAuthApi.Context
{
    public class ApiDbContext: DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options):base(options)
        {
            
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Files> Files { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Files>().ToTable("Files");
        }

    }
}

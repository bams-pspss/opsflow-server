

using Microsoft.EntityFrameworkCore;
using OpsFlow.Models;

namespace OpsFlow.Data
{
    public class DataContextEF(IConfiguration config) : DbContext
    {
        private readonly IConfiguration _config = config;
        public DbSet<TestEntity> TestEntity { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options
                    .UseSqlServer(_config.GetConnectionString("DefaultConnection"),
                    options => options.EnableRetryOnFailure());
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");

            modelBuilder.Entity<TestEntity>()
                .ToTable("TestingTable", "dbo")
                .HasKey(p => p.Id);


        }
    }
}

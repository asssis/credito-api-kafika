using CreditApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditApi.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Credito> Creditos { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Credito>().ToTable("credito");
            base.OnModelCreating(modelBuilder);
        }
    }
}

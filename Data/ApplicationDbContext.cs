using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using JobPortal.Models;

namespace JobPortal.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Job> Jobs { get; set; }
        public DbSet<Application> Applications { get; set; }
        // CompanyProfile removed as per requirements
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure the Salary property to have the appropriate precision and scale
            modelBuilder.Entity<Job>()
                .Property(j => j.Salary)
                .HasColumnType("decimal(18,2)");
        }
    }
}

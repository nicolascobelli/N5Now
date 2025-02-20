using Microsoft.EntityFrameworkCore;
using UserPermissions.Domain.Entities;

namespace UserPermissions.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Permission> Permissions { get; set; }
        public DbSet<PermissionType> PermissionTypes { get; set; }
        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>()
                .HasOne(p => p.Employee)
                .WithMany(e => e.Permissions)
                .HasForeignKey(p => p.EmployeeId);

            modelBuilder.Entity<Permission>()
                .HasOne(p => p.PermissionType)
                .WithMany()
                .HasForeignKey(p => p.PermissionTypeId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
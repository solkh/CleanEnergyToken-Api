using CleanEnergyToken_Api.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Nethereum.Contracts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CleanEnergyToken_Api.Entities;

namespace CleanEnergyToken_Api.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
            // Add clustered translation indexes
            //builder.Entity<Notification>().HasIndex(c => c.Language);


            #region Use updated datetime2
            //https://stackoverflow.com/questions/43277154/entity-framework-core-setting-the-decimal-precision-and-scale-to-all-decimal-p
            foreach (var property in builder.Model
                .GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
            {
                property.SetColumnType("datetime2");
            }
            #endregion

            base.OnModelCreating(builder);
        }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationMessage> NotificationMessages { get; set; }
    }
}

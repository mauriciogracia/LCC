using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class ReferralDbContext : DbContext
    {
        public ReferralDbContext(DbContextOptions<ReferralDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Referral> Referrals => Set<Referral>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(u => u.Uid);
            modelBuilder.Entity<Referral>().HasKey(r => r.ReferralId);

            modelBuilder.Entity<User>().HasData(
                new User { Uid = "U1", Name = "María Gómez", Email = "maria.gomez@mgg.com", ReferralCode = "" },
                new User { Uid = "U2", Name = "Carlos Rodríguez", Email = "carlos.rodriguez@mgg.com", ReferralCode = "" },
                new User { Uid = "U3", Name = "Lucía Fernández", Email = "lucia.fernandez@mgg.com", ReferralCode = "" },
                new User { Uid = "U4", Name = "Javier Martínez", Email = "javier.martinez@mgg.com", ReferralCode = "" },
                new User { Uid = "U5", Name = "Sofía Ramírez", Email = "sofia.ramirez@mgg.com", ReferralCode = "" }
            );

            modelBuilder.Entity<User>()
               .HasMany(u => u.Referrals)
               .WithOne(r => r.User)
               .HasForeignKey(r => r.Uid)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

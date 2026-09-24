using AktienMarkplatz.Classes;
using AktienMarkplatz.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AktienMarkplatz.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Verrechnungskonto> Verrechnungskonten { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Ein Konto pro Benutzer, wird beim Loeschen des Benutzers mitgeloescht.
            builder.Entity<Verrechnungskonto>()
                .HasOne(k => k.User)
                .WithOne()
                .HasForeignKey<Verrechnungskonto>(k => k.UserId);
        }
    }
}

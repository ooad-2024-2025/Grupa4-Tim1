using Jahacki_klub_Zeljeznicar.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Jahacki_klub_Zeljeznicar.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>
       options)
        : base(options)
        {
        }
        public DbSet<Clanarina> Clanarine { get; set; }
        public DbSet<Konj> Konji { get; set; }
        public DbSet<Trail> Trails { get; set; }
        public DbSet<Trail_Konj> TrailKonji { get; set; }
        public DbSet<Trening> Treninzi { get; set; }
        public DbSet<Trening_Konj> TreningKonji { get; set; }
        public DbSet<Trening_User> TreningUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Clanarina>().ToTable("Clanarina");
            modelBuilder.Entity<Konj>().ToTable("Konj");
            modelBuilder.Entity<Trail>().ToTable("Trail");
            modelBuilder.Entity<Trail_Konj>().ToTable("Trail_Konj");
            modelBuilder.Entity<Trening>().ToTable("Trening");
            modelBuilder.Entity<Trening_Konj>().ToTable("Trening_Konj");
            modelBuilder.Entity<Trening_User>().ToTable("Trening_User");

            // Clanarina ↔ User (many-to-one)
            modelBuilder.Entity<Clanarina>()
                .HasOne(c => c.User)
                .WithMany(u => u.Clanarine)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Trail ↔ User (Rezervator)
            modelBuilder.Entity<Trail>()
                .HasOne(t => t.Rezervator)
                .WithMany()
                .HasForeignKey(t => t.RezervatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Trail_Konj (many-to-one) relationships
            modelBuilder.Entity<Trail_Konj>()
                .HasOne(tk => tk.Trail)
                .WithMany(t => t.TrailKonji)
                .HasForeignKey(tk => tk.TrailId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Trail_Konj>()
                .HasOne(tk => tk.Konj)
                .WithMany(k => k.TrailKonji)
                .HasForeignKey(tk => tk.KonjId)
                .OnDelete(DeleteBehavior.Cascade);

            // Trening ↔ User (Trener)
            modelBuilder.Entity<Trening>()
                .HasOne(t => t.Trener)
                .WithMany()
                .HasForeignKey(t => t.TrenerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Trening_Konj relationships
            modelBuilder.Entity<Trening_Konj>()
                .HasOne(tk => tk.Trening)
                .WithMany(t => t.TreningKonji)
                .HasForeignKey(tk => tk.TreningId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Trening_Konj>()
                .HasOne(tk => tk.Konj)
                .WithMany(k => k.TreningKonji)
                .HasForeignKey(tk => tk.KonjId)
                .OnDelete(DeleteBehavior.Cascade);

            // Trening_User relationships
            modelBuilder.Entity<Trening_User>()
                .HasOne(tu => tu.Trening)
                .WithMany(t => t.TreningUsers)
                .HasForeignKey(tu => tu.TreningId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Trening_User>()
                .HasOne(tu => tu.User)
                .WithMany(u => u.TreningUsers)
                .HasForeignKey(tu => tu.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}

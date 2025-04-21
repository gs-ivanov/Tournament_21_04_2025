namespace Tournament.Data
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using System;
    using Tournament.Data.Models;
    using Tournament.Models;

    public class TurnirDbContext : IdentityDbContext<User>
    {
        public TurnirDbContext(DbContextOptions<TurnirDbContext> options)
            : base(options)
        {
        }

        public DbSet<Team> Teams { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<ManagerRequest> ManagerRequests { get; set; }
        public DbSet<MatchSubscription> MatchSubscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Без ключ за Ranking (временно)
            builder.Entity<Ranking>().HasNoKey();

            builder.Entity<Team>()
                .HasOne(t => t.Tournament)
                .WithMany(t => t.Teams)
                .HasForeignKey(t => t.TournamentId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Team>()
                .Property(t => t.UserId)
                .IsRequired(false); // 🟢 позволяваме null

            // Match → TeamA
            builder.Entity<Match>()
                .HasOne(m => m.TeamA)
                .WithMany(t => t.MatchesAsTeamA)
                .HasForeignKey(m => m.TeamAId)
                .OnDelete(DeleteBehavior.Restrict);

            // Match → TeamB
            builder.Entity<Match>()
                .HasOne(m => m.TeamB)
                .WithMany(t => t.MatchesAsTeamB)
                .HasForeignKey(m => m.TeamBId)
                .OnDelete(DeleteBehavior.Restrict);

            // Match → Tournament
            builder.Entity<Match>()
                .HasOne(m => m.Tournament)
                .WithMany(t => t.Matches)
                .HasForeignKey(m => m.TournamentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ManagerRequest → Team
            builder.Entity<ManagerRequest>()
                .HasOne(m => m.Team)
                .WithMany()
                .HasForeignKey(m => m.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // ManagerRequest → Tournament
            builder.Entity<ManagerRequest>()
                .HasOne(m => m.Tournament)
                .WithMany()
                .HasForeignKey(m => m.TournamentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Конверсия на enum TournamentType → int
            builder.Entity<Tournament>()
                .Property(t => t.Type)
                .HasConversion<int>();

            builder.Entity<Tournament>()
                .HasMany(t => t.Teams)
                .WithOne()
                .OnDelete(DeleteBehavior.Restrict);


            // Seed данни
            builder.Entity<Tournament>().HasData(
                new Tournament
                {
                    Id = 1,
                    Name = "Пролетен турнир",
                    Type = TournamentType.Knockout,
                    StartDate = new DateTime(2025, 5, 10),
                    IsOpenForApplications = false
                },
                new Tournament
                {
                    Id = 2,
                    Name = "Летен шампионат",
                    Type = TournamentType.DoubleElimination,
                    StartDate = new DateTime(2025, 7, 1),
                    IsOpenForApplications = false
                },
                new Tournament
                {
                    Id = 3,
                    Name = "Зимна купа",
                    Type = TournamentType.RoundRobin,
                    StartDate = new DateTime(2025, 12, 5),
                    IsOpenForApplications = true
                },
                new Tournament
                {
                    Id = 4,
                    Name = "Есена купа",
                    Type = TournamentType.GroupAndKnockout,
                    StartDate = new DateTime(2025, 09, 5),
                    IsOpenForApplications = false
                },
                new Tournament
                {
                    Id = 5,
                    Name = "Шведска купа",
                    Type = TournamentType.Swiss,
                    StartDate = new DateTime(2025, 11, 5),
                    IsOpenForApplications = false
                }
            );
        }
    }
}

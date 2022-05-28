using System;
using DataModel;
using Microsoft.EntityFrameworkCore;

namespace Domain
{
    public class PatiTournDataBaseContext : DbContext
    {
        public PatiTournDataBaseContext()
        {
        }

        public PatiTournDataBaseContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Person> Persons { get; set; } = null!;
        public DbSet<Referee> Referees { get; set; } = null!;
        public DbSet<Skater> Skaters { get; set; } = null!;
        public DbSet<Competition> Competitions { get; set; } = null!;
        public DbSet<Event> Events { get; set; } = null!;
        public DbSet<TeamEvent> GroupEvents { get; set; } = null!;
        public DbSet<IndividualEvent> IndividualEvents { get; set; } = null!;
        public DbSet<Position> Positions { get; set; } = null!;
        public DbSet<Sprint> Sprints { get; set; } = null!;
        public DbSet<SprintPosition> SprintPositions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

#if DEBUG
            modelBuilder.Entity<Competition>()
                .HasData(
                    new Competition
                    {
                        Id = Guid.NewGuid(),
                        Category = "category",
                        EndDate = DateTime.Today.AddDays(3),
                        Name = "test competition",
                        StartDate = DateTime.Today.AddDays(-1)
                    },
                    new Competition
                    {
                        Id = Guid.NewGuid(),
                        Category = "other category",
                        EndDate = DateTime.Today.AddDays(4),
                        Name = "test2 competition",
                        StartDate = DateTime.Today.AddDays(-2)
                    });
#endif
        }
    }
}

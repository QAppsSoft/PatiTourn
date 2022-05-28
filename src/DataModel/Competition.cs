using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModel
{
    [Table("Competitions")]
    public class Competition : BaseEntity
    {
        public string Name { get; set; } = null!;

        public string Category { get; set; } = null!;

        public DateTime StartDate { get; set; } = DateTime.Today;

        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(3);

        public ICollection<Skater> Skaters { get; set; } = null!;
        public ICollection<Event> Events { get; set; } = null!;
        public ICollection<Referee> Referees { get; set; } = null!;
        public ICollection<Team> Teams { get; set; } = null!;
    }
}

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

        public virtual ICollection<Skater> Skaters { get; set; } = null!;
        public virtual ICollection<Event> Events { get; set; } = null!;
        public virtual ICollection<Referee> Referees { get; set; } = null!;
        public virtual ICollection<Team> Teams { get; set; } = null!;
    }
}

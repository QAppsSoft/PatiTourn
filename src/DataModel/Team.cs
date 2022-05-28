using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModel
{

    [Table("Teams")]
    public class Team : BaseEntity
    {
        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public ICollection<Skater> Skaters { get; set; } = null!;

        //fully defined relationship
        public Guid CompetitionId { get; set; }
        public Competition Competition { get; set; } = null!;
    }
}

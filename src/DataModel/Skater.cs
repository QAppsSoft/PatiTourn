using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModel
{
    [Table("Skaters")]
    public class Skater : Person
    {
        public int Number { get; set; }

        //fully defined relationship
        public Guid CompetitionId { get; set; }
        public virtual Competition Competition { get; set; } = null!;

        //fully defined relationship
        public Guid TeamId { get; set; }
        public virtual Team Team { get; set; } = null!;
    }
}

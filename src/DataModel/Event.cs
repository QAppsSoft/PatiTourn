using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModel
{
    [Table("Events")]
    public abstract class Event : BaseEntity
    {
        public string Name { get; set; } = null!;

        public string Display { get; set; } = null!;

        public string Distance { get; set; } = null!;

        //fully defined relationship
        public Guid CompetitionId { get; set; }
        public virtual Competition Competition { get; set; } = null!;
    }
}

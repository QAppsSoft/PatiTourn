using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModel
{
    public abstract class IndividualEvent : Event
    {
        [NotMapped]
        public abstract IEnumerable<Position> Positions { get; }
    }
}
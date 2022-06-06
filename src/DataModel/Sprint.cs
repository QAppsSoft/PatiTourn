using System.Collections.Generic;

namespace DataModel
{
    public class Sprint : IndividualEvent
    {
        public virtual ICollection<SprintPosition> SprintPositions { get; set; } = null!;
        public override IEnumerable<Position> Positions => SprintPositions;
    }
}

using System;

namespace DataModel
{
    public class SprintPosition : Position
    {
        public TimeSpan? Time1 { get; set; }
        public TimeSpan? Time2 { get; set; }
        public TimeSpan? Time3 { get; set; }

        //fully defined relationship
        public Guid SprintId { get; set; }

        public Sprint Sprint { get; set; } = null!;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModel
{

    [Table("Teams")]
    public class Team : BaseEntity, IEquatable<Team>
    {
        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public virtual ICollection<Skater> Skaters { get; set; } = null!;

        //fully defined relationship
        public Guid CompetitionId { get; set; }
        public virtual Competition Competition { get; set; } = null!;

        public bool Equals(Team? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id == other.Id &&
                   string.Equals(Name, other.Name, StringComparison.Ordinal) &&
                   string.Equals(Description, other.Description, StringComparison.Ordinal) &&
                   CompetitionId.Equals(other.CompetitionId);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((Team)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Description, CompetitionId);
        }
    }
}

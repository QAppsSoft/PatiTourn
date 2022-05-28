using System.ComponentModel.DataAnnotations.Schema;
using DataModel.Enums;

namespace DataModel
{
    [Table("Referees")]
    public class Referee : Person
    {
        public RefereeCategory Category { get; set; }
    }
}
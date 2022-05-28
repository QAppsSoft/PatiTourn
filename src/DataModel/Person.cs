using System.ComponentModel.DataAnnotations.Schema;
using DataModel.Enums;

namespace DataModel
{
    [Table("Persons")]
    public abstract class Person : BaseEntity
    {
        public string Name { get; set; } = null!;

        public string LastNames { get; set; } = null!;

        public string IdentificationNumber { get; set; } = null!;

        public Sex Sex { get; set; }
    }
}

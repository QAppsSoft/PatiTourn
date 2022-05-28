using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModel
{
    [Table("Positions")]
    public class Position : BaseEntity
    {
        public TimeSpan? Time { get; set; }
    
        public int Points { get; set; }
    
        public int Place { get; set; }
    
        public string Observations { get; set; } = null!;

        //fully defined relationship
        public Guid SkaterId { get; set; }
        public Skater Skater { get; set; } = null!;
    }
}

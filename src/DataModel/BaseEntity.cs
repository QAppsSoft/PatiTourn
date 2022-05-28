using System;
using System.ComponentModel.DataAnnotations;

namespace DataModel
{
    public class BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
    }
}

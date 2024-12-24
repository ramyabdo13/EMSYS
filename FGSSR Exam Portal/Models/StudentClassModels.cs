using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EMSYS.Models
{
    public class StudentClass
    {
        [Key, Column(Order = 1)]
        [MaxLength(128)]
        public string StudentId { get; set; }
        [Key, Column(Order = 2)]
        [MaxLength(128)]
        public string ClassId { get; set; }
    }
}

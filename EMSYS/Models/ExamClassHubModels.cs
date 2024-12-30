using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EMSYS.Models
{
    public class ExamClassHub
    {
        [Key, Column(Order = 1)]
        [MaxLength(128)]
        public string ExamId { get; set; }
        [Key, Column(Order = 2)]
        [MaxLength(128)]
        public string ClassHubId { get; set; }
    }
}

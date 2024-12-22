using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EMSYS.Models
{
    public class ExamQuestion
    {
        [Key, Column(Order = 1)]
        [MaxLength(128)]
        public string ExamId { get; set; }
        [Key, Column(Order = 2)]
        [MaxLength(128)]
        public string QuestionId { get; set; }
        public int? QuestionOrder { get; set; }
        public decimal? Mark { get; set; }
    }
}

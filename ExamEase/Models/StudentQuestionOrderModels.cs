using System.ComponentModel.DataAnnotations;

namespace EMSYS.Models
{
    public class StudentQuestionOrder
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(128)]
        public string StudentId { get; set; }
        [MaxLength(128)]
        public string ExamId { get; set; }
        public string QuestionOrder { get; set; }
    }
}

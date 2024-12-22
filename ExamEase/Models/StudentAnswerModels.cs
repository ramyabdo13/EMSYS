using System.ComponentModel.DataAnnotations;

namespace EMSYS.Models
{
    public class StudentAnswer
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(128)]
        public string StudentId { get; set; }
        [MaxLength(128)]
        public string ExamId { get; set; }
        [MaxLength(128)]
        public string QuestionId { get; set; }
        [MaxLength(128)]
        public string AnswerId { get; set; }
        public string AnswerText { get; set; }
        public decimal? MarksObtained { get; set; }
    }
}

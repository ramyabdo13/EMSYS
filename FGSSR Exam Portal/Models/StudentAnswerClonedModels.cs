using System.ComponentModel.DataAnnotations;

namespace EMSYS.Models
{
    //The fields in StudentAnswerCloned is same as StudentAnswer and they are storing the exact same data
    //StudentAnswer table will be used for the students who are currently taking the exam
    //StudentAnswerCloned table will be mainly used for the students/admin/teacher to see their selected answers after the exam
    //Using 2 tables here is to separate the workload because if students/admin/teacher all CRUD to the same StudentAnswer table at the same time,
    //it may affect the student who are currently taking the exam and using the StudentAnswer table
    public class StudentAnswerCloned
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

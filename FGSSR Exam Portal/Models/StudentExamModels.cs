using System.ComponentModel.DataAnnotations;
using System;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using EMSYS.Resources;
using System.Linq;
using EMSYS.Models;

namespace EMSYS.Models
{
    public class StudentExam
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(128)]
        public string StudentId { get; set; }
        [MaxLength(128)]
        public string ExamId { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? EndedOn { get; set; }
        public string IsoUtcStartedOn { get; set; }
        public string IsoUtcEndedOn { get; set; }
        public decimal? Result { get; set; }
        public bool? Passed { get; set; }
    }

    public class StudentExamViewModel
    {
        public string Id { get; set; }
        public string StudentId { get; set; }
        public string ExamId { get; set; }
        public string ExamName { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? EndedOn { get; set; }
        public string IsoUtcStartedOn { get; set; }
        public string IsoUtcEndedOn { get; set; }
        public decimal? Result { get; set; } = 0;
        public int? QuestionNumber { get; set; } = 1;
        public string QuestionId { get; set; }
        public string QuestionText { get; set; }
        public List<AnswerOption> AnswerList { get; set; } //for MCQ
        public string SelectedAnswerId { get; set; }
        public string AnswerText { get; set; } //for Essay or Short answer question
        public string ImageFileName { get; set; }
        public string QuestionType { get; set; }
        public string StudentAnswerId { get; set; }
        public string AnswerId { get; set; }
        public int TotalQuestion { get; set; } = 0;
        public int? Duration { get; set; }
        public decimal? Mark { get; set; } = 0;
        public bool? ReleaseAnswer { get; set; } = false;
        public decimal? EssayMark { get; set; } = 0;
        public ResultViewModel ResultView { get; set; } = new ResultViewModel();
        public List<string> CorrectAnswerText { get; set; } //short answer
        public decimal? MarksObtained { get; set; } = 0;
        public int? AnsweredUntil { get; set; } = 1;
    }

    public class ResultViewModel
    {
        public string ExamId { get; set; }
        public string StudentId { get; set; }
        [Display(Name = "ExamName", ResourceType = typeof(Resource))]
        public string ExamName { get; set; }
        [Display(Name = "StudentName", ResourceType = typeof(Resource))]
        public string StudentName { get; set; } = "-";
        [Display(Name = "YourScore", ResourceType = typeof(Resource))]
        public decimal? YourScore { get; set; } = 0;
        [Display(Name = "TotalScore", ResourceType = typeof(Resource))]
        public decimal? TotalScore { get; set; } = 0;
        [Display(Name = "AnsweredCorrect", ResourceType = typeof(Resource))]
        public int? AnsweredCorrect { get; set; } = 0;
        public int? TotalQuestions { get; set; } = 0;
        [Display(Name = "ScoreToPass", ResourceType = typeof(Resource))]
        public decimal? ScoreToPass { get; set; } = 0;
        [Display(Name = "ResultStatus", ResourceType = typeof(Resource))]
        public bool? Passed { get; set; } = false;
        [Display(Name = "StartDateTime", ResourceType = typeof(Resource))]
        public string StartDateTime { get; set; } = "-";
        [Display(Name = "EndDateTime", ResourceType = typeof(Resource))]
        public string EndDateTime { get; set; } = "-";
        [Display(Name = "TimeTaken", ResourceType = typeof(Resource))]
        public string TimeTaken { get; set; } = "0";
        public bool? ReleaseAnswer { get; set; } = false;
        public List<string> StudentClassIdList { get; set; }
        [Display(Name = "Class", ResourceType = typeof(Resource))]
        public List<string> StudentClassNameList { get; set; }
        [Display(Name = "Class", ResourceType = typeof(Resource))]
        public string StudentClass { get; set; } = "-";
    }

    public class StudentExamResultList
    {
        public List<StudentExamViewModel> Listing { get; set; }
        public ResultViewModel ResultView { get; set; } = new ResultViewModel();
    }
}

using System.ComponentModel.DataAnnotations;
using System;
using EMSYS.Resources;
using System.Collections.Generic;
using EMSYS.Models;

namespace EMSYS.Models
{
    public class Answer
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        public string QuestionId { get; set; }
        public string AnswerText { get; set; }
        public bool IsCorrect { get; set; }
        public int? AnswerOrder { get; set; }
        [MaxLength(128)]
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        [MaxLength(128)]
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string IsoUtcCreatedOn { get; set; }
        public string IsoUtcModifiedOn { get; set; }
    }

    public class AnswerViewModel
    {
        public string Id { get; set; }
        public string QuestionId { get; set; }
        public string QuestionTypeId { get; set; }
        public string QuestionTypeCode { get; set; }
        [Display(Name = "QuestionType", ResourceType = typeof(Resource))]
        public string QuestionTypeName { get; set; }
        [Display(Name = "QuestionTitle", ResourceType = typeof(Resource))]
        public string QuestionTitle { get; set; }
        [Display(Name = "Answer", ResourceType = typeof(Resource))]
        public string[] AnswerText { get; set; }
        [Display(Name = "IsCorrect", ResourceType = typeof(Resource))]
        public bool IsCorrect { get; set; }
        public string CorrectAnswer { get; set; }
        public int? AnswerOrder { get; set; }
        public CreatedAndModifiedViewModel CreatedAndModified { get; set; }
        public List<AnswerOption> AnswerOptions { get; set; }
    }

    public class AnswerOption
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public int? Order { get; set; }
        public bool Selected { get; set; } = false;
        public bool? IsCorrect { get; set; } = false;
    }
}

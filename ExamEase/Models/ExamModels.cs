using System.ComponentModel.DataAnnotations;
using System;
using EMSYS.Resources;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace EMSYS.Models
{
    public class Exam
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(256)]
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public string IsoUtcStartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string IsoUtcEndDate { get; set; }
        public int? Duration { get; set; }
        public bool? ReleaseAnswer { get; set; } = false;
        public bool? IsActive { get; set; }
        public decimal? MarksToPass { get; set; }
        public bool? RandomizeQuestions { get; set; }
        [MaxLength(128)]
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        [MaxLength(128)]
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string IsoUtcCreatedOn { get; set; }
        public string IsoUtcModifiedOn { get; set; }
        public bool? IsPublished { get; set; }
    }
    public class ExamViewModel
    {
        public string Id { get; set; }
        [Required(ErrorMessageResourceName = "FieldIsRequired", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Name", ResourceType = typeof(Resource))]
        public string Name { get; set; }
        [Required(ErrorMessageResourceName = "FieldIsRequired", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Description", ResourceType = typeof(Resource))]
        public string Description { get; set; }
        [Display(Name = "TotalMarks", ResourceType = typeof(Resource))]
        public decimal? TotalMark { get; set; }
        [Display(Name = "StartDateTime", ResourceType = typeof(Resource))]
        public DateTime? StartDate { get; set; }
        [Display(Name = "StartDateTime", ResourceType = typeof(Resource))]
        public string StartDateIsoString { get; set; }
        [Display(Name = "StartDateTime", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceName = "FieldIsRequired", ErrorMessageResourceType = typeof(Resource))]
        public string StartDateIsoUtc { get; set; }
        [Display(Name = "EndDateTime", ResourceType = typeof(Resource))]
        public DateTime? EndDate { get; set; }
        [Display(Name = "EndDateTime", ResourceType = typeof(Resource))]
        public string EndDateIsoString { get; set; }
        [Display(Name = "EndDateTime", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceName = "FieldIsRequired", ErrorMessageResourceType = typeof(Resource))]
        public string EndDateIsoUtc { get; set; }
        [Display(Name = "CreatedOn", ResourceType = typeof(Resource))]
        public DateTime? CreatedOn { get; set; }
        [Display(Name = "CreatedOn", ResourceType = typeof(Resource))]
        public string CreatedOnIsoUtc { get; set; }
        [Display(Name = "ModifiedOn", ResourceType = typeof(Resource))]
        public DateTime? ModifiedOn { get; set; }
        [Display(Name = "ModifiedOn", ResourceType = typeof(Resource))]
        public string ModifiedOnIsoUtc { get; set; }
        [Required(ErrorMessageResourceName = "FieldIsRequired", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "DurationInMinute", ResourceType = typeof(Resource))]
        public int? Duration { get; set; }
        [Display(Name = "ReleaseAnswer", ResourceType = typeof(Resource))]
        public bool? ReleaseAnswer { get; set; } = false;
        [Required(ErrorMessageResourceName = "FieldIsRequired", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "MarksToPass", ResourceType = typeof(Resource))]
        public decimal? MarksToPass { get; set; }
        [Required(ErrorMessageResourceName = "FieldIsRequired", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "RandomizeQuestions", ResourceType = typeof(Resource))]
        public bool? RandomizeQuestions { get; set; }
        public CreatedAndModifiedViewModel CreatedAndModified { get; set; }
        [Required(ErrorMessageResourceName = "FieldIsRequired", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Class", ResourceType = typeof(Resource))]
        public List<string> ClassIdList { get; set; }
        public List<SelectListItem> ClassIdSelectList { get; set; }
        [Display(Name = "Class", ResourceType = typeof(Resource))]
        public string ClassName { get; set; }
        [Required(ErrorMessageResourceName = "FieldIsRequired", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Subject", ResourceType = typeof(Resource))]
        public List<string> SubjectIdList { get; set; }
        public List<SelectListItem> SubjectIdSelectList { get; set; }
        [Display(Name = "Subject", ResourceType = typeof(Resource))]
        public string SubjectName { get; set; }
        public List<string> SelectedQuestions { get; set; }
        public QuestionListing QuestionListing { get; set; }
        [Display(Name = "TotalQuestions", ResourceType = typeof(Resource))]
        public int? TotalQuestions { get; set; }
        [Display(Name = "ActiveInactive", ResourceType = typeof(Resource))]
        public string IsActive { get; set; }
        public List<SelectListItem> ActiveInactiveSelectlist { get; set; }
        [Display(Name = "Status", ResourceType = typeof(Resource))]
        public string ExamStatus { get; set; }
        public string StudentExamStatus { get; set; }
        public string StudentId { get; set; }
        public decimal?[] MarkForEachQuestions { get; set; }
        public string[] QuestionIds { get; set; }
        public bool CanPublishNow { get; set; } = false;
        public bool AlreadyStarted { get; set; } = false;
        public bool? IsPublished { get; set; } = false;
        public string CreatedBy { get; set; }
        //These are used in the exam listing for search/sort/pagination etc.
        [Display(Name = "Actions", ResourceType = typeof(Resource))]
        public string Actions { get; set; }
        [Display(Name = "Result", ResourceType = typeof(Resource))]
        public decimal? Result { get; set; }
    }

    public class ExamListing
    {
        public List<ExamViewModel> Listing { get; set; }
        public string StudentExamStatus { get; set; }
    }

    public class ExamResultViewModel
    {
        public string Id { get; set; }
        [Display(Name = "ExamName", ResourceType = typeof(Resource))]
        public string ExamName { get; set; }
        [Display(Name = "MarksToPass", ResourceType = typeof(Resource))]
        public decimal? MarksToPass { get; set; } = 0;
        [Display(Name = "StudentPassed", ResourceType = typeof(Resource))]
        public int? StudentPassed { get; set; } = 0;
        [Display(Name = "StudentFailed", ResourceType = typeof(Resource))]
        public int? StudentFailed { get; set; } = 0;
        [Display(Name = "StudentNotStarted", ResourceType = typeof(Resource))]
        public int? StudentNotStarted { get; set; } = 0;
        [Display(Name = "StartDateTime", ResourceType = typeof(Resource))]
        public DateTime? StartDate { get; set; }
        [Display(Name = "StartDateTime", ResourceType = typeof(Resource))]
        public string StartDateIsoUtc { get; set; }
    }

}

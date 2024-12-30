using System.ComponentModel.DataAnnotations;
using System;
using EMSYS.Resources;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace EMSYS.Models
{
    public class Question
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        public string QuestionTitle { get; set; }
        [MaxLength(128)]
        public string SubjectId { get; set; }
        [MaxLength(128)]
        public string QuestionTypeId { get; set; }
        public bool? IsActive { get; set; }
        [MaxLength(128)]
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        [MaxLength(128)]
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string IsoUtcCreatedOn { get; set; }
        public string IsoUtcModifiedOn { get; set; }
    }

    public class QuestionViewModel
    {
        public string Id { get; set; }
        [Required(ErrorMessageResourceName = "FieldIsRequired", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "QuestionTitle", ResourceType = typeof(Resource))]
        public string QuestionTitle { get; set; }
        [MaxLength(128)]
        [Required(ErrorMessageResourceName = "FieldIsRequired", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Subject", ResourceType = typeof(Resource))]
        public string SubjectId { get; set; }
        public List<SelectListItem> SubjectIdSelectList { get; set; }
        [MaxLength(128)]
        [Required(ErrorMessageResourceName = "FieldIsRequired", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "QuestionType", ResourceType = typeof(Resource))]
        public string QuestionTypeId { get; set; }
        public List<SelectListItem> QuestionTypeIdSelectList { get; set; }
        [Display(Name = "Subject", ResourceType = typeof(Resource))]
        public string SubjectName { get; set; }
        [Display(Name = "QuestionType", ResourceType = typeof(Resource))]
        public string QuestionTypeName { get; set; }
        public string QuestionTypeCode { get; set; }
        [Required(ErrorMessageResourceName = "FieldIsRequired", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Status", ResourceType = typeof(Resource))]
        public string IsActive { get; set; } = "Active";
        public List<SelectListItem> ActiveInactiveSelectList { get; set; }
        [MaxLength(256)]
        [Display(Name = "QuestionImage", ResourceType = typeof(Resource))]
        public string ImageFileName { get; set; }
        [MaxLength(256)]
        public string ImageUniqueFileName { get; set; }
        public string ImageFileUrl { get; set; }
        public CreatedAndModifiedViewModel CreatedAndModified { get; set; }
        [Display(Name = "QuestionImage", ResourceType = typeof(Resource))]
        public IFormFile ImageFile { get; set; }
        public DateTime? CreatedOn { get; set; }
        [Display(Name = "CreatedOn", ResourceType = typeof(Resource))]
        public string IsoUtcCreatedOn { get; set; }
        [Display(Name = "CreatedOn", ResourceType = typeof(Resource))]
        public string FormattedCreatedOn { get; set; }
        public int? FormattedCreatedOnOrder { get; set; }
        public decimal? ExamQuestionMark { get; set; }
        [Display(Name = "AnswerSaved", ResourceType = typeof(Resource))]
        public bool? AnswerSaved { get; set; }
        public bool? CanDelete { get; set; }
        [Display(Name = "Actions", ResourceType = typeof(Resource))]
        public string Actions { get; set; }
    }

    public class QuestionListing
    {
        public List<QuestionViewModel> Listing { get; set; }
        public List<SelectListItem> SubjectIdSelectList { get; set; }
        public List<SelectListItem> QuestionTypeIdSelectList { get; set; }
    }
}

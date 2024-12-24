using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using EMSYS.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EMSYS.Models
{
    public class UserAttachment
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(128)]
        public string UserProfileId { get; set; }
        public string FileUrl { get; set; }
        public string FileName { get; set; }
        public string UniqueFileName { get; set; }
        [MaxLength(128)]
        public string AttachmentTypeId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string IsoUtcCreatedOn { get; set; }
        public string IsoUtcModifiedOn { get; set; }
    }

    public class UserAttachmentViewModel
    {
        public string Id { get; set; }
        public string UserProfileId { get; set; }
        public string AspNetUserId { get; set; }
        [Display(Name = "FileName", ResourceType = typeof(Resource))]
        public string FileName { get; set; }
        [Display(Name = "FileURL", ResourceType = typeof(Resource))]
        public string FileUrl { get; set; }
        public string UniqueCode { get; set; }
        [Display(Name = "AttachmentType", ResourceType = typeof(Resource))]
        public string AttachmentTypeId { get; set; }
        [Display(Name = "AttachmentType", ResourceType = typeof(Resource))]
        public string AttachmentTypeName { get; set; }
        public IFormFile[] Files { get; set; }
        public string Username { get; set; }
        [Display(Name = "FullName", ResourceType = typeof(Resource))]
        public string FullName { get; set; }
        [Display(Name = "UploadedOn", ResourceType = typeof(Resource))]
        public DateTime? UploadedOn { get; set; }
        public int? UploadedOnOrder { get; set; }
        public string IsoUtcUploadedOn { get; set; }
        [Display(Name = "UploadedBy", ResourceType = typeof(Resource))]
        public string UploadedBy { get; set; }
        public CreatedAndModifiedViewModel CreatedAndModified { get; set; }
        public List<SelectListItem> UserAttachmentTypeSelectList { get; set; }
        [Display(Name = "Actions", ResourceType = typeof(Resource))]
        public string Actions { get; set; }
    }

    public class UserAttachmentListing
    {
        public List<UserAttachmentViewModel> Listing { get; set; }
        [Display(Name = "AttachmentType", ResourceType = typeof(Resource))]
        public List<SelectListItem> UserAttachmentTypeSelectList { get; set; }
        public string UserProfileId { get; set; }
        public string AttachmentTypeId { get; set; }
        public string Username { get; set; }
        [Display(Name = "FullName", ResourceType = typeof(Resource))]
        public string FullName { get; set; }
    }
}
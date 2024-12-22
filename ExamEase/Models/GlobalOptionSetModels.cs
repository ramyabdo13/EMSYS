using EMSYS.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EMSYS.Models
{
    public class GlobalOptionSet
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        public string Code { get; set; }
        public string DisplayName { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public int? OptionOrder { get; set; }
        [MaxLength(128)]
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        [MaxLength(128)]
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool SystemDefault { get; set; }
        public string IsoUtcCreatedOn { get; set; }
        public string IsoUtcModifiedOn { get; set; }
    }

    public class GlobalOptionSetViewModel
    {
        public string Id { get; set; }
        public string Code { get; set; }
        [Required]
        [Display(Name = "Name", ResourceType = typeof(Resource))]
        [RegularExpression(@"^[a-zA-Z0-9 ]+$", ErrorMessageResourceName = "OnlyLettersAndNumbers", ErrorMessageResourceType = typeof(Resource))]
        public string DisplayName { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        [Required]
        [Display(Name = "DisplayOrder", ResourceType = typeof(Resource))]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceName = "OnlyNumbers", ErrorMessageResourceType = typeof(Resource))]
        public int? OptionOrder { get; set; }
        public CreatedAndModifiedViewModel CreatedAndModified { get; set; }
        public bool SystemDefault { get; set; }
        public string IsoUtcCreatedOn { get; set; }
        public string IsoUtcModifiedOn { get; set; }
        [Display(Name = "Actions", ResourceType = typeof(Resource))]
        public string Actions { get; set; }
    }
}
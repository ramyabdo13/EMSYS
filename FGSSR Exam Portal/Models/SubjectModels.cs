using System.ComponentModel.DataAnnotations;
using System;
using EMSYS.Resources;
using System.Collections.Generic;

namespace EMSYS.Models
{
    public class Subject
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(256)]
        public string Name { get; set; }
        [MaxLength(128)]
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        [MaxLength(128)]
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string IsoUtcCreatedOn { get; set; }
        public string IsoUtcModifiedOn { get; set; }
    }

    public class SubjectViewModel
    {
        public string Id { get; set; }
        [MaxLength(256)]
        [Required(ErrorMessageResourceName = "FieldIsRequired", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Name", ResourceType = typeof(Resource))]
        public string Name { get; set; }
        public CreatedAndModifiedViewModel CreatedAndModified { get; set; }
        [Display(Name = "Actions", ResourceType = typeof(Resource))]
        public string Actions { get; set; }
    }
}

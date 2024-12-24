using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EMSYS.Resources;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace EMSYS.Models
{
    public class AspNetRoles : IdentityRole<string>
    {
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

    public class SystemRoleViewModel
    {
        public string Id { get; set; }
        [Required]
        [MaxLength(256)]
        [RegularExpression(@"^[a-zA-Z0-9 ]+$", ErrorMessageResourceName = "OnlyLettersAndNumbers", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "RoleName", ResourceType = typeof(Resource))]
        public string Name { get; set; }
        [Display(Name = "CreatedBy", ResourceType = typeof(Resource))]
        public string CreatedBy { get; set; }
        [Display(Name = "CreatedOn", ResourceType = typeof(Resource))]
        public DateTime? CreatedOn { get; set; }
        public string IsoUtcCreatedOn { get; set; }
        [Display(Name = "ModifiedBy", ResourceType = typeof(Resource))]
        public string ModifiedBy { get; set; }
        [Display(Name = "ModifiedOn", ResourceType = typeof(Resource))]
        public DateTime? ModifiedOn { get; set; }
        public string IsoUtcModifiedOn { get; set; }
        public bool SystemDefault { get; set; }
        public CreatedAndModifiedViewModel CreatedAndModified { get; set; }
        [Display(Name = "Dashboard", ResourceType = typeof(Resource))]
        public Permission DashboardPermission { get; set; }
        [Display(Name = "UserStatus", ResourceType = typeof(Resource))]
        public Permission UserStatusPermission { get; set; }
        [Display(Name = "UserAttachmentType", ResourceType = typeof(Resource))]
        public Permission UserAttachmentTypePermission { get; set; }
        [Display(Name = "RoleManagement", ResourceType = typeof(Resource))]
        public Permission RoleManagementPermission { get; set; }
        [Display(Name = "UserManagement", ResourceType = typeof(Resource))]
        public Permission UserManagementPermission { get; set; }
        [Display(Name = "LoginHistory", ResourceType = typeof(Resource))]
        public Permission LoginHistoryPermission { get; set; }
    }

    public class Permission
    {
        [Display(Name = "ViewList", ResourceType = typeof(Resource))]
        public ViewPermission ViewPermission { get; set; }
        [Display(Name = "Add", ResourceType = typeof(Resource))]
        public AddPermission AddPermission { get; set; }
        [Display(Name = "Edit", ResourceType = typeof(Resource))]
        public EditPermission EditPermission { get; set; }
        [Display(Name = "Delete", ResourceType = typeof(Resource))]
        public DeletePermission DeletePermission { get; set; }
    }

    public class ViewPermission
    {
        public string Type { get; set; }
        public bool IsSelected { get; set; }
    }
    public class AddPermission
    {
        public string Type { get; set; }
        public bool IsSelected { get; set; }
    }
    public class EditPermission
    {
        public string Type { get; set; }
        public bool IsSelected { get; set; }
    }
    public class DeletePermission
    {
        public string Type { get; set; }
        public bool IsSelected { get; set; }
    }
}
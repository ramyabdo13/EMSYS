using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using EMSYS.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;

namespace EMSYS.Models
{
    public class UserProfile
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        public string AspNetUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string IDCardNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        [MaxLength(128)]
        public string GenderId { get; set; }
        public string CountryName { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        [MaxLength(128)]
        public string UserStatusId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string IsoUtcDateOfBirth { get; set; }
        public string IsoUtcCreatedOn { get; set; }
        public string IsoUtcModifiedOn { get; set; }
        public int IntakeYear { get; set; }
        public string Code { get; set; }
    }

    public class UserProfileViewModel
    {
        public string Id { get; set; }
        public string AspNetUserId { get; set; }
        [Display(Name = "FirstName", ResourceType = typeof(Resource))]
        public string FirstName { get; set; }
        [Display(Name = "LastName", ResourceType = typeof(Resource))]
        public string LastName { get; set; }
        [Required]
        [Display(Name = "FullName", ResourceType = typeof(Resource))]
        public string FullName { get; set; }
        [Required]
        [RegularExpression("^[A-Za-z]\\w{3,29}$", ErrorMessageResourceName = "InvalidUsername", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Username", ResourceType = typeof(Resource))]
        public string Username { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Resource))]
        [PasswordValidation]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(Resource))]
        [Compare("Password", ErrorMessageResourceName = "PasswordNotMatch", ErrorMessageResourceType = typeof(Resource))]
        public string ConfirmPassword { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "EmailAddress", ResourceType = typeof(Resource))]
        public string EmailAddress { get; set; }
        [Display(Name = "IDCardNumber", ResourceType = typeof(Resource))]
        public string IDCardNumber { get; set; }
        [Display(Name = "DateOfBirth", ResourceType = typeof(Resource))]
        public DateTime? DateOfBirth { get; set; }
        [Display(Name = "DateOfBirth", ResourceType = typeof(Resource))]
        public string IsoUtcDateOfBirth { get; set; }
        [Display(Name = "Gender", ResourceType = typeof(Resource))]
        public string GenderId { get; set; }
        [Display(Name = "Gender", ResourceType = typeof(Resource))]
        public string GenderName { get; set; }
        [Display(Name = "Gender", ResourceType = typeof(Resource))]
        public List<SelectListItem> GenderSelectList { get; set; }
        [Required]
        [Display(Name = "Country", ResourceType = typeof(Resource))]
        public string CountryName { get; set; }
        [Display(Name = "Country", ResourceType = typeof(Resource))]
        public List<SelectListItem> CountrySelectList { get; set; }
        [Display(Name = "FullAddress", ResourceType = typeof(Resource))]
        public string Address { get; set; }
        [Display(Name = "PostalCode", ResourceType = typeof(Resource))]
        [RegularExpression("^[0-9]*$", ErrorMessageResourceName = "InvalidPostalCode", ErrorMessageResourceType = typeof(Resource))]
        public string PostalCode { get; set; }
        //https://uibakery.io/regex-library/phone-number
        [Required]
        [RegularExpression("^\\+?\\d{1,4}?[-.\\s]?\\(?\\d{1,3}?\\)?[-.\\s]?\\d{1,4}[-.\\s]?\\d{1,4}[-.\\s]?\\d{1,9}$", ErrorMessageResourceName = "InvalidPhoneNumber", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "PhoneNumber", ResourceType = typeof(Resource))]
        public string PhoneNumber { get; set; }
        [Required]
        [Display(Name = "Status", ResourceType = typeof(Resource))]
        public string UserStatusId { get; set; }
        [Display(Name = "Status", ResourceType = typeof(Resource))]
        public string UserStatusName { get; set; }
        [Display(Name = "Status", ResourceType = typeof(Resource))]
        public List<SelectListItem> UserStatusSelectList { get; set; }
        public string CreatedBy { get; set; }

        [Display(Name = "CreatedOn", ResourceType = typeof(Resource))]
        public DateTime? CreatedOn { get; set; }
        [Display(Name = "CreatedOn", ResourceType = typeof(Resource))]
        public string FormattedCreatedOn { get; set; }
        public int FormattedCreatedOnOrder { get; set; }
        [Display(Name = "CreatedOn", ResourceType = typeof(Resource))]
        public string IsoUtcCreatedOn { get; set; }

        [Display(Name = "ModifiedBy", ResourceType = typeof(Resource))]
        public string ModifiedBy { get; set; }

        [Display(Name = "ModifiedOn", ResourceType = typeof(Resource))]
        public DateTime? ModifiedOn { get; set; }
        public string IsoUtcModifiedOn { get; set; }

        [Display(Name = "Role", ResourceType = typeof(Resource))]
        public List<SelectListItem> UserRoleSelectList { get; set; }
        [Required]
        [Display(Name = "Role", ResourceType = typeof(Resource))]
        public string UserRoleName { get; set; }
        [Display(Name = "ProfilePicture", ResourceType = typeof(Resource))]
        public IFormFile ProfilePicture { get; set; }
        public string ProfilePictureFileName { get; set; }
        public CreatedAndModifiedViewModel CreatedAndModified { get; set; }
        public IFormFile[] Files { get; set; }
        [Display(Name = "IntakeYear", ResourceType = typeof(Resource))]
        public int IntakeYear { get; set; }
        [Display(Name = "Code", ResourceType = typeof(Resource))]
        public string Code { get; set; }
        [Display(Name = "Class", ResourceType = typeof(Resource))]
        public List<string> ClassIdList { get; set; }
        public List<SelectListItem> ClassSelectList { get; set; }
        [Display(Name = "Actions", ResourceType = typeof(Resource))]
        public string Actions { get; set; }
    }

    public class UserProfileListing
    {
        public List<UserProfileViewModel> Listing { get; set; }
        public List<SelectListItem> RoleSelectList { get; set; }
    }

    public class AdminChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "NewPassword", ResourceType = typeof(Resource))]
        [PasswordValidation]
        public string NewPassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(Resource))]
        [Compare("NewPassword", ErrorMessageResourceName = "PasswordNotMatch", ErrorMessageResourceType = typeof(Resource))]
        public string ConfirmPassword { get; set; }
        public string Id { get; set; }
        public string AspNetUserId { get; set; }
        [Display(Name = "FullName", ResourceType = typeof(Resource))]
        public string FullName { get; set; }
        [Display(Name = "Username", ResourceType = typeof(Resource))]
        public string Username { get; set; }
        [Display(Name = "EmailAddress", ResourceType = typeof(Resource))]
        public string EmailAddress { get; set; }
    }

    public class ImportFromExcel
    {
        [Display(Name = "File", ResourceType = typeof(Resource))]
        public IFormFile File { get; set; }
        public List<ImportFromExcelError> ErrorList { get; set; }
        public string UploadResult { get; set; }
    }
}
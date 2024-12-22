using EMSYS.Models;
using EMSYS.Resources;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EMSYS.Models
{
    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "EmailAddress", ResourceType = typeof(Resource))]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Username", ResourceType = typeof(Resource))]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Resource))]
        public string Password { get; set; }

        [Display(Name = "Rememberme", ResourceType = typeof(Resource))]
        public bool RememberMe { get; set; } = false;
    }

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Username", ResourceType = typeof(Resource))]
        [MaxLength(256, ErrorMessageResourceName = "MaxLength256", ErrorMessageResourceType = typeof(Resource))]
        [RegularExpression("^[A-Za-z]\\w{3,29}$", ErrorMessageResourceName = "InvalidUsername", ErrorMessageResourceType = typeof(Resource))]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256, ErrorMessageResourceName = "MaxLength256", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "EmailAddress", ResourceType = typeof(Resource))]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessageResourceName = "MinLength6", ErrorMessageResourceType = typeof(Resource), MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Resource))]
        [PasswordValidation]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(Resource))]
        [Compare("Password", ErrorMessageResourceName = "PasswordNotMatch", ErrorMessageResourceType = typeof(Resource))]
        public string ConfirmPassword { get; set; }
        public bool NoUserYet { get; set; }
        [Required]
        [Display(Name = "Role", ResourceType = typeof(Resource))]
        public string RoleName { get; set; }
        public List<SelectListItem> RoleNameSelectList { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "EmailAddress", ResourceType = typeof(Resource))]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessageResourceName = "MinLength6", ErrorMessageResourceType = typeof(Resource), MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Resource))]
        [PasswordValidation]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(Resource))]
        [Compare("Password", ErrorMessageResourceName = "PasswordNotMatch", ErrorMessageResourceType = typeof(Resource))]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "EmailAddress", ResourceType = typeof(Resource))]
        public string Email { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "CurrentPassword", ResourceType = typeof(Resource))]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessageResourceName = "MinLength6", ErrorMessageResourceType = typeof(Resource), MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "NewPassword", ResourceType = typeof(Resource))]
        [PasswordValidation]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmNewPassword", ResourceType = typeof(Resource))]
        [Compare("NewPassword", ErrorMessageResourceName = "NewPasswordNotMatch", ErrorMessageResourceType = typeof(Resource))]
        public string ConfirmPassword { get; set; }
    }

}

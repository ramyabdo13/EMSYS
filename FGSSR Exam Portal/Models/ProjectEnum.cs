using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace EMSYS.Models
{
    //project default enum

    public class ProjectEnum
    {

        public enum ModuleCode
        {
            UserStatus,
            UserAttachmentType,
            RoleManagement,
            UserManagement,
            LoginHistory,
            Dashboard
        }

        public enum UserAttachment
        {
            ProfilePicture
        }

        public enum Gender
        {
            Female,
            Male,
            Other
        }

        public enum UserStatus
        {
            Registered,
            Validated,
            NotValidated,
            Banned
        }

        public enum EmailTemplate
        {
            ConfirmEmail,
            PasswordResetByAdmin
        }

        public enum ExamStatus
        {
            Draft,
            Published,
            [Display(Name = "On Going")]
            OnGoing,
            Ended
        }

        public enum StudentExamStatus
        {
            Upcoming,
            Current,
            Past
        }

    }

}
using Microsoft.AspNetCore.Mvc.Rendering;
using EMSYS.Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace EMSYS.Models
{
    public class CreatedAndModifiedViewModel
    {
        [Display(Name = "CreatedBy", ResourceType = typeof(Resource))]
        public string CreatedByName { get; set; }
        [Display(Name = "CreatedOn", ResourceType = typeof(Resource))]
        public string FormattedCreatedOn { get; set; }
        [Display(Name = "ModifiedBy", ResourceType = typeof(Resource))]
        public string ModifiedByName { get; set; }
        [Display(Name = "ModifiedOn", ResourceType = typeof(Resource))]
        public string FormattedModifiedOn { get; set; }
        public int? OrderCreatedOn { get; set; }
        public int? OrderModifiedOn { get; set; }
    }

    [Serializable]
    public class ImportFromExcelError
    {
        public string Row { get; set; }
        public List<string> Errors { get; set; }
    }

    public class UserInRoleListing
    {
        public List<UserInRoleViewModel> Listing { get; set; }
        public string RoleName { get; set; }
    }

    public class UserInRoleViewModel
    {
        public string Username { get; set; }
        [Display(Name = "FullName", ResourceType = typeof(Resource))]
        public string FullName { get; set; }
        public string UserProfileId { get; set; }
    }

    public class DashboardViewModel
    {
        public int? TotalTeacher { get; set; } = 0;
        public int? TotalStudent { get; set; } = 0;
        public int? ExamInProgress { get; set; } = 0;
        public int? ExamCompleted { get; set; } = 0;
        public int? TotalExam { get; set; } = 0;
        public int? ExamPassed { get; set; } = 0;
        public int? ExamFailed { get; set; } = 0;
        public List<UpcomingExamChart> UpcomingExamCharts { get; set; }
        public List<ResultViewModel> StudentBestPerformanceExams { get; set; }
        public List<ResultViewModel> StudentWeakPerformanceExams { get; set; }
        public List<SelectListItem> ExamSelectList { get; set; }
        public string DefaultExamToDisplayTopTenStudent { get; set; }
    }

    public class IntChart
    {
        public string DataLabel { get; set; }
        public int DataValue { get; set; }
    }

    public class DecimalChart
    {
        public string DataLabel { get; set; }
        public decimal? DataValue { get; set; }
    }

    public class UpcomingExamChart
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string IsoUtcStartDate { get; set; }
        public DateTime? StartDate { get; set; }
        public TimeSpan Countdown { get; set; }
        public string CountdownHour { get; set; }
        public string CountdownMinute { get; set; }
        public string CreatedById { get; set; }
    }

    public class FormControlDemo
    {
        public List<SelectListItem> CategoryList { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Biography { get; set; }
    }

    public class FileModel
    {
        public string FileName { get; set; }
        public string UniqueFileName { get; set; }
        public string FileUrl { get; set; }
    }

    public class ColumnHeader
    {
        public string Title { get; set; }
        public string OrderAction { get; set; } //asc or desc
        public int Index { get; set; }
        public string Key { get; set; }
    }

    public class ActionColumn
    {
        [Display(Name = "Actions", ResourceType = typeof(Resource))]
        public string Actions { get; set; }
    }

}
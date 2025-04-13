using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using EMSYS.Models;
using EMSYS.Resources;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using static EMSYS.Controllers.LoginHistoryController;

namespace EMSYS.Utils
{
    public static class ListUtil
    {
        public static string DateTimeColumnWidth = "170px";
        public static string IntColumnWidth = "170px";

        public static ColumnHeader[] GenerateDefaultColumnHeaders<T>(string defaultSortOrder, List<string> TableColumns) where T : class
        {
            var headers = new List<ColumnHeader>();
            int count = 1;
            foreach (var property in TableColumns)
            {
                ColumnHeader columnHeader = new ColumnHeader();
                columnHeader.Index = count;
                columnHeader.Key = property;

                var parameter = Expression.Parameter(typeof(T), "s");

                var propertyParts = property.Split('.');
                if (propertyParts.Length > 1)
                {
                    // The property is nested (example: StudentViewModel.StudentNameModel.Value), so we need to generate a chain of property expressions
                    Expression memberExpression = parameter;
                    foreach (var part in propertyParts)
                    {
                        memberExpression = Expression.Property(memberExpression, part);
                    }
                    // Retrieve the chain of member expressions
                    var firstExpression = GetMemberExpressions(memberExpression).FirstOrDefault();
                    if (firstExpression != null)
                    {
                        var displayAttribute = (DisplayAttribute?)firstExpression.Member
                            .GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();
                        if (displayAttribute != null)
                        {
                            string displayName = displayAttribute.GetName() ?? "";
                            columnHeader.Title = displayName;
                        }
                    }
                }
                else
                {
                    var memberExpression = Expression.Property(parameter, property);

                    // Ensure memberExpression.Member is not null before attempting to get custom attributes
                    var memberInfo = (memberExpression as MemberExpression)?.Member;

                    if (memberInfo != null)
                    {
                        var displayAttribute = (DisplayAttribute?)memberInfo.GetCustomAttributes(typeof(DisplayAttribute), true)
                            .FirstOrDefault();

                        string propertyName = displayAttribute?.GetName() ?? property;
                        string displayName = propertyName ?? "";
                        columnHeader.Title = displayName;
                    }
                    else
                    {
                        // Fallback if memberInfo is null, just use property
                        columnHeader.Title = property;
                    }
                }

                if (defaultSortOrder.Contains(property))
                {
                    string[] segments = defaultSortOrder.Split('-');
                    string order = segments.Last();
                    columnHeader.OrderAction = order == "desc" ? $"{property}-asc" : $"{property}-desc";
                }
                else
                {
                    columnHeader.OrderAction = $"{property}-desc";
                }

                headers.Add(columnHeader);
                count++;
            }
            return headers.ToArray();
        }

        public static IEnumerable<MemberExpression> GetMemberExpressions(Expression expression)
        {
            var memberExpressions = new List<MemberExpression>();

            // Continue looping as long as the expression is a MemberExpression and has a non-null Expression property
            while (expression is MemberExpression memberExpr && memberExpr.Expression != null)
            {
                memberExpressions.Insert(0, memberExpr); // Insert at the beginning to keep the correct order
                expression = memberExpr.Expression;
            }

            // Handle the last member expression if it has reached the root (no parent expression)
            if (expression is MemberExpression lastMemberExpr)
            {
                memberExpressions.Insert(0, lastMemberExpr);
            }

            return memberExpressions;
        }

        public static IQueryable<T> PerformSort<T>(IQueryable<T> list, string defaultSortOrder, string sort) where T : class
        {
            if (string.IsNullOrEmpty(sort))
            {
                sort = defaultSortOrder;
            }

            string[] segments = sort.Split('-');
            string column = segments[0];
            string direction = segments.Length > 1 ? segments[1] : "desc";

            PropertyInfo propertyInfo;
            Expression propertyExpression;
            var parameter = Expression.Parameter(typeof(T), "s");

            if (column == "Actions") // If sorting by the Actions column, exclude it from sorting
            {
                propertyInfo = typeof(object).GetProperty("GetType"); // Use a dummy property to avoid sorting by Actions
                propertyExpression = Expression.Property(Expression.Constant(new object()), propertyInfo);
            }
            else // Otherwise, sort by the specified column
            {
                propertyInfo = typeof(T).GetProperty(column);
                propertyExpression = Expression.Property(parameter, propertyInfo);
            }

            var lambda = Expression.Lambda(propertyExpression, parameter);

            string methodName = direction == "desc" ? "OrderByDescending" : "OrderBy";

            var result = typeof(Queryable).GetMethods()
                .Single(method => method.Name == methodName &&
                                   method.IsGenericMethodDefinition &&
                                   method.GetGenericArguments().Length == 2 &&
                                   method.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), propertyInfo.PropertyType)
                .Invoke(null, new object[] { list, lambda });

            return (IQueryable<T>)result;
        }

        public static List<ColumnHeader> GetColumnHeaders(ColumnHeader[] DefaultColumnHeaders, string sort)
        {
            List<ColumnHeader> headers = new List<ColumnHeader>();
            foreach (var header in DefaultColumnHeaders)
            {
                if (!string.IsNullOrEmpty(header.OrderAction))
                {
                    header.OrderAction = (sort == $"{header.Key}-asc") ? $"{header.Key}-desc" : $"{header.Key}-asc";
                }
                headers.Add(header);
            }
            return headers;
        }
    }

    public class ClassListConfig
    {
        public static readonly List<string> TableColumns = new List<string>() {
                nameof(ClassHubViewModel.Name),
                nameof(ClassHubViewModel.IsActive),
                nameof(ClassHubViewModel.Actions)
            };

        //searchMessage = the placeholder for the search bar, from here, you can set different placeholder for different listconfig
        public static string SearchMessage = $"{Resource.Search}...";
        public static IQueryable<ClassHubViewModel> PerformSearch(IQueryable<ClassHubViewModel> list, string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                //perform search
                list = list.Where(s => s.Name.Contains(search) || s.IsActive.Contains(search));
            }
            return list;
        }

        public static string DefaultSortOrder = $"{nameof(ClassHubViewModel.Name)}-asc";
        public static int? DefaultPageSize = 10;

        public static readonly ColumnHeader[] DefaultColumnHeaders = ListUtil.GenerateDefaultColumnHeaders<ClassHubViewModel>(DefaultSortOrder, TableColumns);

        public static IQueryable<ClassHubViewModel> PerformSort(IQueryable<ClassHubViewModel> list, string sort)
        {
            var result = ListUtil.PerformSort<ClassHubViewModel>(list, DefaultSortOrder, sort);
            return (IQueryable<ClassHubViewModel>)result;
        }

    }

    public class StudentInClassListConfig
    {
        public static readonly List<string> TableColumns = new List<string>() {
                nameof(StudentViewModel.FullName)
            };

        //searchMessage = the placeholder for the search bar, from here, you can set different placeholder for different listconfig
        public static string SearchMessage = $"{Resource.Search}...";
        public static IQueryable<StudentViewModel> PerformSearch(IQueryable<StudentViewModel> list, string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                //perform search
                list = list.Where(s => s.FullName.Contains(search));
            }
            return list;
        }

        public static string DefaultSortOrder = $"{nameof(StudentViewModel.FullName)}-asc";
        public static int? DefaultPageSize = 10;

        public static readonly ColumnHeader[] DefaultColumnHeaders = ListUtil.GenerateDefaultColumnHeaders<StudentViewModel>(DefaultSortOrder, TableColumns);

        public static IQueryable<StudentViewModel> PerformSort(IQueryable<StudentViewModel> list, string sort)
        {
            var result = ListUtil.PerformSort<StudentViewModel>(list, DefaultSortOrder, sort);
            return (IQueryable<StudentViewModel>)result;
        }

    }

    public class ExamListConfig
    {
        public static readonly List<string> TableColumns = new List<string>() {
                nameof(ExamViewModel.Name),
                nameof(ExamViewModel.StartDate),
                nameof(ExamViewModel.EndDate),
                nameof(ExamViewModel.Duration),
                nameof(ExamViewModel.TotalQuestions),
                nameof(ExamViewModel.TotalMark),
                nameof(ExamViewModel.ExamStatus),
                nameof(ExamViewModel.CreatedOn),
                nameof(ExamViewModel.Actions)
            };

        //searchMessage = the placeholder for the search bar, from here, you can set different placeholder for different listconfig
        public static string SearchMessage = $"{Resource.Search}...";
        public static IQueryable<ExamViewModel> PerformSearch(IQueryable<ExamViewModel> list, string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                //perform search (Date-Time fields are not able to be searched because of different time zones for every user. We will need a date time picker in view.cshtml for searching Date Time fields; this feature will be added in a future update.)
                list = list.Where(s => s.Name.Contains(search) || s.Duration.ToString().Contains(search)
                || s.TotalQuestions.ToString().Contains(search) || s.TotalMark.ToString().Contains(search)
                || s.ExamStatus.Contains(search));
            }
            return list;
        }

        public static string DefaultSortOrder = $"{nameof(ExamViewModel.CreatedOn)}-desc";
        public static int? DefaultPageSize = 10;

        public static readonly ColumnHeader[] DefaultColumnHeaders = ListUtil.GenerateDefaultColumnHeaders<ExamViewModel>(DefaultSortOrder, TableColumns);

        public static IQueryable<ExamViewModel> PerformSort(IQueryable<ExamViewModel> list, string sort)
        {
            var result = ListUtil.PerformSort<ExamViewModel>(list, DefaultSortOrder, sort);
            return (IQueryable<ExamViewModel>)result;
        }

    }

    public class AddQuestionInExamListConfig
    {
        public static readonly List<string> TableColumns = new List<string>() {
                nameof(QuestionViewModel.QuestionTitle),
                nameof(QuestionViewModel.SubjectName),
                nameof(QuestionViewModel.QuestionTypeName),
                nameof(QuestionViewModel.CreatedOn),
                nameof(QuestionViewModel.AnswerSaved)
            };

        //searchMessage = the placeholder for the search bar, from here, you can set different placeholder for different listconfig
        public static string SearchMessage = $"{Resource.Search}...";
        public static IQueryable<QuestionViewModel> PerformSearch(IQueryable<QuestionViewModel> list, string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                //perform search (Date-Time fields are not able to be searched because of different time zones for every user. We will need a date time picker in view.cshtml for searching Date Time fields; this feature will be added in a future update.)
                list = list.Where(s => s.QuestionTitle.Contains(search) || s.SubjectName.Contains(search) || s.QuestionTypeName.Contains(search)
                 || (search == "Yes" && s.AnswerSaved == true) || (search == "No" && s.AnswerSaved == false));
            }
            return list;
        }

        public static string DefaultSortOrder = $"{nameof(QuestionViewModel.SubjectName)}-asc";
        public static int? DefaultPageSize = -1; //-1 = show all

        public static readonly ColumnHeader[] DefaultColumnHeaders = ListUtil.GenerateDefaultColumnHeaders<QuestionViewModel>(DefaultSortOrder, TableColumns);

        public static IQueryable<QuestionViewModel> PerformSort(IQueryable<QuestionViewModel> list, string sort)
        {
            var result = ListUtil.PerformSort<QuestionViewModel>(list, DefaultSortOrder, sort);
            return (IQueryable<QuestionViewModel>)result;
        }

    }

    public class LoginHistoryListConfig
    {
        public static readonly List<string> TableColumns = new List<string>() {
                nameof(LoginHistoryViewModel.Username),
                nameof(LoginHistoryViewModel.FullName),
                nameof(LoginHistoryViewModel.LoginDateTime)
            };

        //searchMessage = the placeholder for the search bar, from here, you can set different placeholder for different listconfig
        public static string SearchMessage = $"{Resource.Search}...";
        public static IQueryable<LoginHistoryViewModel> PerformSearch(IQueryable<LoginHistoryViewModel> list, string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                //perform search (Date-Time fields are not able to be searched because of different time zones for every user. We will need a date time picker in view.cshtml for searching Date Time fields; this feature will be added in a future update.)
                list = list.Where(s => s.Username.Contains(search) || s.FullName.Contains(search));
            }
            return list;
        }

        public static string DefaultSortOrder = $"{nameof(LoginHistoryViewModel.LoginDateTime)}-desc";
        public static int? DefaultPageSize = 10;

        public static readonly ColumnHeader[] DefaultColumnHeaders = ListUtil.GenerateDefaultColumnHeaders<LoginHistoryViewModel>(DefaultSortOrder, TableColumns);

        public static IQueryable<LoginHistoryViewModel> PerformSort(IQueryable<LoginHistoryViewModel> list, string sort)
        {
            var result = ListUtil.PerformSort<LoginHistoryViewModel>(list, DefaultSortOrder, sort);
            return (IQueryable<LoginHistoryViewModel>)result;
        }

    }

    public class QuestionListConfig
    {
        public static readonly List<string> TableColumns = new List<string>() {
                nameof(QuestionViewModel.QuestionTitle),
                nameof(QuestionViewModel.SubjectName),
                nameof(QuestionViewModel.QuestionTypeName),
                nameof(QuestionViewModel.CreatedOn),
                nameof(QuestionViewModel.AnswerSaved),
                nameof(QuestionViewModel.IsActive),
                nameof(QuestionViewModel.Actions)
            };

        //searchMessage = the placeholder for the search bar, from here, you can set different placeholder for different listconfig
        public static string SearchMessage = $"{Resource.Search}...";
        public static IQueryable<QuestionViewModel> PerformSearch(IQueryable<QuestionViewModel> list, string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                //perform search (Date-Time fields are not able to be searched because of different time zones for every user. We will need a date time picker in view.cshtml for searching Date Time fields; this feature will be added in a future update.)
                list = list.Where(s => s.QuestionTitle.Contains(search) || s.SubjectName.Contains(search) || s.QuestionTypeName.Contains(search)
                || (search == "Yes" && s.AnswerSaved == true) || (search == "No" && s.AnswerSaved == false) || s.IsActive.Contains(search));
            }
            return list;
        }

        public static string DefaultSortOrder = $"{nameof(QuestionViewModel.CreatedOn)}-desc";
        public static int? DefaultPageSize = 10;

        public static readonly ColumnHeader[] DefaultColumnHeaders = ListUtil.GenerateDefaultColumnHeaders<QuestionViewModel>(DefaultSortOrder, TableColumns);

        public static IQueryable<QuestionViewModel> PerformSort(IQueryable<QuestionViewModel> list, string sort)
        {
            var result = ListUtil.PerformSort<QuestionViewModel>(list, DefaultSortOrder, sort);
            return (IQueryable<QuestionViewModel>)result;
        }

    }

    public class ExamResultListConfig
    {
        public static readonly List<string> TableColumns = new List<string>() {
                nameof(ExamResultViewModel.ExamName),
                nameof(ExamResultViewModel.MarksToPass),
                nameof(ExamResultViewModel.StudentPassed),
                nameof(ExamResultViewModel.StudentFailed),
                nameof(ExamResultViewModel.StudentNotStarted),
                nameof(ExamResultViewModel.StartDate)
            };

        //searchMessage = the placeholder for the search bar, from here, you can set different placeholder for different listconfig
        public static string SearchMessage = $"{Resource.Search}...";
        public static IQueryable<ExamResultViewModel> PerformSearch(IQueryable<ExamResultViewModel> list, string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                //perform search
                list = list.Where(s => s.ExamName.Contains(search) || s.MarksToPass.ToString().Contains(search) || s.StudentPassed.ToString().Contains(search)
                || s.StudentFailed.ToString().Contains(search) || s.StudentNotStarted.ToString().Contains(search));
            }
            return list;
        }

        public static string DefaultSortOrder = $"{nameof(ExamResultViewModel.StartDate)}-desc";
        public static int? DefaultPageSize = 10;

        public static readonly ColumnHeader[] DefaultColumnHeaders = ListUtil.GenerateDefaultColumnHeaders<ExamResultViewModel>(DefaultSortOrder, TableColumns);

        public static IQueryable<ExamResultViewModel> PerformSort(IQueryable<ExamResultViewModel> list, string sort)
        {
            var result = ListUtil.PerformSort<ExamResultViewModel>(list, DefaultSortOrder, sort);
            return result;
        }

    }

    public class ResultByExamListConfig
    {
        public static readonly List<string> TableColumns = new List<string>() {
                nameof(ResultViewModel.StudentName),
                nameof(ResultViewModel.TotalScore),
                nameof(ResultViewModel.Passed),
                nameof(ResultViewModel.StudentClassNameList)
            };

        //searchMessage = the placeholder for the search bar, from here, you can set different placeholder for different listconfig
        public static string SearchMessage = $"{Resource.Search}...";
        public static IQueryable<ResultViewModel> PerformSearch(IQueryable<ResultViewModel> list, string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                //perform search
                list = list.Where(s => s.StudentName.Contains(search) || s.TotalScore.ToString().Contains(search) || s.StudentClassNameList.Any(n => n.Contains(search))
                || (search == "Yes" && s.Passed == true) || (search == "No" && s.Passed == false));
            }
            return list;
        }

        public static string DefaultSortOrder = $"{nameof(ResultViewModel.StudentName)}-asc";
        public static int? DefaultPageSize = 10;

        public static readonly ColumnHeader[] DefaultColumnHeaders = ListUtil.GenerateDefaultColumnHeaders<ResultViewModel>(DefaultSortOrder, TableColumns);

        public static IQueryable<ResultViewModel> PerformSort(IQueryable<ResultViewModel> list, string sort)
        {
            var result = ListUtil.PerformSort<ResultViewModel>(list, DefaultSortOrder, sort);
            return result;
        }

    }

    public class UpcomingCurrentPastExamListConfig
    {
        public static readonly List<string> TableColumns = new List<string>() {
                nameof(ExamViewModel.Name),
                nameof(ExamViewModel.StartDate),
                nameof(ExamViewModel.EndDate),
                nameof(ExamViewModel.Duration),
                nameof(ExamViewModel.Result),
                nameof(ExamViewModel.TotalQuestions),
                nameof(ExamViewModel.Actions)
            };

        //searchMessage = the placeholder for the search bar, from here, you can set different placeholder for different listconfig
        public static string SearchMessage = $"{Resource.Search}...";
        public static IQueryable<ExamViewModel> PerformSearch(IQueryable<ExamViewModel> list, string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                //perform search (Date-Time fields are not able to be searched because of different time zones for every user. We will need a date time picker in view.cshtml for searching Date Time fields; this feature will be added in a future update.)
                list = list.Where(s => s.Name.Contains(search) || s.Duration.ToString().Contains(search) || s.TotalQuestions.ToString().Contains(search)
                || s.Result.ToString().Contains(search));
            }
            return list;
        }

        public static string DefaultSortOrder = $"{nameof(ExamViewModel.StartDate)}-desc";
        public static int? DefaultPageSize = 10;

        public static readonly ColumnHeader[] DefaultColumnHeaders = ListUtil.GenerateDefaultColumnHeaders<ExamViewModel>(DefaultSortOrder, TableColumns);

        public static IQueryable<ExamViewModel> PerformSort(IQueryable<ExamViewModel> list, string sort)
        {
            var result = ListUtil.PerformSort<ExamViewModel>(list, DefaultSortOrder, sort);
            return result;
        }

    }

    public class SubjectListConfig
    {
        public static readonly List<string> TableColumns = new List<string>() {
                nameof(SubjectViewModel.Name),
                nameof(SubjectViewModel.Actions)
            };

        //searchMessage = the placeholder for the search bar, from here, you can set different placeholder for different listconfig
        public static string SearchMessage = $"{Resource.Search}...";
        public static IQueryable<SubjectViewModel> PerformSearch(IQueryable<SubjectViewModel> list, string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                //perform search
                list = list.Where(s => s.Name.Contains(search));
            }
            return list;
        }

        public static string DefaultSortOrder = $"{nameof(SubjectViewModel.Name)}-asc";
        public static int? DefaultPageSize = 10;

        public static readonly ColumnHeader[] DefaultColumnHeaders = ListUtil.GenerateDefaultColumnHeaders<SubjectViewModel>(DefaultSortOrder, TableColumns);

        public static IQueryable<SubjectViewModel> PerformSort(IQueryable<SubjectViewModel> list, string sort)
        {
            var result = ListUtil.PerformSort<SubjectViewModel>(list, DefaultSortOrder, sort);
            return result;
        }

    }

    public class UserListConfig
    {
        public static readonly List<string> TableColumns = new List<string>() {
                nameof(UserProfileViewModel.Username),
                nameof(UserProfileViewModel.FullName),
                nameof(UserProfileViewModel.EmailAddress),
                nameof(UserProfileViewModel.UserStatusName),
                nameof(UserProfileViewModel.UserRoleName),
                nameof(UserProfileViewModel.PhoneNumber),
                nameof(UserProfileViewModel.GovernateName),
                nameof(UserProfileViewModel.Address),
                nameof(UserProfileViewModel.CreatedOn),
                nameof(UserProfileViewModel.Actions)
            };

        //searchMessage = the placeholder for the search bar, from here, you can set different placeholder for different listconfig
        public static string SearchMessage = $"{Resource.Search}...";
        public static IQueryable<UserProfileViewModel> PerformSearch(IQueryable<UserProfileViewModel> list, string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                //perform search (Date-Time fields are not able to be searched because of different time zones for every user. We will need a date time picker in view.cshtml for searching Date Time fields; this feature will be added in a future update.)
                list = list.Where(s => s.Username.Contains(search) || s.FullName.Contains(search) || s.UserStatusName.Contains(search)
                || s.UserRoleName.Contains(search) || s.PhoneNumber.Contains(search) || s.GovernateName.Contains(search) || s.Address.Contains(search));
            }
            return list;
        }

        public static string DefaultSortOrder = $"{nameof(UserProfileViewModel.CreatedOn)}-desc";
        public static int? DefaultPageSize = 10;

        public static readonly ColumnHeader[] DefaultColumnHeaders = ListUtil.GenerateDefaultColumnHeaders<UserProfileViewModel>(DefaultSortOrder, TableColumns);

        public static IQueryable<UserProfileViewModel> PerformSort(IQueryable<UserProfileViewModel> list, string sort)
        {
            var result = ListUtil.PerformSort<UserProfileViewModel>(list, DefaultSortOrder, sort);
            return result;
        }

    }

    public class UserAttachmentListConfig
    {
        public static readonly List<string> TableColumns = new List<string>() {
                nameof(UserAttachmentViewModel.FileName),
                nameof(UserAttachmentViewModel.AttachmentTypeName),
                nameof(UserAttachmentViewModel.UploadedOn),
                nameof(UserAttachmentViewModel.UploadedBy),
                nameof(UserAttachmentViewModel.Actions)
            };

        //searchMessage = the placeholder for the search bar, from here, you can set different placeholder for different listconfig
        public static string SearchMessage = $"{Resource.Search}...";
        public static IQueryable<UserAttachmentViewModel> PerformSearch(IQueryable<UserAttachmentViewModel> list, string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                //perform search (Date-Time fields are not able to be searched because of different time zones for every user. We will need a date time picker in view.cshtml for searching Date Time fields; this feature will be added in a future update.)
                list = list.Where(s => s.FileName.Contains(search) || s.AttachmentTypeName.Contains(search) || s.UploadedBy.Contains(search));
            }
            return list;
        }

        public static string DefaultSortOrder = $"{nameof(UserAttachmentViewModel.UploadedOn)}-desc";
        public static int? DefaultPageSize = 10;

        public static readonly ColumnHeader[] DefaultColumnHeaders = ListUtil.GenerateDefaultColumnHeaders<UserAttachmentViewModel>(DefaultSortOrder, TableColumns);

        public static IQueryable<UserAttachmentViewModel> PerformSort(IQueryable<UserAttachmentViewModel> list, string sort)
        {
            var result = ListUtil.PerformSort<UserAttachmentViewModel>(list, DefaultSortOrder, sort);
            return result;
        }

    }

    public class UserAttachmentTypeListConfig
    {
        public static readonly List<string> TableColumns = new List<string>() {
                nameof(GlobalOptionSetViewModel.OptionOrder),
                nameof(GlobalOptionSetViewModel.DisplayName),
                nameof(GlobalOptionSetViewModel.Actions)
            };

        //searchMessage = the placeholder for the search bar, from here, you can set different placeholder for different listconfig
        public static string SearchMessage = $"{Resource.Search}...";
        public static IQueryable<GlobalOptionSetViewModel> PerformSearch(IQueryable<GlobalOptionSetViewModel> list, string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                //perform search
                list = list.Where(s => s.OptionOrder.ToString().Contains(search) || s.DisplayName.Contains(search));
            }
            return list;
        }

        public static string DefaultSortOrder = $"{nameof(GlobalOptionSetViewModel.OptionOrder)}-asc";
        public static int? DefaultPageSize = 10;

        public static readonly ColumnHeader[] DefaultColumnHeaders = ListUtil.GenerateDefaultColumnHeaders<GlobalOptionSetViewModel>(DefaultSortOrder, TableColumns);

        public static IQueryable<GlobalOptionSetViewModel> PerformSort(IQueryable<GlobalOptionSetViewModel> list, string sort)
        {
            var result = ListUtil.PerformSort<GlobalOptionSetViewModel>(list, DefaultSortOrder, sort);
            return result;
        }

    }

    public class UserStatusListConfig
    {
        public static readonly List<string> TableColumns = new List<string>() {
                nameof(GlobalOptionSetViewModel.OptionOrder),
                nameof(GlobalOptionSetViewModel.DisplayName),
                nameof(GlobalOptionSetViewModel.Actions)
            };

        //searchMessage = the placeholder for the search bar, from here, you can set different placeholder for different listconfig
        public static string SearchMessage = $"{Resource.Search}...";
        public static IQueryable<GlobalOptionSetViewModel> PerformSearch(IQueryable<GlobalOptionSetViewModel> list, string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                //perform search
                list = list.Where(s => s.OptionOrder.ToString().Contains(search) || s.DisplayName.Contains(search));
            }
            return list;
        }

        public static string DefaultSortOrder = $"{nameof(GlobalOptionSetViewModel.OptionOrder)}-asc";
        public static int? DefaultPageSize = 10;

        public static readonly ColumnHeader[] DefaultColumnHeaders = ListUtil.GenerateDefaultColumnHeaders<GlobalOptionSetViewModel>(DefaultSortOrder, TableColumns);

        public static IQueryable<GlobalOptionSetViewModel> PerformSort(IQueryable<GlobalOptionSetViewModel> list, string sort)
        {
            var result = ListUtil.PerformSort<GlobalOptionSetViewModel>(list, DefaultSortOrder, sort);
            return result;
        }

    }

}
